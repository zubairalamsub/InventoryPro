import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, ActivatedRoute, RouterModule } from '@angular/router';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { ApiService } from '../../core/services/api.service';

@Component({
  selector: 'app-product-form',
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
    MatCheckboxModule,
    MatProgressSpinnerModule,
    MatSnackBarModule
  ],
  template: `
    <div class="max-w-2xl mx-auto">
      <div class="flex items-center gap-4 mb-6">
        <button mat-icon-button routerLink="/products">
          <mat-icon>arrow_back</mat-icon>
        </button>
        <h1 class="text-2xl font-bold text-gray-800">
          {{ isEditMode() ? 'Edit Product' : 'Add Product' }}
        </h1>
      </div>

      <mat-card>
        <mat-card-content class="p-6">
          <form [formGroup]="form" (ngSubmit)="onSubmit()">
            <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
              <mat-form-field appearance="outline" class="w-full">
                <mat-label>Product Name</mat-label>
                <input matInput formControlName="name" placeholder="Enter product name">
                @if (form.get('name')?.hasError('required') && form.get('name')?.touched) {
                  <mat-error>Name is required</mat-error>
                }
              </mat-form-field>

              <mat-form-field appearance="outline" class="w-full">
                <mat-label>SKU</mat-label>
                <input matInput formControlName="sku" placeholder="Enter SKU">
                @if (form.get('sku')?.hasError('required') && form.get('sku')?.touched) {
                  <mat-error>SKU is required</mat-error>
                }
              </mat-form-field>

              <mat-form-field appearance="outline" class="w-full">
                <mat-label>Barcode</mat-label>
                <input matInput formControlName="barcode" placeholder="Enter barcode (optional)">
              </mat-form-field>

              <mat-form-field appearance="outline" class="w-full">
                <mat-label>Cost Price</mat-label>
                <input matInput type="number" formControlName="costPrice" placeholder="0.00">
                <span matPrefix>$&nbsp;</span>
              </mat-form-field>

              <mat-form-field appearance="outline" class="w-full">
                <mat-label>Selling Price</mat-label>
                <input matInput type="number" formControlName="sellingPrice" placeholder="0.00">
                <span matPrefix>$&nbsp;</span>
              </mat-form-field>

              <mat-form-field appearance="outline" class="w-full">
                <mat-label>Reorder Level</mat-label>
                <input matInput type="number" formControlName="reorderLevel" placeholder="10">
              </mat-form-field>

              <mat-form-field appearance="outline" class="w-full md:col-span-2">
                <mat-label>Description</mat-label>
                <textarea matInput formControlName="description" rows="3" placeholder="Product description"></textarea>
              </mat-form-field>

              <div class="md:col-span-2">
                <mat-checkbox formControlName="isActive">Active</mat-checkbox>
              </div>
            </div>

            <div class="flex justify-end gap-4 mt-6">
              <button mat-stroked-button type="button" routerLink="/products">Cancel</button>
              <button mat-raised-button color="primary" type="submit" [disabled]="isLoading() || form.invalid">
                @if (isLoading()) {
                  <mat-spinner diameter="20"></mat-spinner>
                } @else {
                  {{ isEditMode() ? 'Update' : 'Create' }} Product
                }
              </button>
            </div>
          </form>
        </mat-card-content>
      </mat-card>
    </div>
  `
})
export class ProductFormComponent implements OnInit {
  private fb = inject(FormBuilder);
  private router = inject(Router);
  private route = inject(ActivatedRoute);
  private apiService = inject(ApiService);
  private snackBar = inject(MatSnackBar);

  isEditMode = signal(false);
  isLoading = signal(false);
  productId: string | null = null;

  form: FormGroup = this.fb.group({
    name: ['', Validators.required],
    sku: ['', Validators.required],
    barcode: [''],
    description: [''],
    costPrice: [0, [Validators.required, Validators.min(0)]],
    sellingPrice: [0, [Validators.required, Validators.min(0)]],
    reorderLevel: [10, Validators.min(0)],
    isActive: [true]
  });

  ngOnInit(): void {
    this.productId = this.route.snapshot.paramMap.get('id');
    if (this.productId && this.productId !== 'new') {
      this.isEditMode.set(true);
      this.loadProduct();
    }
  }

  loadProduct(): void {
    this.isLoading.set(true);
    this.apiService.get<any>(`/api/products/${this.productId}`).subscribe({
      next: (product) => {
        this.form.patchValue(product);
        this.isLoading.set(false);
      },
      error: () => {
        this.snackBar.open('Failed to load product', 'Close', { duration: 3000 });
        this.isLoading.set(false);
      }
    });
  }

  onSubmit(): void {
    if (this.form.invalid) return;

    this.isLoading.set(true);
    const formValue = this.form.value;

    // Build request data - isActive only supported on update
    const data: any = {
      name: formValue.name,
      sku: formValue.sku,
      costPrice: formValue.costPrice,
      sellingPrice: formValue.sellingPrice,
      barcode: formValue.barcode || null,
      description: formValue.description || null,
      reorderLevel: formValue.reorderLevel || 10
    };

    if (this.isEditMode()) {
      data.isActive = formValue.isActive;
    }

    const request = this.isEditMode()
      ? this.apiService.put(`/api/products/${this.productId}`, data)
      : this.apiService.post('/api/products', data);

    request.subscribe({
      next: () => {
        this.snackBar.open(
          `Product ${this.isEditMode() ? 'updated' : 'created'} successfully`,
          'Close',
          { duration: 3000 }
        );
        this.router.navigate(['/products']);
      },
      error: () => {
        this.snackBar.open('Failed to save product', 'Close', { duration: 3000 });
        this.isLoading.set(false);
      }
    });
  }
}
