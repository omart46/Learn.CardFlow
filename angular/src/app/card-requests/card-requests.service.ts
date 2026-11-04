import { Injectable, inject } from '@angular/core';
import { RestService } from '@abp/ng.core';

export interface CardRequest {
  id: string;
  employeeName: string;
  employeeNumber: string;
  status: number;
  rejectionReason?: string | null;
  isMine: boolean;
  needsResubmission: boolean;
  creationTime: string;
}

@Injectable({ providedIn: 'root' })
export class CardRequestsService {
  private readonly rest = inject(RestService);
  private readonly url = '/api/app/employee-card-request';

  getList(skipCount: number, maxResultCount: number, onlyMine = false, needsAttention = false,
    filters: { filter?: string; status?: number; createdFrom?: string; createdTo?: string } = {}) {
    return this.rest.request<void, { items: CardRequest[]; totalCount: number }>({
      method: 'GET', url: this.url, params: { skipCount, maxResultCount, onlyMine, needsAttention, ...filters },
    }, { apiName: 'default' });
  }

  create(body: { employeeName: string; employeeNumber: string }) {
    return this.rest.request<typeof body, CardRequest>({ method: 'POST', url: this.url, body },
      { apiName: 'default' });
  }

  getMyAttentionCount() {
    return this.rest.request<void, number>({ method: 'GET', url: `${this.url}/my-attention-count` },
      { apiName: 'default' });
  }

  correct(id: string, body: { employeeName: string; employeeNumber: string }) {
    return this.rest.request<typeof body, CardRequest>({ method: 'POST', url: `${this.url}/${encodeURIComponent(id)}/correct`, body },
      { apiName: 'default' });
  }

  resubmit(id: string) {
    return this.rest.request<void, CardRequest>({ method: 'POST', url: `${this.url}/${encodeURIComponent(id)}/resubmit` },
      { apiName: 'default' });
  }

  reject(id: string, reason: string) {
    return this.rest.request<{ reason: string }, CardRequest>({
      method: 'POST', url: `${this.url}/${encodeURIComponent(id)}/reject`, body: { reason },
    }, { apiName: 'default' });
  }

  transition(id: string, action: 'start-review' | 'approve') {
    return this.rest.request<void, CardRequest>({
      method: 'POST', url: `${this.url}/${encodeURIComponent(id)}/${action}`,
    }, { apiName: 'default' });
  }
}
