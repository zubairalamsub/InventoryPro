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
import { MatAutocompleteModule } from '@angular/material/autocomplete';
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
  selector: 'app-stock-adjustment',
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
    MatSnackBarModule,
    MatAutocompleteModule
  ],
  template: `
    <div class="max-w-2xl mx-auto">
      <div class="flex items-center gap-4 mb-6">
        <button mat-icon-button routerLink="/inventory">
          <mat-icon>arrow_back</mat-icon>
        </button>
        <h1 class="text-2xl font-bold text-gray-800">Stock Adjustment</h1>
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
                <mat-label>Warehouse</mat-label>
                <mat-select formControlName="warehouseId">
                  @for (warehouse of warehouses(); track warehouse.id) {
                    <mat-option [value]="warehouse.id">{{warehouse.name}}</mat-option>
                  }
                </mat-select>
                @if (form.get('warehouseId')?.hasError('required') && form.get('warehouseId')?.touched) {
                  <mat-error>Warehouse is required</mat-error>
                }
              </mat-form-field>

              <mat-form-field appearance="outline" class="w-full">
                <mat-label>Adjustment Type</mat-label>
                <mat-select formControlName="adjustmentType">
                  <mat-option value="add">Add Stock</mat-option>
                  <mat-option value="remove">Remove Stock</mat-option>
                  <mat-option value="set">Set Quantity</mat-option>
                </mat-select>
              </mat-form-field>

              <mat-form-field appearance="outline" class="w-full">
                <mat-label>Quantity</mat-label>
                <input matInput type="number" formControlName="quantity" placeholder="0">
                @if (form.get('quantity')?.hasError('required') && form.get('quantity')?.touched) {
                  <mat-error>Quantity is required</mat-error>
                }
                @if (form.get('quantity')?.hasError('min')) {
                  <mat-error>Quantity must be positive</mat-error>
                }
              </mat-form-field>

              <mat-form-field appearance="outline" class="w-full">
                <mat-label>Reason</mat-label>
                <mat-select formControlName="reason">
                  <mat-option value="damaged">Damaged</mat-option>
                  <mat-option value="lost">Lost</mat-option>
                  <mat-option value="found">Found</mat-option>
                  <mat-option value="returned">Returned</mat-option>
                  <mat-option value="correction">Inventory Correction</mat-option>
                  <mat-option value="other">Other</mat-option>
                </mat-select>
              </mat-form-field>

              <mat-form-field appearance="outline" class="w-full md:col-span-2">
                <mat-label>Notes</mat-label>
                <textarea matInput formControlName="notes" rows="3" placeholder="Additional notes (optional)"></textarea>
              </mat-form-field>
            </div>

            <div class="flex justify-end gap-4 mt-6">
              <button mat-stroked-button type="button" routerLink="/inventory">Cancel</button>
              <button mat-raised-button color="primary" type="submit" [disabled]="isLoading() || form.invalid">
                @if (isLoading()) {
                  <mat-spinner diameter="20"></mat-spinner>
                } @else {
                  Submit Adjustment
                }
              </button>
            </div>
          </form>
        </mat-card-content>
      </mat-card>
    </div>
  `
})
export class StockAdjustmentComponent implements OnInit {
  private fb = inject(FormBuilder);
  private router = inject(Router);
  private apiService = inject(ApiService);
  private snackBar = inject(MatSnackBar);

  isLoading = signal(false);
  products = signal<Product[]>([]);
  warehouses = signal<Warehouse[]>([]);

  form: FormGroup = this.fb.group({
    productId: ['', Validators.required],
    warehouseId: ['', Validators.required],
    adjustmentType: ['add', Validators.required],
    quantity: [1, [Validators.required, Validators.min(1)]],
    reason: ['correction', Validators.required],
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

    this.apiService.post('/api/inventory/adjustments', data).subscribe({
      next: () => {
        this.snackBar.open('Stock adjustment saved successfully', 'Close', { duration: 3000 });
        this.router.navigate(['/inventory']);
      },
      error: () => {
        this.snackBar.open('Failed to save stock adjustment', 'Close', { duration: 3000 });
        this.isLoading.set(false);
      }
    });
  }
}
