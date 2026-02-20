import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterModule, ActivatedRoute } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule
  ],
  template: `
    <div class="auth-container">
      <div class="auth-card">
        <!-- Logo & Header -->
        <div class="auth-header">
          <div class="logo-container">
            <mat-icon class="logo-icon">inventory_2</mat-icon>
          </div>
          <h1 class="auth-title">Welcome Back</h1>
          <p class="auth-subtitle">Sign in to your InventoryPro account</p>
        </div>

        <!-- Form -->
        <form [formGroup]="loginForm" (ngSubmit)="onSubmit()" class="auth-form">
          <div class="form-group">
            <label class="form-label">Email Address</label>
            <div class="input-wrapper" [class.error]="loginForm.get('email')?.invalid && loginForm.get('email')?.touched">
              <mat-icon class="input-icon">mail_outline</mat-icon>
              <input type="email" formControlName="email" placeholder="you@company.com" class="form-input">
            </div>
            @if (loginForm.get('email')?.hasError('required') && loginForm.get('email')?.touched) {
              <span class="error-text">Email is required</span>
            }
            @if (loginForm.get('email')?.hasError('email') && loginForm.get('email')?.touched) {
              <span class="error-text">Please enter a valid email</span>
            }
          </div>

          <div class="form-group">
            <label class="form-label">Password</label>
            <div class="input-wrapper" [class.error]="loginForm.get('password')?.invalid && loginForm.get('password')?.touched">
              <mat-icon class="input-icon">lock_outline</mat-icon>
              <input [type]="hidePassword() ? 'password' : 'text'" formControlName="password" placeholder="Enter your password" class="form-input">
              <button type="button" class="toggle-password" (click)="hidePassword.set(!hidePassword())">
                <mat-icon>{{hidePassword() ? 'visibility_off' : 'visibility'}}</mat-icon>
              </button>
            </div>
            @if (loginForm.get('password')?.hasError('required') && loginForm.get('password')?.touched) {
              <span class="error-text">Password is required</span>
            }
          </div>

          @if (errorMessage()) {
            <div class="error-alert">
              <mat-icon>error_outline</mat-icon>
              <span>{{errorMessage()}}</span>
            </div>
          }

          <button type="submit" class="submit-btn" [disabled]="loginForm.invalid || isLoading()">
            @if (isLoading()) {
              <mat-spinner diameter="20"></mat-spinner>
            } @else {
              <span>Sign In</span>
              <mat-icon>arrow_forward</mat-icon>
            }
          </button>
        </form>

        <!-- Footer -->
        <div class="auth-footer">
          <p>Don't have an account? <a routerLink="/auth/register">Create account</a></p>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .auth-container {
      min-height: 100vh;
      display: flex;
      align-items: center;
      justify-content: center;
      background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
      padding: 20px;
    }

    .auth-card {
      width: 100%;
      max-width: 420px;
      background: white;
      border-radius: 24px;
      box-shadow: 0 25px 50px -12px rgba(0, 0, 0, 0.25);
      padding: 48px 40px;
      animation: slideUp 0.4s ease-out;
    }

    @keyframes slideUp {
      from {
        opacity: 0;
        transform: translateY(20px);
      }
      to {
        opacity: 1;
        transform: translateY(0);
      }
    }

    .auth-header {
      text-align: center;
      margin-bottom: 36px;
    }

    .logo-container {
      width: 72px;
      height: 72px;
      background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
      border-radius: 20px;
      display: flex;
      align-items: center;
      justify-content: center;
      margin: 0 auto 20px;
      box-shadow: 0 10px 30px -10px rgba(102, 126, 234, 0.5);
    }

    .logo-icon {
      font-size: 36px;
      width: 36px;
      height: 36px;
      color: white;
    }

    .auth-title {
      font-size: 28px;
      font-weight: 700;
      color: #1a1a2e;
      margin: 0 0 8px;
    }

    .auth-subtitle {
      font-size: 15px;
      color: #6b7280;
      margin: 0;
    }

    .auth-form {
      display: flex;
      flex-direction: column;
      gap: 20px;
    }

    .form-group {
      display: flex;
      flex-direction: column;
      gap: 8px;
    }

    .form-label {
      font-size: 14px;
      font-weight: 600;
      color: #374151;
    }

    .input-wrapper {
      position: relative;
      display: flex;
      align-items: center;
      background: #f9fafb;
      border: 2px solid #e5e7eb;
      border-radius: 12px;
      transition: all 0.2s ease;
    }

    .input-wrapper:focus-within {
      border-color: #667eea;
      background: white;
      box-shadow: 0 0 0 4px rgba(102, 126, 234, 0.1);
    }

    .input-wrapper.error {
      border-color: #ef4444;
    }

    .input-icon {
      color: #9ca3af;
      margin-left: 14px;
      font-size: 20px;
      width: 20px;
      height: 20px;
    }

    .form-input {
      flex: 1;
      border: none;
      background: transparent;
      padding: 14px 12px;
      font-size: 15px;
      color: #1f2937;
      outline: none;
    }

    .form-input::placeholder {
      color: #9ca3af;
    }

    .toggle-password {
      background: none;
      border: none;
      padding: 8px 12px;
      cursor: pointer;
      color: #9ca3af;
      display: flex;
      align-items: center;
    }

    .toggle-password:hover {
      color: #6b7280;
    }

    .error-text {
      font-size: 13px;
      color: #ef4444;
      margin-top: -4px;
    }

    .error-alert {
      display: flex;
      align-items: center;
      gap: 10px;
      padding: 12px 16px;
      background: #fef2f2;
      border: 1px solid #fecaca;
      border-radius: 10px;
      color: #dc2626;
      font-size: 14px;
    }

    .error-alert mat-icon {
      font-size: 20px;
      width: 20px;
      height: 20px;
    }

    .submit-btn {
      display: flex;
      align-items: center;
      justify-content: center;
      gap: 8px;
      width: 100%;
      padding: 16px 24px;
      background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
      color: white;
      border: none;
      border-radius: 12px;
      font-size: 16px;
      font-weight: 600;
      cursor: pointer;
      transition: all 0.2s ease;
      margin-top: 8px;
    }

    .submit-btn:hover:not(:disabled) {
      transform: translateY(-2px);
      box-shadow: 0 10px 30px -10px rgba(102, 126, 234, 0.5);
    }

    .submit-btn:disabled {
      opacity: 0.6;
      cursor: not-allowed;
    }

    .submit-btn mat-icon {
      font-size: 20px;
      width: 20px;
      height: 20px;
    }

    .auth-footer {
      text-align: center;
      margin-top: 32px;
      padding-top: 24px;
      border-top: 1px solid #e5e7eb;
    }

    .auth-footer p {
      color: #6b7280;
      font-size: 14px;
      margin: 0;
    }

    .auth-footer a {
      color: #667eea;
      font-weight: 600;
      text-decoration: none;
      transition: color 0.2s;
    }

    .auth-footer a:hover {
      color: #764ba2;
    }

    @media (max-width: 480px) {
      .auth-card {
        padding: 36px 24px;
        border-radius: 20px;
      }

      .auth-title {
        font-size: 24px;
      }
    }
  `]
})
export class LoginComponent {
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private router = inject(Router);
  private route = inject(ActivatedRoute);

  hidePassword = signal(true);
  isLoading = signal(false);
  errorMessage = signal('');

  loginForm: FormGroup = this.fb.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required]]
  });

  onSubmit(): void {
    if (this.loginForm.invalid) return;

    this.isLoading.set(true);
    this.errorMessage.set('');

    this.authService.login(this.loginForm.value).subscribe({
      next: () => {
        const returnUrl = this.route.snapshot.queryParams['returnUrl'] || '/dashboard';
        this.router.navigateByUrl(returnUrl);
      },
      error: (error) => {
        this.isLoading.set(false);
        this.errorMessage.set(error.error?.message || 'Login failed. Please check your credentials.');
      }
    });
  }
}
