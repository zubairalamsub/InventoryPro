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
  selector: 'app-customer-form',
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
        <button mat-icon-button routerLink="/customers">
          <mat-icon>arrow_back</mat-icon>
        </button>
        <h1 class="text-2xl font-bold text-gray-800">
          {{ isEditMode() ? 'Edit Customer' : 'Add Customer' }}
        </h1>
      </div>

      <mat-card>
        <mat-card-content class="p-6">
          <form [formGroup]="form" (ngSubmit)="onSubmit()">
            <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
              <mat-form-field appearance="outline" class="w-full md:col-span-2">
                <mat-label>Customer Name</mat-label>
                <input matInput formControlName="name" placeholder="Enter customer name">
                @if (form.get('name')?.hasError('required') && form.get('name')?.touched) {
                  <mat-error>Name is required</mat-error>
                }
              </mat-form-field>

              <mat-form-field appearance="outline" class="w-full">
                <mat-label>Email</mat-label>
                <input matInput type="email" formControlName="email" placeholder="email@example.com">
              </mat-form-field>

              <mat-form-field appearance="outline" class="w-full">
                <mat-label>Phone</mat-label>
                <input matInput formControlName="phone" placeholder="+1 234 567 8900">
              </mat-form-field>

              <mat-form-field appearance="outline" class="w-full md:col-span-2">
                <mat-label>Address</mat-label>
                <textarea matInput formControlName="address" rows="2" placeholder="Street address"></textarea>
              </mat-form-field>

              <mat-form-field appearance="outline" class="w-full">
                <mat-label>City</mat-label>
                <input matInput formControlName="city" placeholder="City">
              </mat-form-field>

              <mat-form-field appearance="outline" class="w-full">
                <mat-label>Country</mat-label>
                <input matInput formControlName="country" placeholder="Country">
              </mat-form-field>

              <mat-form-field appearance="outline" class="w-full">
                <mat-label>Credit Limit</mat-label>
                <input matInput type="number" formControlName="creditLimit" placeholder="0.00">
                <span matPrefix>$&nbsp;</span>
              </mat-form-field>

              <mat-form-field appearance="outline" class="w-full">
                <mat-label>Tax Number</mat-label>
                <input matInput formControlName="taxNumber" placeholder="Tax ID (optional)">
              </mat-form-field>

              <div class="md:col-span-2">
                <mat-checkbox formControlName="isActive">Active</mat-checkbox>
              </div>
            </div>

            <div class="flex justify-end gap-4 mt-6">
              <button mat-stroked-button type="button" routerLink="/customers">Cancel</button>
              <button mat-raised-button color="primary" type="submit" [disabled]="isLoading() || form.invalid">
                @if (isLoading()) {
                  <mat-spinner diameter="20"></mat-spinner>
                } @else {
                  {{ isEditMode() ? 'Update' : 'Create' }} Customer
                }
              </button>
            </div>
          </form>
        </mat-card-content>
      </mat-card>
    </div>
  `
})
export class CustomerFormComponent implements OnInit {
  private fb = inject(FormBuilder);
  private router = inject(Router);
  private route = inject(ActivatedRoute);
  private apiService = inject(ApiService);
  private snackBar = inject(MatSnackBar);

  isEditMode = signal(false);
  isLoading = signal(false);
  customerId: string | null = null;

  form: FormGroup = this.fb.group({
    name: ['', Validators.required],
    email: ['', Validators.email],
    phone: [''],
    address: [''],
    city: [''],
    country: [''],
    creditLimit: [0],
    taxNumber: [''],
    isActive: [true]
  });

  ngOnInit(): void {
    this.customerId = this.route.snapshot.paramMap.get('id');
    if (this.customerId && this.customerId !== 'new') {
      this.isEditMode.set(true);
      this.loadCustomer();
    }
  }

  loadCustomer(): void {
    this.isLoading.set(true);
    this.apiService.get<any>(`/api/customers/${this.customerId}`).subscribe({
      next: (customer) => {
        // Map API response to form fields
        this.form.patchValue({
          name: customer.name,
          email: customer.email,
          phone: customer.phone,
          address: customer.street,
          city: customer.city,
          country: customer.country,
          creditLimit: customer.creditLimit,
          taxNumber: customer.taxIdentificationNo,
          isActive: customer.isActive
        });
        this.isLoading.set(false);
      },
      error: () => {
        this.snackBar.open('Failed to load customer', 'Close', { duration: 3000 });
        this.isLoading.set(false);
      }
    });
  }

  onSubmit(): void {
    if (this.form.invalid) return;

    this.isLoading.set(true);
    const formValue = this.form.value;

    // Map form fields to API request format
    const data = {
      name: formValue.name,
      email: formValue.email || null,
      phone: formValue.phone || null,
      street: formValue.address || null,
      city: formValue.city || null,
      country: formValue.country || null,
      creditLimit: formValue.creditLimit || null,
      taxIdentificationNo: formValue.taxNumber || null,
      isActive: formValue.isActive
    };

    const request = this.isEditMode()
      ? this.apiService.put(`/api/customers/${this.customerId}`, data)
      : this.apiService.post('/api/customers', data);

    request.subscribe({
      next: () => {
        this.snackBar.open(
          `Customer ${this.isEditMode() ? 'updated' : 'created'} successfully`,
          'Close',
          { duration: 3000 }
        );
        this.router.navigate(['/customers']);
      },
      error: () => {
        this.snackBar.open('Failed to save customer', 'Close', { duration: 3000 });
        this.isLoading.set(false);
      }
    });
  }
}
