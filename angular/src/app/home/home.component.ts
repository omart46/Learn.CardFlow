import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { AuthService, PermissionService } from '@abp/ng.core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss'],
  changeDetection: ChangeDetectionStrategy.Eager,
  imports: [RouterLink]
})
export class HomeComponent {
  private authService = inject(AuthService);
  private permissions = inject(PermissionService);
  get canViewDeliveries(): boolean {
    return this.permissions.getGrantedPolicy('CardFlow.Deliveries');
  }
  get canViewQuality(): boolean {
    return this.permissions.getGrantedPolicy('CardFlow.QualityChecks');
  }

  get canViewBatches(): boolean {
    return this.permissions.getGrantedPolicy('CardFlow.ProductionBatches');
  }

  get canViewRequests(): boolean {
    return this.permissions.getGrantedPolicy('CardFlow.CardRequests');
  }

  get hasLoggedIn(): boolean {
    return this.authService.isAuthenticated
  }

  login() {
    this.authService.navigateToLogin();
  }
}
