import { inject, Injectable } from '@angular/core';
import { RestService } from '@abp/ng.core';
export interface DashboardSummary {
  generatedAt: string;
  cardRequests: { draft: number; underReview: number; approved: number; rejected: number } | null;
  production: { draft: number; inProduction: number; completed: number } | null;
  quality: { pending: number; passed: number; failed: number } | null;
  delivery: { readyForDelivery: number; delivered: number } | null;
}
@Injectable({ providedIn: 'root' })
export class DashboardService {
  private readonly rest = inject(RestService);
  summary() {
    return this.rest.request<void, DashboardSummary>({ method: 'GET', url: '/api/app/dashboard/summary' }, { apiName: 'default' });
  }
}
