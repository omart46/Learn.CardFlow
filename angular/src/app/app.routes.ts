import { authGuard, permissionGuard } from '@abp/ng.core';
import { Routes } from '@angular/router';
export const APP_ROUTES: Routes = [
  {
    path: 'dashboard', canActivate: [authGuard],
    loadComponent: () => import('./dashboard/dashboard.component').then(c => c.DashboardComponent),
  },
  {
    path: 'deliveries', canActivate: [authGuard, permissionGuard],
    data: { requiredPolicy: 'CardFlow.Deliveries' },
    loadComponent: () => import('./deliveries/deliveries.component').then(c => c.DeliveriesComponent),
  },
  {
    path: 'quality-checks', canActivate: [authGuard, permissionGuard],
    data: { requiredPolicy: 'CardFlow.QualityChecks' },
    loadComponent: () => import('./quality-checks/quality-checks.component').then(c => c.QualityChecksComponent),
  },
  {
    path: 'production-batches',
    canActivate: [authGuard, permissionGuard],
    data: { requiredPolicy: 'CardFlow.ProductionBatches' },
    loadComponent: () => import('./production-batches/production-batches.component').then(c => c.ProductionBatchesComponent),
  },
  {
    path: 'card-requests',
    canActivate: [authGuard, permissionGuard],
    data: { requiredPolicy: 'CardFlow.CardRequests' },
    loadComponent: () => import('./card-requests/card-requests.component').then(c => c.CardRequestsComponent),
  },
  {
    path: '',
    pathMatch: 'full',
    loadComponent: () => import('./home/home.component').then(c => c.HomeComponent),
  },
  {
    path: 'account',
    loadChildren: () => import('@abp/ng.account').then(c => c.createRoutes()),
  },
  {
    path: 'identity',
    loadChildren: () => import('@abp/ng.identity').then(c => c.createRoutes()),
  },
  {
    path: 'setting-management',
    loadChildren: () => import('@abp/ng.setting-management').then(c => c.createRoutes()),
  },
];
