import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { RouterModule } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';

interface DashboardCard {
  title: string;
  value: string;
  icon: string;
  gradient: string;
  colorClass: string;
  change?: string;
  changeType?: 'positive' | 'negative';
}

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatIconModule,
    MatButtonModule,
    RouterModule
  ],
  template: `
    <div class="dashboard">
      <!-- Page Header -->
      <div class="page-header">
        <div class="header-content">
          <h1 class="page-title">Dashboard</h1>
          <p class="page-subtitle">Welcome back, {{authService.currentUser()?.fullName}}! Here's what's happening today.</p>
        </div>
        <button class="primary-btn" routerLink="/sales/new">
          <mat-icon>add</mat-icon>
          <span>New Sale</span>
        </button>
      </div>

      <!-- Stats Cards -->
      <div class="stats-grid">
        @for (card of dashboardCards; track card.title) {
          <div class="stat-card" [attr.data-color]="card.colorClass">
            <div class="stat-icon" [style.background]="card.gradient">
              <mat-icon>{{card.icon}}</mat-icon>
            </div>
            <div class="stat-content">
              <span class="stat-label">{{card.title}}</span>
              <span class="stat-value">{{card.value}}</span>
              @if (card.change) {
                <span class="stat-change" [class.positive]="card.changeType === 'positive'" [class.negative]="card.changeType === 'negative'">
                  <mat-icon>{{card.changeType === 'positive' ? 'trending_up' : 'trending_down'}}</mat-icon>
                  {{card.change}}
                </span>
              }
            </div>
          </div>
        }
      </div>

      <!-- Main Grid -->
      <div class="main-grid">
        <!-- Quick Actions -->
        <div class="card">
          <div class="card-header">
            <h2 class="card-title">Quick Actions</h2>
            <span class="card-subtitle">Frequently used features</span>
          </div>
          <div class="card-body">
            <div class="actions-grid">
              <a routerLink="/products/new" class="action-item">
                <div class="action-icon blue">
                  <mat-icon>add_box</mat-icon>
                </div>
                <span class="action-label">Add Product</span>
              </a>
              <a routerLink="/sales/new" class="action-item">
                <div class="action-icon green">
                  <mat-icon>point_of_sale</mat-icon>
                </div>
                <span class="action-label">New Sale</span>
              </a>
              <a routerLink="/inventory/adjustment" class="action-item">
                <div class="action-icon purple">
                  <mat-icon>inventory</mat-icon>
                </div>
                <span class="action-label">Stock Adjust</span>
              </a>
              <a routerLink="/customers/new" class="action-item">
                <div class="action-icon orange">
                  <mat-icon>person_add</mat-icon>
                </div>
                <span class="action-label">Add Customer</span>
              </a>
            </div>
          </div>
        </div>

        <!-- Low Stock Alerts -->
        <div class="card">
          <div class="card-header">
            <h2 class="card-title">Low Stock Alerts</h2>
            <span class="card-badge warning">3 items</span>
          </div>
          <div class="card-body">
            <div class="alerts-list">
              <div class="alert-item critical">
                <div class="alert-icon">
                  <mat-icon>error</mat-icon>
                </div>
                <div class="alert-content">
                  <span class="alert-name">Product A</span>
                  <span class="alert-desc">Below minimum stock</span>
                </div>
                <span class="alert-count">5 left</span>
              </div>
              <div class="alert-item warning">
                <div class="alert-icon">
                  <mat-icon>warning</mat-icon>
                </div>
                <div class="alert-content">
                  <span class="alert-name">Product B</span>
                  <span class="alert-desc">Running low</span>
                </div>
                <span class="alert-count">12 left</span>
              </div>
              <div class="alert-item warning">
                <div class="alert-icon">
                  <mat-icon>warning</mat-icon>
                </div>
                <div class="alert-content">
                  <span class="alert-name">Product C</span>
                  <span class="alert-desc">Running low</span>
                </div>
                <span class="alert-count">15 left</span>
              </div>
            </div>
            <a routerLink="/inventory" class="view-all-link">
              View all inventory
              <mat-icon>arrow_forward</mat-icon>
            </a>
          </div>
        </div>

        <!-- Recent Sales -->
        <div class="card wide">
          <div class="card-header">
            <h2 class="card-title">Recent Sales</h2>
            <a routerLink="/sales" class="card-link">View All</a>
          </div>
          <div class="card-body">
            <div class="sales-table">
              <div class="table-header">
                <span>Customer</span>
                <span>Date</span>
                <span>Amount</span>
                <span>Status</span>
              </div>
              <div class="table-row">
                <span class="customer">John Smith</span>
                <span class="date">Today, 2:30 PM</span>
                <span class="amount">$125.00</span>
                <span class="status completed">Completed</span>
              </div>
              <div class="table-row">
                <span class="customer">Sarah Wilson</span>
                <span class="date">Today, 11:15 AM</span>
                <span class="amount">$89.50</span>
                <span class="status completed">Completed</span>
              </div>
              <div class="table-row">
                <span class="customer">Mike Johnson</span>
                <span class="date">Yesterday</span>
                <span class="amount">$245.00</span>
                <span class="status pending">Pending</span>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .dashboard {
      max-width: 1400px;
      margin: 0 auto;
    }

    /* Page Header */
    .page-header {
      display: flex;
      justify-content: space-between;
      align-items: flex-start;
      margin-bottom: 32px;
    }

    .page-title {
      font-size: 28px;
      font-weight: 700;
      color: #1a1a2e;
      margin: 0 0 4px;
    }

    .page-subtitle {
      font-size: 15px;
      color: #6b7280;
      margin: 0;
    }

    .primary-btn {
      display: flex;
      align-items: center;
      gap: 8px;
      padding: 12px 24px;
      background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
      color: white;
      border: none;
      border-radius: 12px;
      font-size: 14px;
      font-weight: 600;
      cursor: pointer;
      transition: all 0.2s;
      text-decoration: none;
    }

    .primary-btn:hover {
      transform: translateY(-2px);
      box-shadow: 0 8px 25px rgba(102, 126, 234, 0.4);
    }

    .primary-btn mat-icon {
      font-size: 20px;
      width: 20px;
      height: 20px;
    }

    /* Stats Grid */
    .stats-grid {
      display: grid;
      grid-template-columns: repeat(4, 1fr);
      gap: 24px;
      margin-bottom: 32px;
    }

    .stat-card {
      background: white;
      border-radius: 16px;
      padding: 24px;
      display: flex;
      gap: 16px;
      box-shadow: 0 1px 3px rgba(0,0,0,0.08);
      transition: all 0.2s;
    }

    .stat-card:hover {
      transform: translateY(-4px);
      box-shadow: 0 12px 24px rgba(0,0,0,0.1);
    }

    .stat-icon {
      width: 56px;
      height: 56px;
      border-radius: 14px;
      display: flex;
      align-items: center;
      justify-content: center;
      flex-shrink: 0;
    }

    .stat-icon mat-icon {
      color: white;
      font-size: 28px;
      width: 28px;
      height: 28px;
    }

    .stat-content {
      display: flex;
      flex-direction: column;
    }

    .stat-label {
      font-size: 13px;
      color: #6b7280;
      margin-bottom: 4px;
    }

    .stat-value {
      font-size: 24px;
      font-weight: 700;
      color: #1a1a2e;
      line-height: 1.2;
    }

    .stat-change {
      display: flex;
      align-items: center;
      gap: 4px;
      font-size: 12px;
      font-weight: 500;
      margin-top: 6px;
    }

    .stat-change mat-icon {
      font-size: 16px;
      width: 16px;
      height: 16px;
    }

    .stat-change.positive {
      color: #10b981;
    }

    .stat-change.negative {
      color: #ef4444;
    }

    /* Main Grid */
    .main-grid {
      display: grid;
      grid-template-columns: repeat(2, 1fr);
      gap: 24px;
    }

    .card {
      background: white;
      border-radius: 16px;
      box-shadow: 0 1px 3px rgba(0,0,0,0.08);
      overflow: hidden;
    }

    .card.wide {
      grid-column: span 2;
    }

    .card-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 20px 24px;
      border-bottom: 1px solid #f3f4f6;
    }

    .card-title {
      font-size: 16px;
      font-weight: 600;
      color: #1a1a2e;
      margin: 0;
    }

    .card-subtitle {
      font-size: 13px;
      color: #9ca3af;
    }

    .card-badge {
      padding: 4px 12px;
      border-radius: 20px;
      font-size: 12px;
      font-weight: 600;
    }

    .card-badge.warning {
      background: #fef3c7;
      color: #d97706;
    }

    .card-link {
      font-size: 14px;
      color: #667eea;
      text-decoration: none;
      font-weight: 500;
    }

    .card-link:hover {
      text-decoration: underline;
    }

    .card-body {
      padding: 24px;
    }

    /* Quick Actions */
    .actions-grid {
      display: grid;
      grid-template-columns: repeat(2, 1fr);
      gap: 16px;
    }

    .action-item {
      display: flex;
      flex-direction: column;
      align-items: center;
      gap: 12px;
      padding: 20px;
      background: #f9fafb;
      border-radius: 12px;
      text-decoration: none;
      transition: all 0.2s;
    }

    .action-item:hover {
      background: #f3f4f6;
      transform: translateY(-2px);
    }

    .action-icon {
      width: 48px;
      height: 48px;
      border-radius: 12px;
      display: flex;
      align-items: center;
      justify-content: center;
    }

    .action-icon mat-icon {
      color: white;
      font-size: 24px;
      width: 24px;
      height: 24px;
    }

    .action-icon.blue { background: linear-gradient(135deg, #3b82f6 0%, #1d4ed8 100%); }
    .action-icon.green { background: linear-gradient(135deg, #10b981 0%, #059669 100%); }
    .action-icon.purple { background: linear-gradient(135deg, #8b5cf6 0%, #6d28d9 100%); }
    .action-icon.orange { background: linear-gradient(135deg, #f59e0b 0%, #d97706 100%); }

    .action-label {
      font-size: 13px;
      font-weight: 600;
      color: #374151;
    }

    /* Alerts */
    .alerts-list {
      display: flex;
      flex-direction: column;
      gap: 12px;
    }

    .alert-item {
      display: flex;
      align-items: center;
      gap: 12px;
      padding: 14px 16px;
      border-radius: 12px;
    }

    .alert-item.critical {
      background: #fef2f2;
    }

    .alert-item.warning {
      background: #fffbeb;
    }

    .alert-icon {
      width: 36px;
      height: 36px;
      border-radius: 10px;
      display: flex;
      align-items: center;
      justify-content: center;
    }

    .alert-item.critical .alert-icon {
      background: #fee2e2;
    }

    .alert-item.critical .alert-icon mat-icon {
      color: #dc2626;
    }

    .alert-item.warning .alert-icon {
      background: #fef3c7;
    }

    .alert-item.warning .alert-icon mat-icon {
      color: #d97706;
    }

    .alert-content {
      flex: 1;
      display: flex;
      flex-direction: column;
    }

    .alert-name {
      font-size: 14px;
      font-weight: 600;
      color: #1f2937;
    }

    .alert-desc {
      font-size: 12px;
      color: #6b7280;
    }

    .alert-count {
      font-size: 13px;
      font-weight: 600;
      color: #6b7280;
    }

    .view-all-link {
      display: flex;
      align-items: center;
      justify-content: center;
      gap: 6px;
      margin-top: 16px;
      padding: 12px;
      color: #667eea;
      font-size: 14px;
      font-weight: 500;
      text-decoration: none;
      border-radius: 10px;
      transition: background 0.2s;
    }

    .view-all-link:hover {
      background: #f5f7fb;
    }

    .view-all-link mat-icon {
      font-size: 18px;
      width: 18px;
      height: 18px;
    }

    /* Sales Table */
    .sales-table {
      display: flex;
      flex-direction: column;
    }

    .table-header, .table-row {
      display: grid;
      grid-template-columns: 2fr 1.5fr 1fr 1fr;
      padding: 14px 0;
      align-items: center;
    }

    .table-header {
      font-size: 12px;
      font-weight: 600;
      color: #6b7280;
      text-transform: uppercase;
      letter-spacing: 0.05em;
      border-bottom: 1px solid #f3f4f6;
    }

    .table-row {
      border-bottom: 1px solid #f9fafb;
    }

    .table-row:last-child {
      border-bottom: none;
    }

    .customer {
      font-size: 14px;
      font-weight: 600;
      color: #1f2937;
    }

    .date {
      font-size: 14px;
      color: #6b7280;
    }

    .amount {
      font-size: 14px;
      font-weight: 600;
      color: #1f2937;
    }

    .status {
      display: inline-flex;
      padding: 4px 12px;
      border-radius: 20px;
      font-size: 12px;
      font-weight: 600;
      width: fit-content;
    }

    .status.completed {
      background: #d1fae5;
      color: #059669;
    }

    .status.pending {
      background: #fef3c7;
      color: #d97706;
    }

    /* Responsive */
    @media (max-width: 1200px) {
      .stats-grid {
        grid-template-columns: repeat(2, 1fr);
      }
    }

    @media (max-width: 768px) {
      .page-header {
        flex-direction: column;
        gap: 16px;
      }

      .stats-grid {
        grid-template-columns: 1fr;
      }

      .main-grid {
        grid-template-columns: 1fr;
      }

      .card.wide {
        grid-column: span 1;
      }

      .table-header, .table-row {
        grid-template-columns: 1fr 1fr;
        gap: 8px;
      }

      .table-header span:nth-child(2),
      .table-row .date {
        display: none;
      }
    }
  `]
})
export class DashboardComponent {
  authService = inject(AuthService);

  dashboardCards: DashboardCard[] = [
    {
      title: "Today's Sales",
      value: '$1,250.00',
      icon: 'point_of_sale',
      gradient: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
      colorClass: 'purple',
      change: '+12% from yesterday',
      changeType: 'positive'
    },
    {
      title: 'Total Products',
      value: '156',
      icon: 'inventory_2',
      gradient: 'linear-gradient(135deg, #10b981 0%, #059669 100%)',
      colorClass: 'green',
      change: '+5 new this week',
      changeType: 'positive'
    },
    {
      title: 'Low Stock Items',
      value: '8',
      icon: 'warning',
      gradient: 'linear-gradient(135deg, #f59e0b 0%, #d97706 100%)',
      colorClass: 'orange',
      change: 'Needs attention',
      changeType: 'negative'
    },
    {
      title: 'Total Customers',
      value: '324',
      icon: 'people',
      gradient: 'linear-gradient(135deg, #3b82f6 0%, #1d4ed8 100%)',
      colorClass: 'blue',
      change: '+3 new today',
      changeType: 'positive'
    }
  ];
}
