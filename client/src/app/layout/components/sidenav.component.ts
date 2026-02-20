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
    <div class="app-container">
      <!-- Sidebar -->
      <aside class="sidebar" [class.collapsed]="sidebarCollapsed">
        <div class="sidebar-header">
          <div class="logo">
            <mat-icon class="logo-icon">inventory_2</mat-icon>
            @if (!sidebarCollapsed) {
              <span class="logo-text">InventoryPro</span>
            }
          </div>
        </div>

        <nav class="sidebar-nav">
          @for (item of navItems; track item.route) {
            <a [routerLink]="item.route" routerLinkActive="active" class="nav-item">
              <mat-icon>{{item.icon}}</mat-icon>
              @if (!sidebarCollapsed) {
                <span>{{item.label}}</span>
              }
            </a>
          }
        </nav>

        <div class="sidebar-footer">
          <button class="collapse-btn" (click)="sidebarCollapsed = !sidebarCollapsed">
            <mat-icon>{{sidebarCollapsed ? 'chevron_right' : 'chevron_left'}}</mat-icon>
          </button>
        </div>
      </aside>

      <!-- Main Content -->
      <div class="main-wrapper">
        <!-- Top Bar -->
        <header class="topbar">
          <div class="topbar-left">
            <button class="menu-toggle" (click)="sidebarCollapsed = !sidebarCollapsed">
              <mat-icon>menu</mat-icon>
            </button>
            <div class="search-box">
              <mat-icon>search</mat-icon>
              <input type="text" placeholder="Search...">
            </div>
          </div>

          <div class="topbar-right">
            <button class="icon-btn">
              <mat-icon>notifications_none</mat-icon>
              <span class="badge">3</span>
            </button>

            @if (authService.currentUser(); as user) {
              <div class="user-menu" [matMenuTriggerFor]="userMenu">
                <div class="user-avatar">
                  {{user.fullName.charAt(0)}}
                </div>
                <div class="user-info">
                  <span class="user-name">{{user.fullName}}</span>
                  <span class="user-role">{{user.role || 'User'}}</span>
                </div>
                <mat-icon>expand_more</mat-icon>
              </div>
              <mat-menu #userMenu="matMenu" class="user-dropdown">
                <button mat-menu-item>
                  <mat-icon>person_outline</mat-icon>
                  <span>My Profile</span>
                </button>
                <button mat-menu-item>
                  <mat-icon>settings</mat-icon>
                  <span>Settings</span>
                </button>
                <button mat-menu-item>
                  <mat-icon>help_outline</mat-icon>
                  <span>Help Center</span>
                </button>
                <mat-divider></mat-divider>
                <button mat-menu-item (click)="authService.logout()" class="logout-btn">
                  <mat-icon>logout</mat-icon>
                  <span>Logout</span>
                </button>
              </mat-menu>
            }
          </div>
        </header>

        <!-- Page Content -->
        <main class="main-content">
          <ng-content></ng-content>
        </main>
      </div>
    </div>
  `,
  styles: [`
    .app-container {
      display: flex;
      min-height: 100vh;
      background: #f5f7fb;
    }

    /* Sidebar */
    .sidebar {
      width: 260px;
      background: linear-gradient(180deg, #1a1a2e 0%, #16213e 100%);
      display: flex;
      flex-direction: column;
      transition: width 0.3s ease;
      position: fixed;
      height: 100vh;
      z-index: 100;
    }

    .sidebar.collapsed {
      width: 80px;
    }

    .sidebar-header {
      padding: 24px 20px;
      border-bottom: 1px solid rgba(255,255,255,0.1);
    }

    .logo {
      display: flex;
      align-items: center;
      gap: 12px;
    }

    .logo-icon {
      color: #667eea;
      font-size: 32px;
      width: 32px;
      height: 32px;
    }

    .logo-text {
      color: white;
      font-size: 20px;
      font-weight: 700;
      white-space: nowrap;
    }

    .sidebar-nav {
      flex: 1;
      padding: 16px 12px;
      overflow-y: auto;
    }

    .nav-item {
      display: flex;
      align-items: center;
      gap: 12px;
      padding: 12px 16px;
      color: rgba(255,255,255,0.7);
      text-decoration: none;
      border-radius: 10px;
      margin-bottom: 4px;
      transition: all 0.2s ease;
      font-size: 14px;
      font-weight: 500;
    }

    .nav-item:hover {
      color: white;
      background: rgba(102, 126, 234, 0.2);
    }

    .nav-item.active {
      color: white;
      background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
      box-shadow: 0 4px 15px rgba(102, 126, 234, 0.4);
    }

    .nav-item mat-icon {
      font-size: 22px;
      width: 22px;
      height: 22px;
    }

    .sidebar-footer {
      padding: 16px;
      border-top: 1px solid rgba(255,255,255,0.1);
    }

    .collapse-btn {
      width: 100%;
      padding: 10px;
      background: rgba(255,255,255,0.1);
      border: none;
      border-radius: 8px;
      color: rgba(255,255,255,0.7);
      cursor: pointer;
      display: flex;
      align-items: center;
      justify-content: center;
      transition: all 0.2s;
    }

    .collapse-btn:hover {
      background: rgba(255,255,255,0.15);
      color: white;
    }

    /* Main Wrapper */
    .main-wrapper {
      flex: 1;
      margin-left: 260px;
      transition: margin-left 0.3s ease;
      display: flex;
      flex-direction: column;
      min-height: 100vh;
    }

    .sidebar.collapsed + .main-wrapper {
      margin-left: 80px;
    }

    /* Topbar */
    .topbar {
      height: 70px;
      background: white;
      border-bottom: 1px solid #e5e7eb;
      display: flex;
      align-items: center;
      justify-content: space-between;
      padding: 0 24px;
      position: sticky;
      top: 0;
      z-index: 50;
    }

    .topbar-left {
      display: flex;
      align-items: center;
      gap: 16px;
    }

    .menu-toggle {
      display: none;
      background: none;
      border: none;
      padding: 8px;
      cursor: pointer;
      color: #6b7280;
      border-radius: 8px;
    }

    .menu-toggle:hover {
      background: #f3f4f6;
    }

    .search-box {
      display: flex;
      align-items: center;
      gap: 10px;
      background: #f5f7fb;
      padding: 10px 16px;
      border-radius: 10px;
      min-width: 280px;
    }

    .search-box mat-icon {
      color: #9ca3af;
      font-size: 20px;
      width: 20px;
      height: 20px;
    }

    .search-box input {
      border: none;
      background: transparent;
      outline: none;
      font-size: 14px;
      color: #1f2937;
      width: 100%;
    }

    .search-box input::placeholder {
      color: #9ca3af;
    }

    .topbar-right {
      display: flex;
      align-items: center;
      gap: 16px;
    }

    .icon-btn {
      position: relative;
      background: none;
      border: none;
      padding: 10px;
      cursor: pointer;
      color: #6b7280;
      border-radius: 10px;
      transition: all 0.2s;
    }

    .icon-btn:hover {
      background: #f5f7fb;
      color: #667eea;
    }

    .badge {
      position: absolute;
      top: 6px;
      right: 6px;
      background: #ef4444;
      color: white;
      font-size: 10px;
      font-weight: 600;
      padding: 2px 6px;
      border-radius: 10px;
    }

    .user-menu {
      display: flex;
      align-items: center;
      gap: 12px;
      padding: 8px 12px;
      border-radius: 12px;
      cursor: pointer;
      transition: background 0.2s;
    }

    .user-menu:hover {
      background: #f5f7fb;
    }

    .user-avatar {
      width: 40px;
      height: 40px;
      background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
      border-radius: 10px;
      display: flex;
      align-items: center;
      justify-content: center;
      color: white;
      font-weight: 600;
      font-size: 16px;
    }

    .user-info {
      display: flex;
      flex-direction: column;
    }

    .user-name {
      font-size: 14px;
      font-weight: 600;
      color: #1f2937;
    }

    .user-role {
      font-size: 12px;
      color: #9ca3af;
    }

    .user-menu mat-icon {
      color: #9ca3af;
    }

    /* Main Content */
    .main-content {
      flex: 1;
      padding: 24px;
    }

    /* Responsive */
    @media (max-width: 1024px) {
      .sidebar {
        transform: translateX(-100%);
      }

      .sidebar.collapsed {
        transform: translateX(-100%);
      }

      .main-wrapper {
        margin-left: 0;
      }

      .menu-toggle {
        display: flex;
      }

      .search-box {
        min-width: 200px;
      }
    }

    @media (max-width: 640px) {
      .search-box {
        display: none;
      }

      .user-info {
        display: none;
      }

      .topbar {
        padding: 0 16px;
      }

      .main-content {
        padding: 16px;
      }
    }
  `]
})
export class SidenavComponent {
  authService = inject(AuthService);
  sidebarCollapsed = false;

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
