import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-sale-form',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule
  ],
  template: `
    <div class="max-w-4xl mx-auto">
      <div class="flex items-center gap-4 mb-6">
        <button mat-icon-button routerLink="/sales">
          <mat-icon>arrow_back</mat-icon>
        </button>
        <h1 class="text-2xl font-bold text-gray-800">New Sale</h1>
      </div>

      <mat-card>
        <mat-card-content class="p-6">
          <div class="text-center py-12">
            <mat-icon class="text-6xl text-gray-400">point_of_sale</mat-icon>
            <h2 class="text-xl font-semibold text-gray-700 mt-4">Point of Sale</h2>
            <p class="text-gray-500 mt-2">
              The full POS interface with product search, cart management, and payment processing
              will be available in the next update.
            </p>
            <button mat-raised-button color="primary" routerLink="/sales" class="mt-6">
              Back to Sales
            </button>
          </div>
        </mat-card-content>
      </mat-card>
    </div>
  `
})
export class SaleFormComponent {}
