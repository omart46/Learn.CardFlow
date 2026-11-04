import { CommonModule } from '@angular/common';
import { Component, ChangeDetectionStrategy, ChangeDetectorRef, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { PermissionService } from '@abp/ng.core';
import { finalize, forkJoin } from 'rxjs';
import { CardRequest, CardRequestsService } from './card-requests.service';

@Component({
  selector: 'app-card-requests',
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './card-requests.component.html',
  styleUrls: ['./card-requests.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
})
export class CardRequestsComponent {
  private readonly api = inject(CardRequestsService);
  private readonly permissions = inject(PermissionService);
  private readonly fb = inject(FormBuilder);
  private readonly changeDetector = inject(ChangeDetectorRef);
  readonly pageSize = 10;
  readonly statuses = ['Draft', 'Under Review', 'Approved', 'Rejected'];
  readonly badgeClasses = ['bg-secondary', 'bg-info text-dark', 'bg-success', 'bg-danger'];
  items: CardRequest[] = [];
  totalCount = 0;
  page = 0;
  loading = false;
  saving = false;
  pendingId: string | null = null;
  showCreate = false;
  editing: CardRequest | null = null;
  view: 'all' | 'mine' | 'attention' = 'all';
  attentionCount = 0;
  readonly filtersForm = this.fb.nonNullable.group({
    filter: ['', Validators.maxLength(128)],
    status: [''], createdFrom: [''], createdTo: [''],
  });
  appliedFilters: { filter?: string; status?: number; createdFrom?: string; createdTo?: string } = {};
  filterError = '';
  get hasFilters() { return Object.keys(this.appliedFilters).length > 0; }

  applyFilters() {
    if (this.loading || this.saving || this.pendingId) return;
    const value = this.filtersForm.getRawValue();
    this.filterError = '';
    if (this.filtersForm.invalid) { this.filterError = 'Search must be 128 characters or fewer.'; return; }
    if (value.createdFrom && value.createdTo && value.createdFrom > value.createdTo) {
      this.filterError = 'The start date must be on or before the end date.'; return;
    }
    this.appliedFilters = {};
    if (value.filter.trim()) this.appliedFilters.filter = value.filter.trim();
    if (value.status !== '') this.appliedFilters.status = Number(value.status);
    if (value.createdFrom) this.appliedFilters.createdFrom = value.createdFrom;
    if (value.createdTo) this.appliedFilters.createdTo = value.createdTo;
    this.page = 0; this.load();
  }

  clearFilters() {
    if (this.loading || this.saving || this.pendingId) return;
    this.filtersForm.reset(); this.appliedFilters = {}; this.filterError = '';
    this.page = 0; this.load();
  }
  rejecting: CardRequest | null = null;
  readonly rejectionForm = this.fb.nonNullable.group({
    reason: ['', [Validators.required, Validators.maxLength(500), Validators.pattern(/\S/)]],
  });
  notice = '';
  loadError = false;
  readonly form = this.fb.nonNullable.group({
    employeeName: ['', [Validators.required, Validators.maxLength(128), Validators.pattern(/\S/)]],
    employeeNumber: ['', [Validators.required, Validators.maxLength(32), Validators.pattern(/\S/)]],
  });

  constructor() { this.load(); }

  can(permission: string) {
    return this.permissions.getGrantedPolicy(`CardFlow.CardRequests.${permission}`);
  }

  load() {
    this.loading = true;
    this.loadError = false;
    forkJoin({
      list: this.api.getList(this.page * this.pageSize, this.pageSize, this.view !== 'all', this.view === 'attention', this.appliedFilters),
      attention: this.api.getMyAttentionCount(),
    })
      .pipe(finalize(() => { this.loading = false; this.changeDetector.markForCheck(); }))
      .subscribe({
        next: result => {
          this.items = result.list.items;
          this.totalCount = result.list.totalCount;
          this.attentionCount = result.attention;
          if (this.page > 0 && this.items.length === 0) { this.page--; this.load(); }
        },
        error: () => this.loadError = true,
      });
  }

  openCreate() {
    if (this.saving || this.pendingId) return;
    this.editing = null;
    this.rejecting = null;
    this.form.reset();
    this.showCreate = true;
    this.notice = '';
  }

  create() {
    if (this.saving || (this.editing ? !this.can('Edit') : !this.can('Create'))) return;
    this.form.markAllAsTouched();
    if (this.form.invalid) return;
    this.saving = true;
    const value = this.form.getRawValue();
    const body = { employeeName: value.employeeName.trim(), employeeNumber: value.employeeNumber.trim() };
    const wasCorrection = !!this.editing;
    const operation = this.editing ? this.api.correct(this.editing.id, body) : this.api.create(body);
    operation
      .pipe(finalize(() => { this.saving = false; this.changeDetector.markForCheck(); }))
      .subscribe({
        next: () => {
          this.showCreate = false;
          this.editing = null;
          this.notice = wasCorrection ? 'Corrections saved as Draft. Select Resubmit for Review when ready.' : 'Request created successfully.';
          this.page = 0;
          this.load();
        },
        error: () => {}, // ABP displays the server error; keep the form for correction.
      });
  }

  setView(view: 'all' | 'mine' | 'attention') {
    if (this.loading || this.saving || this.pendingId) return;
    this.view = view;
    this.page = 0;
    this.showCreate = false;
    this.editing = null;
    this.rejecting = null;
    this.load();
  }

  canCorrect(request: CardRequest) {
    return request.isMine && this.can('Edit') && (request.status === 3 || (request.status === 0 && request.needsResubmission));
  }

  openCorrection(request: CardRequest) {
    if (!this.canCorrect(request) || this.pendingId || this.saving) return;
    this.editing = request;
    this.showCreate = true;
    this.rejecting = null;
    this.notice = '';
    this.form.setValue({ employeeName: request.employeeName, employeeNumber: request.employeeNumber });
    this.form.markAsUntouched();
  }

  resubmit(request: CardRequest) {
    if (this.pendingId || this.saving || !request.isMine || !this.can('Edit') || request.status !== 0 || !request.needsResubmission) return;
    this.pendingId = request.id;
    this.api.resubmit(request.id)
      .pipe(finalize(() => { this.pendingId = null; this.changeDetector.markForCheck(); }))
      .subscribe({
        next: () => { this.notice = 'Request resubmitted for review.'; this.load(); },
        error: () => this.load(),
      });
  }

  openReject(request: CardRequest) {
    if (this.pendingId || request.status !== 1 || !this.can('Decide')) return;
    this.showCreate = false;
    this.editing = null;
    this.rejectionForm.reset();
    this.rejecting = request;
    this.notice = '';
  }

  confirmReject() {
    this.rejectionForm.markAllAsTouched();
    if (!this.rejecting || this.pendingId || this.rejectionForm.invalid || !this.can('Decide')) return;
    this.pendingId = this.rejecting.id;
    this.api.reject(this.rejecting.id, this.rejectionForm.getRawValue().reason.trim())
      .pipe(finalize(() => { this.pendingId = null; this.changeDetector.markForCheck(); }))
      .subscribe({
        next: updated => {
          this.items = this.items.map(item => item.id === updated.id ? updated : item);
          this.rejecting = null;
          this.notice = 'Request rejected. The reason has been saved.';
          this.load();
        },
        error: () => {}, // Keep the reason so the supervisor can correct or retry the request.
      });
  }

  transition(request: CardRequest, action: 'start-review' | 'approve') {
    if (this.pendingId || this.loading) return;
    const allowed = action === 'start-review'
      ? request.status === 0 && !request.needsResubmission && this.can('StartReview')
      : request.status === 1 && this.can('Decide');
    if (!allowed) return;
    this.pendingId = request.id;
    this.notice = '';
    this.api.transition(request.id, action)
      .pipe(finalize(() => { this.pendingId = null; this.changeDetector.markForCheck(); }))
      .subscribe({
        next: updated => {
          this.items = this.items.map(item => item.id === updated.id ? updated : item);
          this.notice = `Request updated to ${this.statuses[updated.status]}.`;
          this.load();
        },
        error: () => this.load(),
      });
  }

  goToPage(page: number) {
    if (this.loading || this.pendingId || page < 0 || page * this.pageSize >= this.totalCount) return;
    this.page = page;
    this.load();
  }
}
