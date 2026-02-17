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

interface Product {
  id: string;
  name: string;
  sku: string;
  barcode?: string;
  costPrice: number;
  sellingPrice: number;
  reorderLevel: number;
  isActive: boolean;
  createdAt: string;
}

@Component({
  selector: 'app-products',
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
        <h1 class="text-2xl font-bold text-gray-800">Products</h1>
        <button mat-raised-button color="primary" routerLink="new">
          <mat-icon>add</mat-icon>
          Add Product
        </button>
      </div>

      <mat-card>
        <mat-card-content class="p-4">
          <!-- Search -->
          <mat-form-field appearance="outline" class="w-full md:w-96 mb-4">
            <mat-label>Search products</mat-label>
            <input matInput [(ngModel)]="searchTerm" (keyup.enter)="search()" placeholder="Search by name, SKU...">
            <mat-icon matSuffix>search</mat-icon>
          </mat-form-field>

          @if (isLoading()) {
            <div class="flex justify-center p-8">
              <mat-spinner></mat-spinner>
            </div>
          } @else {
            <div class="overflow-x-auto">
              <table mat-table [dataSource]="products()" class="w-full">
                <ng-container matColumnDef="name">
                  <th mat-header-cell *matHeaderCellDef>Name</th>
                  <td mat-cell *matCellDef="let product">{{product.name}}</td>
                </ng-container>

                <ng-container matColumnDef="sku">
                  <th mat-header-cell *matHeaderCellDef>SKU</th>
                  <td mat-cell *matCellDef="let product">{{product.sku}}</td>
                </ng-container>

                <ng-container matColumnDef="costPrice">
                  <th mat-header-cell *matHeaderCellDef>Cost Price</th>
                  <td mat-cell *matCellDef="let product">{{product.costPrice | currency}}</td>
                </ng-container>

                <ng-container matColumnDef="sellingPrice">
                  <th mat-header-cell *matHeaderCellDef>Selling Price</th>
                  <td mat-cell *matCellDef="let product">{{product.sellingPrice | currency}}</td>
                </ng-container>

                <ng-container matColumnDef="status">
                  <th mat-header-cell *matHeaderCellDef>Status</th>
                  <td mat-cell *matCellDef="let product">
                    <span [class]="product.isActive ? 'bg-green-100 text-green-800' : 'bg-red-100 text-red-800'"
                          class="px-2 py-1 rounded-full text-xs">
                      {{product.isActive ? 'Active' : 'Inactive'}}
                    </span>
                  </td>
                </ng-container>

                <ng-container matColumnDef="actions">
                  <th mat-header-cell *matHeaderCellDef>Actions</th>
                  <td mat-cell *matCellDef="let product">
                    <button mat-icon-button [routerLink]="[product.id]">
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
export class ProductsComponent implements OnInit {
  private apiService = inject(ApiService);

  products = signal<Product[]>([]);
  totalCount = signal(0);
  isLoading = signal(false);

  searchTerm = '';
  pageNumber = 1;
  pageSize = 10;

  displayedColumns = ['name', 'sku', 'costPrice', 'sellingPrice', 'status', 'actions'];

  ngOnInit(): void {
    this.loadProducts();
  }

  loadProducts(): void {
    this.isLoading.set(true);
    this.apiService.get<PagedResult<Product>>('/api/products', {
      searchTerm: this.searchTerm || undefined,
      pageNumber: this.pageNumber,
      pageSize: this.pageSize
    }).subscribe({
      next: (result) => {
        this.products.set(result.items);
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
    this.loadProducts();
  }

  onPageChange(event: PageEvent): void {
    this.pageNumber = event.pageIndex + 1;
    this.pageSize = event.pageSize;
    this.loadProducts();
  }
}
