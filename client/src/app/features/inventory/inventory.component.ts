import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MatTableModule } from '@angular/material/table';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTabsModule } from '@angular/material/tabs';
import { MatSelectModule } from '@angular/material/select';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { FormsModule } from '@angular/forms';
import { ApiService, PagedResult } from '../../core/services/api.service';

interface StockLevel {
  id: string;
  productId: string;
  productName: string;
  productSKU: string;
  warehouseId: string;
  warehouseName: string;
  quantity: number;
  reservedQuantity: number;
  availableQuantity: number;
  reorderLevel: number;
  isLowStock: boolean;
  lastUpdated: string;
}

@Component({
  selector: 'app-inventory',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    FormsModule,
    MatTableModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatTabsModule,
    MatSelectModule,
    MatFormFieldModule,
    MatPaginatorModule,
    MatProgressSpinnerModule
  ],
  template: `
    <div class="space-y-6">
      <div class="flex justify-between items-center">
        <h1 class="text-2xl font-bold text-gray-800">Inventory</h1>
        <div class="space-x-2">
          <button mat-stroked-button color="primary" routerLink="adjustment">
            <mat-icon>tune</mat-icon>
            Stock Adjustment
          </button>
          <button mat-raised-button color="primary" routerLink="transfer">
            <mat-icon>swap_horiz</mat-icon>
            Stock Transfer
          </button>
        </div>
      </div>

      <mat-card>
        <mat-card-content class="p-4">
          <mat-tab-group>
            <mat-tab label="Stock Levels">
              <div class="p-4">
                <!-- Filters -->
                <div class="flex gap-4 mb-4">
                  <mat-form-field appearance="outline">
                    <mat-label>Filter</mat-label>
                    <mat-select [(ngModel)]="stockFilter" (selectionChange)="loadStockLevels()">
                      <mat-option value="all">All Stock</mat-option>
                      <mat-option value="low">Low Stock Only</mat-option>
                    </mat-select>
                  </mat-form-field>
                </div>

                @if (isLoading()) {
                  <div class="flex justify-center p-8">
                    <mat-spinner></mat-spinner>
                  </div>
                } @else {
                  <div class="overflow-x-auto">
                    <table mat-table [dataSource]="stockLevels()" class="w-full">
                      <ng-container matColumnDef="product">
                        <th mat-header-cell *matHeaderCellDef>Product</th>
                        <td mat-cell *matCellDef="let stock">
                          <div>
                            <div class="font-medium">{{stock.productName}}</div>
                            <div class="text-sm text-gray-500">{{stock.productSKU}}</div>
                          </div>
                        </td>
                      </ng-container>

                      <ng-container matColumnDef="warehouse">
                        <th mat-header-cell *matHeaderCellDef>Warehouse</th>
                        <td mat-cell *matCellDef="let stock">{{stock.warehouseName}}</td>
                      </ng-container>

                      <ng-container matColumnDef="quantity">
                        <th mat-header-cell *matHeaderCellDef>Total Qty</th>
                        <td mat-cell *matCellDef="let stock">{{stock.quantity}}</td>
                      </ng-container>

                      <ng-container matColumnDef="available">
                        <th mat-header-cell *matHeaderCellDef>Available</th>
                        <td mat-cell *matCellDef="let stock">{{stock.availableQuantity}}</td>
                      </ng-container>

                      <ng-container matColumnDef="reserved">
                        <th mat-header-cell *matHeaderCellDef>Reserved</th>
                        <td mat-cell *matCellDef="let stock">{{stock.reservedQuantity}}</td>
                      </ng-container>

                      <ng-container matColumnDef="status">
                        <th mat-header-cell *matHeaderCellDef>Status</th>
                        <td mat-cell *matCellDef="let stock">
                          @if (stock.isLowStock) {
                            <span class="bg-red-100 text-red-800 px-2 py-1 rounded-full text-xs">
                              Low Stock
                            </span>
                          } @else {
                            <span class="bg-green-100 text-green-800 px-2 py-1 rounded-full text-xs">
                              In Stock
                            </span>
                          }
                        </td>
                      </ng-container>

                      <tr mat-header-row *matHeaderRowDef="stockColumns"></tr>
                      <tr mat-row *matRowDef="let row; columns: stockColumns;"
                          [class.bg-red-50]="row.isLowStock"></tr>
                    </table>
                  </div>

                  <mat-paginator
                    [length]="totalCount()"
                    [pageSize]="pageSize"
                    [pageSizeOptions]="[10, 25, 50]"
                    (page)="onPageChange($event)">
                  </mat-paginator>
                }
              </div>
            </mat-tab>

            <mat-tab label="Transactions">
              <div class="p-4">
                <p class="text-gray-500 text-center py-8">Transaction history will be displayed here</p>
              </div>
            </mat-tab>
          </mat-tab-group>
        </mat-card-content>
      </mat-card>
    </div>
  `
})
export class InventoryComponent implements OnInit {
  private apiService = inject(ApiService);

  stockLevels = signal<StockLevel[]>([]);
  totalCount = signal(0);
  isLoading = signal(false);

  stockFilter = 'all';
  pageNumber = 1;
  pageSize = 10;

  stockColumns = ['product', 'warehouse', 'quantity', 'available', 'reserved', 'status'];

  ngOnInit(): void {
    this.loadStockLevels();
  }

  loadStockLevels(): void {
    this.isLoading.set(true);
    this.apiService.get<PagedResult<StockLevel>>('/api/inventory/stock-levels', {
      lowStockOnly: this.stockFilter === 'low' ? true : undefined,
      pageNumber: this.pageNumber,
      pageSize: this.pageSize
    }).subscribe({
      next: (result) => {
        this.stockLevels.set(result.items);
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
    this.loadStockLevels();
  }
}
