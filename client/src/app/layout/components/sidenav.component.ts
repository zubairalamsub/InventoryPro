import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatListModule } from '@angular/material/list';
import { MatIconModule } from '@angular/material/icon';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';
import { MatMenuModule } from '@angular/material/menu';
import { AuthService } from '../../core/services/auth.service';

interface NavItem {
  label: string;
  icon: string;
  route: string;
}

@Component({
  selector: 'app-sidenav',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatSidenavModule,
    MatListModule,
    MatIconModule,
    MatToolbarModule,
    MatButtonModule,
    MatMenuModule
  ],
  template: `
    <mat-sidenav-container class="h-screen">
      <mat-sidenav #sidenav mode="side" opened class="w-64 bg-primary-700">
        <div class="p-4 text-white">
          <h1 class="text-xl font-bold">InventoryPro</h1>
        </div>

        <mat-nav-list>
          @for (item of navItems; track item.route) {
            <a mat-list-item [routerLink]="item.route" routerLinkActive="bg-primary-600"
               class="text-white hover:bg-primary-600">
              <mat-icon matListItemIcon class="text-white">{{item.icon}}</mat-icon>
              <span matListItemTitle>{{item.label}}</span>
            </a>
          }
        </mat-nav-list>
      </mat-sidenav>

      <mat-sidenav-content class="bg-gray-100">
        <mat-toolbar color="primary" class="shadow-md">
          <button mat-icon-button (click)="sidenav.toggle()">
            <mat-icon>menu</mat-icon>
          </button>
          <span class="flex-1"></span>

          @if (authService.currentUser(); as user) {
            <button mat-button [matMenuTriggerFor]="userMenu" class="text-white">
              <mat-icon>account_circle</mat-icon>
              <span class="ml-2">{{user.fullName}}</span>
              <mat-icon>arrow_drop_down</mat-icon>
            </button>
            <mat-menu #userMenu="matMenu">
              <button mat-menu-item>
                <mat-icon>person</mat-icon>
                <span>Profile</span>
              </button>
              <button mat-menu-item>
                <mat-icon>settings</mat-icon>
                <span>Settings</span>
              </button>
              <mat-divider></mat-divider>
              <button mat-menu-item (click)="authService.logout()">
                <mat-icon>logout</mat-icon>
                <span>Logout</span>
              </button>
            </mat-menu>
          }
        </mat-toolbar>

        <main class="p-6">
          <ng-content></ng-content>
        </main>
      </mat-sidenav-content>
    </mat-sidenav-container>
  `,
  styles: [`
    mat-sidenav {
      background-color: #1976d2;
    }
    mat-nav-list a {
      color: white !important;
    }
    mat-nav-list a:hover {
      background-color: rgba(255, 255, 255, 0.1);
    }
    mat-nav-list a.bg-primary-600 {
      background-color: rgba(255, 255, 255, 0.2);
    }
  `]
})
export class SidenavComponent {
  authService = inject(AuthService);

  navItems: NavItem[] = [
    { label: 'Dashboard', icon: 'dashboard', route: '/dashboard' },
    { label: 'Products', icon: 'inventory_2', route: '/products' },
    { label: 'Inventory', icon: 'warehouse', route: '/inventory' },
    { label: 'Sales', icon: 'point_of_sale', route: '/sales' },
    { label: 'Customers', icon: 'people', route: '/customers' },
    { label: 'Reports', icon: 'assessment', route: '/reports' },
    { label: 'Settings', icon: 'settings', route: '/settings' }
  ];
}
