import { ChangeDetectionStrategy, Component } from '@angular/core';
import { DynamicLayoutComponent } from '@abp/ng.core';
import { LoaderBarComponent } from '@abp/ng.theme.shared';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { AccountSwitcherComponent } from './account-switcher/account-switcher.component';

@Component({
  selector: 'app-root',
  template: `
    <abp-loader-bar />
    <nav class="cf-navigation" aria-label="CardFlow navigation">
      <a class="cf-brand" routerLink="/"><span class="cf-brand-icon"><i class="far fa-id-card" aria-hidden="true"></i></span><span>CardFlow<small>OPERATIONS WORKSPACE</small></span></a>
      <div class="cf-links">
        <a routerLink="/" routerLinkActive="active" [routerLinkActiveOptions]="{ exact: true }">Home</a>
        <a routerLink="/dashboard" routerLinkActive="active">Dashboard</a>
        <a routerLink="/card-requests" routerLinkActive="active">Card Requests</a>
        <a routerLink="/production-batches" routerLinkActive="active">Batches</a>
        <a routerLink="/quality-checks" routerLinkActive="active">Quality Checks</a>
        <a routerLink="/deliveries" routerLinkActive="active">Deliveries</a>
      </div>
      <app-account-switcher />
    </nav>
    <abp-dynamic-layout />
  `,
  styles: [`
    .cf-navigation { display:flex; align-items:center; justify-content:space-between; gap:18px 24px; flex-wrap:wrap; padding:18px 28px; background:#fff; border-bottom:1px solid #e4eaf0; }
    .cf-brand { display:flex; align-items:center; gap:10px; color:#1d3b48; font-size:21px; font-weight:750; text-decoration:none; letter-spacing:-.6px; }
    .cf-brand-icon { display:grid; place-items:center; height:38px; width:38px; background:#147e76; color:white; border-radius:11px; font-size:20px; box-shadow:0 3px 8px #147e7618; }
    .cf-brand small { display:block; margin-top:3px; color:#82909e; font-size:8px; letter-spacing:1.4px; font-weight:650; }
    .cf-links { display:flex; align-items:center; gap:4px; flex-wrap:wrap; }
    .cf-links a { color:#627383; padding:10px 11px; border-radius:8px; text-decoration:none; font-size:12px; font-weight:600; transition:background .15s,color .15s; white-space:nowrap; }
    .cf-links a:hover { color:#147e76; background:#f4f8f7; }.cf-links a.active { color:#116c65; background:#e9f5f1; box-shadow:inset 0 -2px #147e76; }
    @media(min-width:1200px) { .cf-navigation { margin-left:280px; }.cf-brand { display:none; } }
    @media(max-width:1199px) { .cf-links { order:3; flex:1 0 100%; flex-wrap:nowrap; overflow-x:auto; padding-bottom:3px; scrollbar-width:thin; } }
    @media(max-width:600px) { .cf-navigation { padding:16px; gap:16px; } app-account-switcher { width:100%; }.cf-links { gap:2px; }.cf-links a { padding:10px; } }
  `],
  changeDetection: ChangeDetectionStrategy.Eager,
  imports: [LoaderBarComponent, DynamicLayoutComponent, RouterLink, RouterLinkActive, AccountSwitcherComponent],
})
export class AppComponent {}
