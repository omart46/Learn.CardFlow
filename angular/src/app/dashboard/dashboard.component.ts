import { CommonModule } from '@angular/common';
import { Component, ChangeDetectorRef, inject, OnDestroy } from '@angular/core';
import { RouterLink } from '@angular/router';
import { finalize, Subscription } from 'rxjs';
import { DashboardService, DashboardSummary } from './dashboard.service';
interface Metric { label: string; value: number; tone: string; }
interface DashboardSection { title: string; subtitle: string; route: string; icon: string; metrics: Metric[]; }
@Component({
  selector: 'app-dashboard', imports: [CommonModule, RouterLink],
  templateUrl: './dashboard.component.html', styleUrls: ['./dashboard.component.scss'],
})
export class DashboardComponent implements OnDestroy {
  private readonly api = inject(DashboardService);
  private readonly cd = inject(ChangeDetectorRef);
  private request?: Subscription;
  summary: DashboardSummary | null = null;
  sections: DashboardSection[] = [];
  loading = false;
  error = '';
  private readonly timer = setInterval(() => { if (!this.loading) this.load(); }, 30000);
  constructor() { this.load(); }
  ngOnDestroy() { clearInterval(this.timer); this.request?.unsubscribe(); }
  load() {
    if (this.loading) return;
    this.loading = true; this.error = '';
    this.request = this.api.summary().pipe(finalize(() => { this.loading = false; this.cd.markForCheck(); })).subscribe({
      next: summary => { this.summary = summary; this.sections = this.buildSections(summary); },
      error: () => this.error = 'Unable to update the dashboard. Try refreshing. Any figures below are from the last successful update.',
    });
  }
  private buildSections(data: DashboardSummary): DashboardSection[] {
    const sections: DashboardSection[] = [];
    if (data.cardRequests) sections.push({ title: 'Enrollment', subtitle: 'Employee card requests', route: '/card-requests', icon: 'fa-id-card', metrics: [
      { label: 'Draft', value: data.cardRequests.draft, tone: 'neutral' },
      { label: 'Under review', value: data.cardRequests.underReview, tone: 'blue' },
      { label: 'Approved', value: data.cardRequests.approved, tone: 'green' },
      { label: 'Rejected', value: data.cardRequests.rejected, tone: 'red' },
    ] });
    if (data.production) sections.push({ title: 'Production', subtitle: 'Batch progress', route: '/production-batches', icon: 'fa-layer-group', metrics: [
      { label: 'Draft batches', value: data.production.draft, tone: 'neutral' },
      { label: 'In production', value: data.production.inProduction, tone: 'blue' },
      { label: 'Completed batches', value: data.production.completed, tone: 'green' },
    ] });
    if (data.quality) sections.push({ title: 'Quality', subtitle: 'Inspection results', route: '/quality-checks', icon: 'fa-clipboard-check', metrics: [
      { label: 'Pending inspection', value: data.quality.pending, tone: 'amber' },
      { label: 'Passed', value: data.quality.passed, tone: 'green' },
      { label: 'Failed', value: data.quality.failed, tone: 'red' },
    ] });
    if (data.delivery) sections.push({ title: 'Delivery', subtitle: 'Card handover', route: '/deliveries', icon: 'fa-hand-holding', metrics: [
      { label: 'Ready for delivery', value: data.delivery.readyForDelivery, tone: 'amber' },
      { label: 'Delivered', value: data.delivery.delivered, tone: 'green' },
    ] });
    return sections;
  }
}
