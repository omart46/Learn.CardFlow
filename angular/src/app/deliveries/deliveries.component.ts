import { CommonModule } from '@angular/common';
import { Component, ChangeDetectorRef, inject, OnDestroy, ViewChild, ElementRef, afterNextRender, Injector } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { PermissionService } from '@abp/ng.core';
import { finalize, forkJoin } from 'rxjs';
import { DeliveriesService, Delivery, ReadyDelivery } from './deliveries.service';
@Component({
  selector: 'app-deliveries', imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './deliveries.component.html',
  styleUrls: ['../card-requests/card-requests.component.scss', './deliveries.component.scss'],
})
export class DeliveriesComponent implements OnDestroy {
  private readonly api = inject(DeliveriesService);
  private readonly permissions = inject(PermissionService);
  private readonly fb = inject(FormBuilder);
  private readonly cd = inject(ChangeDetectorRef);
  private readonly injector = inject(Injector);
  @ViewChild('receiptPanel') private receiptPanel?: ElementRef<HTMLElement>;
  ready: ReadyDelivery[] = [];
  items: Delivery[] = [];
  readyTotal = 0;
  total = 0;
  page = 0;
  readyPage = 0;
  loading = false;
  busy = false;
  error = '';
  notice = '';
  selected: ReadyDelivery | null = null;
  receipt: Delivery | null = null;
  readonly form = this.fb.nonNullable.group({
    recipientName: ['', [Validators.required, Validators.maxLength(128), Validators.pattern(/\S/)]],
    confirmed: [false, Validators.requiredTrue],
  });
  private readonly timer = setInterval(() => {
    if (!this.loading && !this.busy && !this.selected) this.load();
  }, 15000);
  constructor() { this.load(); }
  ngOnDestroy() { clearInterval(this.timer); }
  get canRecord() { return this.permissions.getGrantedPolicy('CardFlow.Deliveries.Record'); }
  load() {
    if (this.loading || this.busy) return;
    this.loading = true; this.error = '';
    let retryPage = false;
    forkJoin({ ready: this.api.ready(this.readyPage), list: this.api.list(this.page) })
      .pipe(finalize(() => {
        this.loading = false; this.cd.markForCheck();
        if (retryPage) this.load();
      })).subscribe({
        next: result => {
          this.ready = result.ready.items; this.readyTotal = result.ready.totalCount;
          this.items = result.list.items; this.total = result.list.totalCount;
          if (this.readyPage > 0 && !this.ready.length) { this.readyPage--; retryPage = true; }
          if (this.page > 0 && !this.items.length) { this.page--; retryPage = true; }
        },
        error: () => this.error = 'Unable to load deliveries. Check your access and refresh.',
      });
  }
  open(card: ReadyDelivery) {
    this.selected = card; this.notice = ''; this.receipt = null;
    this.form.reset({ recipientName: card.employeeName, confirmed: false });
    afterNextRender(() => {
      this.receiptPanel?.nativeElement.scrollIntoView({ behavior: 'smooth', block: 'start' });
      this.receiptPanel?.nativeElement.querySelector('input')?.focus({ preventScroll: true });
    }, { injector: this.injector });
  }
  record() {
    if (this.busy || this.loading || !this.selected || this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.busy = true; this.error = ''; this.notice = '';
    const input = this.form.getRawValue();
    this.api.record(this.selected.id, input.recipientName.trim())
      .pipe(finalize(() => { this.busy = false; this.cd.markForCheck(); })).subscribe({
        next: receipt => {
          this.receipt = receipt; this.selected = null; this.busy = false;
          this.notice = `Card delivered to ${receipt.recipientName}. The handover is recorded.`;
          this.page = 0; this.load();
        },
        error: () => this.error = 'Handover could not be recorded. Refresh to check whether this card was already delivered.',
      });
  }
}
