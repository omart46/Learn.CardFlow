import { Injectable, inject } from '@angular/core';
import { RestService } from '@abp/ng.core';
export interface ReadyCard { id: string; employeeName: string; employeeNumber: string; productionBatchId: string; batchName: string; }
export interface QualityCheck { id: string; employeeCardRequestId: string; productionBatchId: string; employeeName: string; employeeNumber: string; batchName: string; status: number; failureReason: string | null; creationTime: string; }
export interface Page<T> { items: T[]; totalCount: number; }
@Injectable({ providedIn: 'root' })
export class QualityChecksService {
  private readonly rest = inject(RestService);
  private readonly url = '/api/app/card-quality-check';
  list(page: number) {
    return this.rest.request<void, Page<QualityCheck>>({ method: 'GET', url: this.url, params: { skipCount: page * 10, maxResultCount: 10 } }, { apiName: 'default' });
  }
  ready(page: number) {
    return this.rest.request<void, Page<ReadyCard>>({ method: 'GET', url: `${this.url}/ready-requests`, params: { skipCount: page * 10, maxResultCount: 10 } }, { apiName: 'default' });
  }
  create(requestId: string) {
    return this.rest.request<void, QualityCheck>({ method: 'POST', url: this.url, params: { requestId } }, { apiName: 'default' });
  }
  pass(id: string) {
    return this.rest.request<void, QualityCheck>({ method: 'POST', url: `${this.url}/${id}/pass` }, { apiName: 'default' });
  }
  fail(id: string, reason: string) {
    return this.rest.request<{ reason: string }, QualityCheck>({ method: 'POST', url: `${this.url}/${id}/fail`, body: { reason } }, { apiName: 'default' });
  }
}
