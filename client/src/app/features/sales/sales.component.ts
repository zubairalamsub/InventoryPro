import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MatTableModule } from '@angular/material/table';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { ApiService, PagedResult } from '../../core/services/api.service';

interface Sale {
  id: string;
  invoiceNumber: string;
  warehouseName: string;
  customerName?: string;
  status: string;
  totalAmount: number;
  paidAmount: number;
  outstandingAmount: number;
  itemCount: number;
  saleDate: string;
}

@Component({
  selector: 'app-sales',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatTableModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatChipsModule,
    MatPaginatorModule,
    MatProgressSpinnerModule
  ],
  template: `
    <div class="space-y-6">
      <div class="flex justify-between items-center">
        <h1 class="text-2xl font-bold text-gray-800">Sales</h1>
        <button mat-raised-button color="primary" routerLink="new">
          <mat-icon>add</mat-icon>
          New Sale
        </button>
      </div>

      <mat-card>
        <mat-card-content class="p-4">
          @if (isLoading()) {
            <div class="flex justify-center p-8">
              <mat-spinner></mat-spinner>
            </div>
          } @else {
            <div class="overflow-x-auto">
              <table mat-table [dataSource]="sales()" class="w-full">
                <ng-container matColumnDef="invoiceNumber">
                  <th mat-header-cell *matHeaderCellDef>Invoice #</th>
                  <td mat-cell *matCellDef="let sale">{{sale.invoiceNumber}}</td>
                </ng-container>

                <ng-container matColumnDef="customer">
                  <th mat-header-cell *matHeaderCellDef>Customer</th>
                  <td mat-cell *matCellDef="let sale">{{sale.customerName || 'Walk-in'}}</td>
                </ng-container>

                <ng-container matColumnDef="date">
                  <th mat-header-cell *matHeaderCellDef>Date</th>
                  <td mat-cell *matCellDef="let sale">{{sale.saleDate | date:'short'}}</td>
                </ng-container>

                <ng-container matColumnDef="items">
                  <th mat-header-cell *matHeaderCellDef>Items</th>
                  <td mat-cell *matCellDef="let sale">{{sale.itemCount}}</td>
                </ng-container>

                <ng-container matColumnDef="total">
                  <th mat-header-cell *matHeaderCellDef>Total</th>
                  <td mat-cell *matCellDef="let sale" class="font-semibold">{{sale.totalAmount | currency}}</td>
                </ng-container>

                <ng-container matColumnDef="status">
                  <th mat-header-cell *matHeaderCellDef>Status</th>
                  <td mat-cell *matCellDef="let sale">
                    <span [class]="getStatusClass(sale.status)"
                          class="px-2 py-1 rounded-full text-xs">
                      {{sale.status}}
                    </span>
                  </td>
                </ng-container>

                <ng-container matColumnDef="actions">
                  <th mat-header-cell *matHeaderCellDef>Actions</th>
                  <td mat-cell *matCellDef="let sale">
                    <button mat-icon-button [routerLink]="[sale.id]" matTooltip="View Details">
                      <mat-icon>visibility</mat-icon>
                    </button>
                    <button mat-icon-button matTooltip="Print Invoice">
                      <mat-icon>print</mat-icon>
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
export class SalesComponent implements OnInit {
  private apiService = inject(ApiService);

  sales = signal<Sale[]>([]);
  totalCount = signal(0);
  isLoading = signal(false);

  pageNumber = 1;
  pageSize = 10;

  displayedColumns = ['invoiceNumber', 'customer', 'date', 'items', 'total', 'status', 'actions'];

  ngOnInit(): void {
    this.loadSales();
  }

  loadSales(): void {
    this.isLoading.set(true);
    this.apiService.get<PagedResult<Sale>>('/api/sales', {
      pageNumber: this.pageNumber,
      pageSize: this.pageSize
    }).subscribe({
      next: (result) => {
        this.sales.set(result.items);
        this.totalCount.set(result.totalCount);
        this.isLoading.set(false);
      },
      error: () => {
        this.isLoading.set(false);
      }
    });
  }

  onPageChange(event: PageEvent): void {
    this.pageNumber = event.pageIndex + 1;
    this.pageSize = event.pageSize;
    this.loadSales();
  }

  getStatusClass(status: string): string {
    switch (status) {
      case 'Completed': return 'bg-green-100 text-green-800';
      case 'Held': return 'bg-yellow-100 text-yellow-800';
      case 'Voided': return 'bg-red-100 text-red-800';
      case 'Returned': return 'bg-purple-100 text-purple-800';
      default: return 'bg-gray-100 text-gray-800';
    }
  }
}
