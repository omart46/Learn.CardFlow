import { CommonModule } from '@angular/common';
import { Component, ChangeDetectorRef, ElementRef, inject, OnDestroy, ViewChild, afterNextRender, Injector } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { PermissionService } from '@abp/ng.core';
import { finalize } from 'rxjs';
import { Batch, BatchRequest, ProductionBatchesService } from './production-batches.service';

@Component({
  selector: 'app-production-batches',
  imports: [CommonModule, FormsModule],
  templateUrl: './production-batches.component.html',
  styleUrls: ['../card-requests/card-requests.component.scss'],
})
export class ProductionBatchesComponent implements OnDestroy {
  private readonly api = inject(ProductionBatchesService);
  private readonly permissions = inject(PermissionService);
  private readonly cd = inject(ChangeDetectorRef);
  private readonly injector = inject(Injector);
  @ViewChild('batchDetails') private batchDetails?: ElementRef<HTMLElement>;
  readonly statuses = ['Draft', 'In Production', 'Completed'];
  items: Batch[] = [];
  requests: BatchRequest[] = [];
  selected: Batch | null = null;
  total = 0;
  requestTotal = 0;
  page = 0;
  requestPage = 0;
  loading = false;
  detailLoading = false;
  busy = false;
  error = '';
  detailError = '';
  notice = '';
  name = '';
  ready: BatchRequest[] = [];
  readyTotal = 0;
  readyPage = 0;
  readyLoading = false;
  readyError = '';
  private readonly refreshTimer = setInterval(() => {
    if (!this.busy && !this.readyLoading) this.loadReady();
  }, 15000);
  showCreate = false;
  constructor() { this.load(); this.loadReady(); }
  ngOnDestroy() { clearInterval(this.refreshTimer); }
  loadReady() {
    this.readyLoading = true;
    this.readyError = '';
    this.api.ready(this.readyPage).pipe(finalize(() => { this.readyLoading = false; this.cd.markForCheck(); })).subscribe({
      next: result => {
        this.ready = result.items; this.readyTotal = result.totalCount;
        if (this.readyPage > 0 && !this.ready.length) { this.readyPage--; this.loadReady(); }
      },
      error: () => this.readyError = 'Unable to load approved requests. Please refresh.',
    });
  }
  can(action: string) { return this.permissions.getGrantedPolicy(`CardFlow.ProductionBatches.${action}`); }
  load() {
    this.loading = true;
    this.error = '';
    this.api.list(this.page).pipe(finalize(() => { this.loading = false; this.cd.markForCheck(); })).subscribe({
      next: result => { this.items = result.items; this.total = result.totalCount; },
      error: () => this.error = 'Unable to load batches. Please try again.',
    });
  }
  open(batch: Batch) {
    if (this.busy || this.detailLoading) return;
    this.selected = batch;
    this.requestPage = 0;
    this.loadRequests();
    afterNextRender(() => {
      this.batchDetails?.nativeElement.scrollIntoView({ behavior: 'smooth', block: 'start' });
      this.batchDetails?.nativeElement.focus({ preventScroll: true });
    }, { injector: this.injector });
  }
  loadRequests() {
    if (!this.selected) return;
    this.detailLoading = true;
    this.detailError = '';
    this.requests = [];
    this.api.requests(this.selected.id, this.requestPage)
      .pipe(finalize(() => { this.detailLoading = false; this.cd.markForCheck(); })).subscribe({
        next: result => { this.requests = result.items; this.requestTotal = result.totalCount; },
        error: () => this.detailError = 'Unable to load batch requests. Please try again.',
      });
  }
  create() {
    if (this.busy || !this.name.trim() || this.name.length > 128) return;
    this.busy = true;
    this.api.create(this.name.trim()).pipe(finalize(() => { this.busy = false; this.cd.markForCheck(); })).subscribe({
      next: batch => { this.showCreate = false; this.name = ''; this.page = 0; this.notice = 'Batch created.'; this.load(); this.selected = batch; this.requestPage = 0; this.loadRequests(); },
      error: () => { this.notice = ''; },
    });
  }
  changeRequest(id: string, remove = false) {
    if (!this.selected || this.busy || !id.trim()) return;
    this.busy = true;
    this.api.changeRequest(this.selected.id, id.trim(), remove)
      .pipe(finalize(() => { this.busy = false; this.cd.markForCheck(); })).subscribe({
        next: () => {
          this.selected = { ...this.selected!, requestCount: this.selected!.requestCount + (remove ? -1 : 1) };
          this.requestPage = 0;
          this.notice = remove ? 'Request removed from batch.' : 'Request added to batch.';
          this.load(); this.loadRequests(); this.loadReady();
        },
        error: () => { this.notice = ''; this.loadReady(); },
      });
  }
  transition(batch: Batch) {
    if (this.busy) return;
    this.busy = true;
    this.api.transition(batch.id, batch.status === 0 ? 'start-production' : 'complete-production')
      .pipe(finalize(() => { this.busy = false; this.cd.markForCheck(); })).subscribe({
        next: updated => { if (this.selected?.id === updated.id) this.selected = updated;
          this.notice = updated.status === 1 ? 'Production started.' : 'Production completed.'; this.load(); },
        error: () => { this.notice = ''; },
      });
  }
}
