import { Injectable, inject } from '@angular/core';
import { RestService } from '@abp/ng.core';

export interface Batch { id: string; name: string; status: number; requestCount: number; creationTime: string; }
export interface BatchRequest { id: string; employeeName: string; employeeNumber: string; status: number; }
export interface Page<T> { items: T[]; totalCount: number; }

@Injectable({ providedIn: 'root' })
export class ProductionBatchesService {
  private readonly rest = inject(RestService);
  private readonly url = '/api/app/production-batch';
  list(page: number) {
    return this.rest.request<void, Page<Batch>>({ method: 'GET', url: this.url,
      params: { skipCount: page * 10, maxResultCount: 10 } }, { apiName: 'default' });
  }
  ready(page: number) {
    return this.rest.request<void, Page<BatchRequest>>({ method: 'GET', url: `${this.url}/ready-requests`,
      params: { skipCount: page * 10, maxResultCount: 10 } }, { apiName: 'default' });
  }
  requests(id: string, page: number) {
    return this.rest.request<void, Page<BatchRequest>>({ method: 'GET', url: `${this.url}/${id}/requests`,
      params: { skipCount: page * 10, maxResultCount: 10 } }, { apiName: 'default' });
  }
  create(name: string) {
    return this.rest.request<{ name: string }, Batch>({ method: 'POST', url: this.url, body: { name } }, { apiName: 'default' });
  }
  changeRequest(id: string, requestId: string, remove = false) {
    return this.rest.request<void, void>({ method: remove ? 'DELETE' : 'POST',
      url: `${this.url}/${id}/request/${encodeURIComponent(requestId)}` }, { apiName: 'default' });
  }
  transition(id: string, action: 'start-production' | 'complete-production') {
    return this.rest.request<void, Batch>({ method: 'POST', url: `${this.url}/${id}/${action}` }, { apiName: 'default' });
  }
}
