import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MatTableModule } from '@angular/material/table';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { FormsModule } from '@angular/forms';
import { ApiService, PagedResult } from '../../core/services/api.service';

interface Customer {
  id: string;
  name: string;
  code?: string;
  email?: string;
  phone?: string;
  groupName?: string;
  currentBalance: number;
  loyaltyPoints: number;
  totalPurchases: number;
  totalOrders: number;
  isActive: boolean;
  createdAt: string;
}

@Component({
  selector: 'app-customers',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    FormsModule,
    MatTableModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatInputModule,
    MatFormFieldModule,
    MatPaginatorModule,
    MatProgressSpinnerModule
  ],
  template: `
    <div class="space-y-6">
      <div class="flex justify-between items-center">
        <h1 class="text-2xl font-bold text-gray-800">Customers</h1>
        <button mat-raised-button color="primary" routerLink="new">
          <mat-icon>person_add</mat-icon>
          Add Customer
        </button>
      </div>

      <mat-card>
        <mat-card-content class="p-4">
          <!-- Search -->
          <mat-form-field appearance="outline" class="w-full md:w-96 mb-4">
            <mat-label>Search customers</mat-label>
            <input matInput [(ngModel)]="searchTerm" (keyup.enter)="search()" placeholder="Search by name, email, phone...">
            <mat-icon matSuffix>search</mat-icon>
          </mat-form-field>

          @if (isLoading()) {
            <div class="flex justify-center p-8">
              <mat-spinner></mat-spinner>
            </div>
          } @else {
            <div class="overflow-x-auto">
              <table mat-table [dataSource]="customers()" class="w-full">
                <ng-container matColumnDef="name">
                  <th mat-header-cell *matHeaderCellDef>Name</th>
                  <td mat-cell *matCellDef="let customer">
                    <div>
                      <div class="font-medium">{{customer.name}}</div>
                      @if (customer.code) {
                        <div class="text-sm text-gray-500">{{customer.code}}</div>
                      }
                    </div>
                  </td>
                </ng-container>

                <ng-container matColumnDef="contact">
                  <th mat-header-cell *matHeaderCellDef>Contact</th>
                  <td mat-cell *matCellDef="let customer">
                    <div>
                      @if (customer.email) {
                        <div class="text-sm">{{customer.email}}</div>
                      }
                      @if (customer.phone) {
                        <div class="text-sm text-gray-500">{{customer.phone}}</div>
                      }
                    </div>
                  </td>
                </ng-container>

                <ng-container matColumnDef="totalPurchases">
                  <th mat-header-cell *matHeaderCellDef>Total Purchases</th>
                  <td mat-cell *matCellDef="let customer">{{customer.totalPurchases | currency}}</td>
                </ng-container>

                <ng-container matColumnDef="orders">
                  <th mat-header-cell *matHeaderCellDef>Orders</th>
                  <td mat-cell *matCellDef="let customer">{{customer.totalOrders}}</td>
                </ng-container>

                <ng-container matColumnDef="loyaltyPoints">
                  <th mat-header-cell *matHeaderCellDef>Loyalty Points</th>
                  <td mat-cell *matCellDef="let customer">{{customer.loyaltyPoints}}</td>
                </ng-container>

                <ng-container matColumnDef="status">
                  <th mat-header-cell *matHeaderCellDef>Status</th>
                  <td mat-cell *matCellDef="let customer">
                    <span [class]="customer.isActive ? 'bg-green-100 text-green-800' : 'bg-red-100 text-red-800'"
                          class="px-2 py-1 rounded-full text-xs">
                      {{customer.isActive ? 'Active' : 'Inactive'}}
                    </span>
                  </td>
                </ng-container>

                <ng-container matColumnDef="actions">
                  <th mat-header-cell *matHeaderCellDef>Actions</th>
                  <td mat-cell *matCellDef="let customer">
                    <button mat-icon-button [routerLink]="[customer.id]">
                      <mat-icon>edit</mat-icon>
                    </button>
                    <button mat-icon-button color="warn">
                      <mat-icon>delete</mat-icon>
                    </button>
                  </td>
                </ng-container>

                <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
                <tr mat-row *matRowDef="let row; columns: displayedColumns;"></tr>
              </table>
            </div>

            <mat-paginator
              [length]="totalCount()"
              [pageSize]="pageSize"
              [pageSizeOptions]="[10, 25, 50]"
              (page)="onPageChange($event)">
            </mat-paginator>
          }
        </mat-card-content>
      </mat-card>
    </div>
  `
})
export class CustomersComponent implements OnInit {
  private apiService = inject(ApiService);

  customers = signal<Customer[]>([]);
  totalCount = signal(0);
  isLoading = signal(false);

  searchTerm = '';
  pageNumber = 1;
  pageSize = 10;

  displayedColumns = ['name', 'contact', 'totalPurchases', 'orders', 'loyaltyPoints', 'status', 'actions'];

  ngOnInit(): void {
    this.loadCustomers();
  }

  loadCustomers(): void {
    this.isLoading.set(true);
    this.apiService.get<PagedResult<Customer>>('/api/customers', {
      searchTerm: this.searchTerm || undefined,
      pageNumber: this.pageNumber,
      pageSize: this.pageSize
    }).subscribe({
      next: (result) => {
        this.customers.set(result.items);
        this.totalCount.set(result.totalCount);
        this.isLoading.set(false);
      },
      error: () => {
        this.isLoading.set(false);
      }
    });
  }

  search(): void {
    this.pageNumber = 1;
    this.loadCustomers();
  }

  onPageChange(event: PageEvent): void {
    this.pageNumber = event.pageIndex + 1;
    this.pageSize = event.pageSize;
    this.loadCustomers();
  }
}
