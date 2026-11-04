import { ChangeDetectionStrategy, Component } from '@angular/core';

@Component({
  selector: 'abp-footer',
  template: `
    <div class="lpx-footbar-container end-0">
      <div class="lpx-footbar">
        <div class="lpx-footbar-copyright">
          <span>© {{ currentYear }} CardFlow</span>
        </div>
        <div class="lpx-footbar-solo-links">
          <span>Employee card management</span>
        </div>
      </div>
    </div>
  `,
  changeDetection: ChangeDetectionStrategy.Eager,
})
export class FooterComponent {
  currentYear = new Date().getFullYear();
}
