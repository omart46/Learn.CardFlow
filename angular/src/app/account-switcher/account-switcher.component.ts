import { ChangeDetectorRef, Component, inject } from '@angular/core';
import { AuthService, ConfigStateService } from '@abp/ng.core';

@Component({
  selector: 'app-account-switcher',
  template: `
    <div class="switcher">
      <span class="current">{{ currentUser || 'Not signed in' }}</span>
      <label for="switch-account">Switch user</label>
      <select id="switch-account" [disabled]="switching" (change)="switchAccount($event)">
        <option value="">Choose an account</option>
        @for (account of accounts; track account.username) {
          <option [value]="account.username" [disabled]="account.username.toLowerCase() === currentUser.toLowerCase()">
            {{ account.role }} / {{ account.username }}
          </option>
        }
      </select>
    </div>
    @if (error) { <p class="switch-error" role="alert">{{ error }}</p> }
    <span class="hint">Switching opens a new sign-in.</span>
  `,
  styles: [`
    :host { display:block; }.switcher { display:flex; align-items:center; gap:9px; flex-wrap:wrap; }
    .current { color:#315b58; background:#edf5f3; padding:7px 11px; border-radius:20px; font-size:11px; font-weight:650; max-width:130px; overflow:hidden; text-overflow:ellipsis; }
    label { font-size:11px; font-weight:650; color:#718292; }
    select { max-width:215px; min-height:38px; background:#f8fafb; border:1px solid #dfe7ec; border-radius:9px; padding:8px 11px; color:#344e60; font-size:12px; cursor:pointer; }
    select:focus-visible { outline:3px solid #65b8ad; outline-offset:2px; }
    .hint { display:block; margin-top:5px; text-align:right; color:#7b8b98; font-size:10px; }.switch-error { font-size:12px; color:#a45454; margin:8px 0; }
  `],
})
export class AccountSwitcherComponent {
  private readonly auth = inject(AuthService);
  private readonly config = inject(ConfigStateService);
  private readonly cd = inject(ChangeDetectorRef);
  private readonly pendingKey = 'cardflow.pending-account';
  readonly accounts = [
    { role: 'Admin', username: 'admin' },
    { role: 'Reviewer', username: 'Reviewer' },
    { role: 'Supervisor', username: 'supervisor1' },
    { role: 'Production', username: 'production1' },
    { role: 'Quality', username: 'quality1' },
    { role: 'Delivery', username: 'delivery1' },
  ];
  switching = false;
  error = '';

  get currentUser(): string { return this.config.getOne('currentUser')?.userName || ''; }

  constructor() {
    const pending = sessionStorage.getItem(this.pendingKey);
    if (pending) {
      sessionStorage.removeItem(this.pendingKey);
      if (this.accounts.some(account => account.username === pending)) this.signIn(pending);
    }
  }

  switchAccount(event: Event) {
    const select = event.target as HTMLSelectElement;
    const username = select.value;
    select.value = '';
    if (this.switching || !this.accounts.some(account => account.username === username)) return;
    this.switching = true;
    this.error = '';
    if (!this.auth.isAuthenticated) {
      this.signIn(username);
      return;
    }
    // Keep only the intended username across the real OIDC logout redirect.
    sessionStorage.setItem(this.pendingKey, username);
    this.auth.logout().subscribe({
      error: () => {
        sessionStorage.removeItem(this.pendingKey);
        this.switching = false;
        this.error = 'Unable to sign out. Please try again.';
        this.cd.markForCheck();
      },
    });
  }

  private signIn(username: string) {
    this.switching = true;
    this.auth.navigateToLogin({ prompt: 'login', login_hint: username, returnUrl: '/dashboard' });
  }
}
