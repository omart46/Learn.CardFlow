import { RoutesService, eLayoutType } from '@abp/ng.core';
import { inject, provideAppInitializer } from '@angular/core';
export const APP_ROUTE_PROVIDER = [
  provideAppInitializer(() => {
    configureRoutes();
  }),
];
function configureRoutes() {
  const routes = inject(RoutesService);
  routes.add([
      {
        path: '/dashboard', name: 'Dashboard', iconClass: 'fas fa-chart-pie',
        order: 1.5, layout: eLayoutType.application,
      },
      {
        path: '/deliveries', name: 'Deliveries', iconClass: 'fas fa-hand-holding',
        order: 5, layout: eLayoutType.application, requiredPolicy: 'CardFlow.Deliveries',
      },
      {
        path: '/quality-checks', name: 'Quality Checks', iconClass: 'fas fa-clipboard-check',
        order: 4, layout: eLayoutType.application, requiredPolicy: 'CardFlow.QualityChecks',
      },
      {
        path: '/production-batches',
        name: 'Batches',
        iconClass: 'fas fa-layer-group',
        order: 3,
        layout: eLayoutType.application,
        requiredPolicy: 'CardFlow.ProductionBatches',
      },
      {
        path: '/card-requests',
        name: 'Card Requests',
        iconClass: 'fas fa-id-card',
        order: 2,
        layout: eLayoutType.application,
        requiredPolicy: 'CardFlow.CardRequests',
      },
      {
        path: '/',
        name: '::Menu:Home',
        iconClass: 'fas fa-home',
        order: 1,
        layout: eLayoutType.application,
      },
  ]);
}
