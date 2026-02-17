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
  color: string;
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
    <div class="space-y-6">
      <div class="flex justify-between items-center">
        <div>
          <h1 class="text-2xl font-bold text-gray-800">Dashboard</h1>
          <p class="text-gray-600">Welcome back, {{authService.currentUser()?.fullName}}</p>
        </div>
        <button mat-raised-button color="primary" routerLink="/sales/new">
          <mat-icon>add</mat-icon>
          New Sale
        </button>
      </div>

      <!-- Stats Cards -->
      <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
        @for (card of dashboardCards; track card.title) {
          <mat-card class="hover:shadow-lg transition-shadow">
            <mat-card-content class="p-6">
              <div class="flex items-center justify-between">
                <div>
                  <p class="text-gray-500 text-sm">{{card.title}}</p>
                  <p class="text-2xl font-bold mt-1">{{card.value}}</p>
                  @if (card.change) {
                    <p class="text-sm mt-1"
                       [class.text-green-600]="card.changeType === 'positive'"
                       [class.text-red-600]="card.changeType === 'negative'">
                      {{card.change}}
                    </p>
                  }
                </div>
                <div [class]="'p-3 rounded-full ' + card.color">
                  <mat-icon class="text-white">{{card.icon}}</mat-icon>
                </div>
              </div>
            </mat-card-content>
          </mat-card>
        }
      </div>

      <!-- Quick Actions -->
      <div class="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <mat-card>
          <mat-card-header>
            <mat-card-title>Quick Actions</mat-card-title>
          </mat-card-header>
          <mat-card-content class="p-4">
            <div class="grid grid-cols-2 gap-4">
              <button mat-stroked-button routerLink="/products/new" class="h-20 flex-col">
                <mat-icon>add_box</mat-icon>
                <span>Add Product</span>
              </button>
              <button mat-stroked-button routerLink="/sales/new" class="h-20 flex-col">
                <mat-icon>point_of_sale</mat-icon>
                <span>New Sale</span>
              </button>
              <button mat-stroked-button routerLink="/inventory/adjustment" class="h-20 flex-col">
                <mat-icon>inventory</mat-icon>
                <span>Stock Adjustment</span>
              </button>
              <button mat-stroked-button routerLink="/customers/new" class="h-20 flex-col">
                <mat-icon>person_add</mat-icon>
                <span>Add Customer</span>
              </button>
            </div>
          </mat-card-content>
        </mat-card>

        <mat-card>
          <mat-card-header>
            <mat-card-title>Low Stock Alerts</mat-card-title>
          </mat-card-header>
          <mat-card-content class="p-4">
            <div class="space-y-3">
              <div class="flex items-center justify-between p-3 bg-red-50 rounded-lg">
                <div class="flex items-center gap-3">
                  <mat-icon class="text-red-500">warning</mat-icon>
                  <span>Product A</span>
                </div>
                <span class="text-red-600 font-semibold">5 units left</span>
              </div>
              <div class="flex items-center justify-between p-3 bg-yellow-50 rounded-lg">
                <div class="flex items-center gap-3">
                  <mat-icon class="text-yellow-500">info</mat-icon>
                  <span>Product B</span>
                </div>
                <span class="text-yellow-600 font-semibold">12 units left</span>
              </div>
              <div class="flex items-center justify-between p-3 bg-yellow-50 rounded-lg">
                <div class="flex items-center gap-3">
                  <mat-icon class="text-yellow-500">info</mat-icon>
                  <span>Product C</span>
                </div>
                <span class="text-yellow-600 font-semibold">15 units left</span>
              </div>
            </div>
          </mat-card-content>
        </mat-card>
      </div>
    </div>
  `
})
export class DashboardComponent {
  authService = inject(AuthService);

  dashboardCards: DashboardCard[] = [
    {
      title: "Today's Sales",
      value: '$1,250.00',
      icon: 'point_of_sale',
      color: 'bg-blue-500',
      change: '+12% from yesterday',
      changeType: 'positive'
    },
    {
      title: 'Total Products',
      value: '156',
      icon: 'inventory_2',
      color: 'bg-green-500',
      change: '+5 new this week',
      changeType: 'positive'
    },
    {
      title: 'Low Stock Items',
      value: '8',
      icon: 'warning',
      color: 'bg-orange-500',
      change: 'Needs attention',
      changeType: 'negative'
    },
    {
      title: 'Total Customers',
      value: '324',
      icon: 'people',
      color: 'bg-purple-500',
      change: '+3 new today',
      changeType: 'positive'
    }
  ];
}
