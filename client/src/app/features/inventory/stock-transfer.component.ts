import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSelectModule } from '@angular/material/select';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { ApiService } from '../../core/services/api.service';

interface Product {
  id: string;
  name: string;
  sku: string;
}

interface Warehouse {
  id: string;
  name: string;
}

@Component({
  selector: 'app-stock-transfer',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    ReactiveFormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatSelectModule,
    MatProgressSpinnerModule,
    MatSnackBarModule
  ],
  template: `
    <div class="max-w-2xl mx-auto">
      <div class="flex items-center gap-4 mb-6">
        <button mat-icon-button routerLink="/inventory">
          <mat-icon>arrow_back</mat-icon>
        </button>
        <h1 class="text-2xl font-bold text-gray-800">Stock Transfer</h1>
      </div>

      <mat-card>
        <mat-card-content class="p-6">
          <form [formGroup]="form" (ngSubmit)="onSubmit()">
            <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
              <mat-form-field appearance="outline" class="w-full md:col-span-2">
                <mat-label>Product</mat-label>
                <mat-select formControlName="productId">
                  @for (product of products(); track product.id) {
                    <mat-option [value]="product.id">
                      {{product.name}} ({{product.sku}})
                    </mat-option>
                  }
                </mat-select>
                @if (form.get('productId')?.hasError('required') && form.get('productId')?.touched) {
                  <mat-error>Product is required</mat-error>
                }
              </mat-form-field>

              <mat-form-field appearance="outline" class="w-full">
                <mat-label>From Warehouse</mat-label>
                <mat-select formControlName="fromWarehouseId">
                  @for (warehouse of warehouses(); track warehouse.id) {
                    <mat-option [value]="warehouse.id" [disabled]="warehouse.id === form.get('toWarehouseId')?.value">
                      {{warehouse.name}}
                    </mat-option>
                  }
                </mat-select>
                @if (form.get('fromWarehouseId')?.hasError('required') && form.get('fromWarehouseId')?.touched) {
                  <mat-error>Source warehouse is required</mat-error>
                }
              </mat-form-field>

              <mat-form-field appearance="outline" class="w-full">
                <mat-label>To Warehouse</mat-label>
                <mat-select formControlName="toWarehouseId">
                  @for (warehouse of warehouses(); track warehouse.id) {
                    <mat-option [value]="warehouse.id" [disabled]="warehouse.id === form.get('fromWarehouseId')?.value">
                      {{warehouse.name}}
                    </mat-option>
                  }
                </mat-select>
                @if (form.get('toWarehouseId')?.hasError('required') && form.get('toWarehouseId')?.touched) {
                  <mat-error>Destination warehouse is required</mat-error>
                }
              </mat-form-field>

              <mat-form-field appearance="outline" class="w-full">
                <mat-label>Quantity</mat-label>
                <input matInput type="number" formControlName="quantity" placeholder="0">
                @if (form.get('quantity')?.hasError('required') && form.get('quantity')?.touched) {
                  <mat-error>Quantity is required</mat-error>
                }
                @if (form.get('quantity')?.hasError('min')) {
                  <mat-error>Quantity must be at least 1</mat-error>
                }
              </mat-form-field>

              <mat-form-field appearance="outline" class="w-full md:col-span-2">
                <mat-label>Notes</mat-label>
                <textarea matInput formControlName="notes" rows="3" placeholder="Transfer notes (optional)"></textarea>
              </mat-form-field>
            </div>

            <div class="flex justify-end gap-4 mt-6">
              <button mat-stroked-button type="button" routerLink="/inventory">Cancel</button>
              <button mat-raised-button color="primary" type="submit" [disabled]="isLoading() || form.invalid">
                @if (isLoading()) {
                  <mat-spinner diameter="20"></mat-spinner>
                } @else {
                  Transfer Stock
                }
              </button>
            </div>
          </form>
        </mat-card-content>
      </mat-card>
    </div>
  `
})
export class StockTransferComponent implements OnInit {
  private fb = inject(FormBuilder);
  private router = inject(Router);
  private apiService = inject(ApiService);
  private snackBar = inject(MatSnackBar);

  isLoading = signal(false);
  products = signal<Product[]>([]);
  warehouses = signal<Warehouse[]>([]);

  form: FormGroup = this.fb.group({
    productId: ['', Validators.required],
    fromWarehouseId: ['', Validators.required],
    toWarehouseId: ['', Validators.required],
    quantity: [1, [Validators.required, Validators.min(1)]],
    notes: ['']
  });

  ngOnInit(): void {
    this.loadProducts();
    this.loadWarehouses();
  }

  loadProducts(): void {
    this.apiService.get<any>('/api/products', { pageSize: 1000 }).subscribe({
      next: (result) => {
        this.products.set(result.items || []);
      }
    });
  }

  loadWarehouses(): void {
    this.apiService.get<any>('/api/warehouses', { pageSize: 100 }).subscribe({
      next: (result) => {
        this.warehouses.set(result.items || []);
      }
    });
  }

  onSubmit(): void {
    if (this.form.invalid) return;

    this.isLoading.set(true);
    const data = this.form.value;

    this.apiService.post('/api/inventory/transfers', data).subscribe({
      next: () => {
        this.snackBar.open('Stock transfer completed successfully', 'Close', { duration: 3000 });
        this.router.navigate(['/inventory']);
      },
      error: () => {
        this.snackBar.open('Failed to complete stock transfer', 'Close', { duration: 3000 });
        this.isLoading.set(false);
      }
    });
  }
}
