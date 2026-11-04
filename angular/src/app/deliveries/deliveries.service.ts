import { Injectable, inject } from '@angular/core';
import { RestService } from '@abp/ng.core';
export interface ReadyDelivery { id: string; employeeCardRequestId: string; employeeName: string; employeeNumber: string; batchName: string; }
export interface Delivery { id: string; employeeCardRequestId: string; cardQualityCheckId: string; employeeName: string; employeeNumber: string; batchName: string; recipientName: string; deliveredAt: string; deliveredById: string; }
export interface Page<T> { items: T[]; totalCount: number; }
@Injectable({ providedIn: 'root' })
export class DeliveriesService {
  private readonly rest = inject(RestService);
  private readonly url = '/api/app/card-delivery';
  ready(page: number) {
    return this.rest.request<void, Page<ReadyDelivery>>({ method: 'GET', url: `${this.url}/ready-cards`, params: { skipCount: page * 10, maxResultCount: 10 } }, { apiName: 'default' });
  }
  list(page: number) {
    return this.rest.request<void, Page<Delivery>>({ method: 'GET', url: this.url, params: { skipCount: page * 10, maxResultCount: 10 } }, { apiName: 'default' });
  }
  record(cardQualityCheckId: string, recipientName: string) {
    return this.rest.request<{ cardQualityCheckId: string; recipientName: string }, Delivery>({ method: 'POST', url: this.url, body: { cardQualityCheckId, recipientName } }, { apiName: 'default' });
  }
}
