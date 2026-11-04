import { CommonModule } from '@angular/common';
import { Component, ChangeDetectorRef, inject, OnDestroy, ViewChild, ElementRef, afterNextRender, Injector } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { PermissionService } from '@abp/ng.core';
import { finalize, forkJoin, Observable } from 'rxjs';
import { QualityCheck, QualityChecksService, ReadyCard } from './quality-checks.service';
@Component({
  selector: 'app-quality-checks',
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './quality-checks.component.html',
  styleUrls: ['../card-requests/card-requests.component.scss', './quality-checks.component.scss'],
})
export class QualityChecksComponent implements OnDestroy {
  private readonly api = inject(QualityChecksService);
  private readonly permissions = inject(PermissionService);
  private readonly cd = inject(ChangeDetectorRef);
  private readonly fb = inject(FormBuilder);
  private readonly injector = inject(Injector);
  @ViewChild('failurePanel') private failurePanel?: ElementRef<HTMLElement>;
  readonly statuses = ['Pending', 'Passed', 'Failed'];
  ready: ReadyCard[] = [];
  checks: QualityCheck[] = [];
  readyTotal = 0;
  total = 0;
  page = 0;
  readyPage = 0;
  loading = false;
  busy = false;
  error = '';
  notice = '';
  failing: QualityCheck | null = null;
  readonly form = this.fb.nonNullable.group({ reason: ['', [Validators.required, Validators.maxLength(500), Validators.pattern(/\S/)]] });
  private readonly timer = setInterval(() => {
    if (!this.loading && !this.busy && !this.failing) this.load();
  }, 15000);
  constructor() { this.load(); }
  ngOnDestroy() { clearInterval(this.timer); }
  can(action: string) { return this.permissions.getGrantedPolicy(`CardFlow.QualityChecks.${action}`); }
  load() {
    if (this.loading || this.busy) return;
    this.loading = true;
    this.error = '';
    forkJoin({ ready: this.api.ready(this.readyPage), checks: this.api.list(this.page) })
      .pipe(finalize(() => { this.loading = false; this.cd.markForCheck(); })).subscribe({
        next: result => {
          this.ready = result.ready.items; this.readyTotal = result.ready.totalCount;
          this.checks = result.checks.items; this.total = result.checks.totalCount;
          if (this.readyPage > 0 && !this.ready.length) this.readyPage--;
          if (this.page > 0 && !this.checks.length) this.page--;
        },
        error: () => this.error = 'Unable to load quality checks. Check your access and try again.',
      });
  }
  create(card: ReadyCard) { this.perform(this.api.create(card.id), `Inspection started for ${card.employeeName}.`); }
  pass(check: QualityCheck) { this.perform(this.api.pass(check.id), `Quality check passed for ${check.employeeName}.`); }
  openFailure(check: QualityCheck) {
    this.failing = check; this.form.reset(); this.notice = '';
    afterNextRender(() => {
      this.failurePanel?.nativeElement.scrollIntoView({ behavior: 'smooth', block: 'start' });
      this.failurePanel?.nativeElement.querySelector('textarea')?.focus({ preventScroll: true });
    }, { injector: this.injector });
  }
  fail() {
    if (!this.failing || this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.perform(this.api.fail(this.failing.id, this.form.getRawValue().reason.trim()), 'Quality check failed. The reason has been recorded.');
  }
  private perform(operation: Observable<QualityCheck>, message: string) {
    if (this.busy || this.loading) return;
    this.busy = true;
    this.error = ''; this.notice = '';
    operation.pipe(finalize(() => { this.busy = false; this.cd.markForCheck(); })).subscribe({
      next: () => { this.busy = false; this.failing = null; this.form.reset(); this.notice = message; this.load(); },
      error: () => this.error = 'The action could not be completed. Review the error message and refresh before retrying.',
    });
  }
}
