import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { SidenavComponent } from './components/sidenav.component';

@Component({
  selector: 'app-layout',
  standalone: true,
  imports: [RouterOutlet, SidenavComponent],
  template: `
    <app-sidenav>
      <router-outlet></router-outlet>
    </app-sidenav>
  `
})
export class LayoutComponent {}
