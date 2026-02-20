import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-register',
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
            <mat-icon class="logo-icon">rocket_launch</mat-icon>
          </div>
          <h1 class="auth-title">Get Started</h1>
          <p class="auth-subtitle">Create your free InventoryPro account</p>
        </div>

        <!-- Form -->
        <form [formGroup]="registerForm" (ngSubmit)="onSubmit()" class="auth-form">
          <div class="form-row">
            <div class="form-group">
              <label class="form-label">Full Name</label>
              <div class="input-wrapper" [class.error]="registerForm.get('fullName')?.invalid && registerForm.get('fullName')?.touched">
                <mat-icon class="input-icon">person_outline</mat-icon>
                <input type="text" formControlName="fullName" placeholder="John Doe" class="form-input">
              </div>
              @if (registerForm.get('fullName')?.hasError('required') && registerForm.get('fullName')?.touched) {
                <span class="error-text">Full name is required</span>
              }
            </div>

            <div class="form-group">
              <label class="form-label">Business Name</label>
              <div class="input-wrapper" [class.error]="registerForm.get('tenantName')?.invalid && registerForm.get('tenantName')?.touched">
                <mat-icon class="input-icon">business</mat-icon>
                <input type="text" formControlName="tenantName" placeholder="Acme Inc." class="form-input">
              </div>
              @if (registerForm.get('tenantName')?.hasError('required') && registerForm.get('tenantName')?.touched) {
                <span class="error-text">Business name is required</span>
              }
            </div>
          </div>

          <div class="form-group">
            <label class="form-label">Email Address</label>
            <div class="input-wrapper" [class.error]="registerForm.get('email')?.invalid && registerForm.get('email')?.touched">
              <mat-icon class="input-icon">mail_outline</mat-icon>
              <input type="email" formControlName="email" placeholder="you@company.com" class="form-input">
            </div>
            @if (registerForm.get('email')?.hasError('required') && registerForm.get('email')?.touched) {
              <span class="error-text">Email is required</span>
            }
            @if (registerForm.get('email')?.hasError('email') && registerForm.get('email')?.touched) {
              <span class="error-text">Please enter a valid email</span>
            }
          </div>

          <div class="form-group">
            <label class="form-label">Password</label>
            <div class="input-wrapper" [class.error]="registerForm.get('password')?.invalid && registerForm.get('password')?.touched">
              <mat-icon class="input-icon">lock_outline</mat-icon>
              <input [type]="hidePassword() ? 'password' : 'text'" formControlName="password" placeholder="Min. 8 characters" class="form-input">
              <button type="button" class="toggle-password" (click)="hidePassword.set(!hidePassword())">
                <mat-icon>{{hidePassword() ? 'visibility_off' : 'visibility'}}</mat-icon>
              </button>
            </div>
            @if (registerForm.get('password')?.hasError('required') && registerForm.get('password')?.touched) {
              <span class="error-text">Password is required</span>
            }
            @if (registerForm.get('password')?.hasError('minlength') && registerForm.get('password')?.touched) {
              <span class="error-text">Password must be at least 8 characters</span>
            }
          </div>

          <div class="password-hints">
            <div class="hint" [class.valid]="registerForm.get('password')?.value?.length >= 8">
              <mat-icon>{{registerForm.get('password')?.value?.length >= 8 ? 'check_circle' : 'radio_button_unchecked'}}</mat-icon>
              <span>At least 8 characters</span>
            </div>
            <div class="hint" [class.valid]="hasUppercase()">
              <mat-icon>{{hasUppercase() ? 'check_circle' : 'radio_button_unchecked'}}</mat-icon>
              <span>One uppercase letter</span>
            </div>
            <div class="hint" [class.valid]="hasNumber()">
              <mat-icon>{{hasNumber() ? 'check_circle' : 'radio_button_unchecked'}}</mat-icon>
              <span>One number</span>
            </div>
            <div class="hint" [class.valid]="hasSpecial()">
              <mat-icon>{{hasSpecial() ? 'check_circle' : 'radio_button_unchecked'}}</mat-icon>
              <span>One special character</span>
            </div>
          </div>

          @if (errorMessage()) {
            <div class="error-alert">
              <mat-icon>error_outline</mat-icon>
              <span>{{errorMessage()}}</span>
            </div>
          }

          <button type="submit" class="submit-btn" [disabled]="registerForm.invalid || isLoading()">
            @if (isLoading()) {
              <mat-spinner diameter="20"></mat-spinner>
            } @else {
              <span>Create Account</span>
              <mat-icon>arrow_forward</mat-icon>
            }
          </button>

          <p class="terms-text">
            By creating an account, you agree to our
            <a href="#">Terms of Service</a> and <a href="#">Privacy Policy</a>
          </p>
        </form>

        <!-- Footer -->
        <div class="auth-footer">
          <p>Already have an account? <a routerLink="/auth/login">Sign in</a></p>
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
      max-width: 480px;
      background: white;
      border-radius: 24px;
      box-shadow: 0 25px 50px -12px rgba(0, 0, 0, 0.25);
      padding: 40px;
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
      margin-bottom: 32px;
    }

    .logo-container {
      width: 68px;
      height: 68px;
      background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
      border-radius: 18px;
      display: flex;
      align-items: center;
      justify-content: center;
      margin: 0 auto 16px;
      box-shadow: 0 10px 30px -10px rgba(102, 126, 234, 0.5);
    }

    .logo-icon {
      font-size: 32px;
      width: 32px;
      height: 32px;
      color: white;
    }

    .auth-title {
      font-size: 26px;
      font-weight: 700;
      color: #1a1a2e;
      margin: 0 0 6px;
    }

    .auth-subtitle {
      font-size: 14px;
      color: #6b7280;
      margin: 0;
    }

    .auth-form {
      display: flex;
      flex-direction: column;
      gap: 18px;
    }

    .form-row {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 16px;
    }

    .form-group {
      display: flex;
      flex-direction: column;
      gap: 6px;
    }

    .form-label {
      font-size: 13px;
      font-weight: 600;
      color: #374151;
    }

    .input-wrapper {
      position: relative;
      display: flex;
      align-items: center;
      background: #f9fafb;
      border: 2px solid #e5e7eb;
      border-radius: 10px;
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
      margin-left: 12px;
      font-size: 20px;
      width: 20px;
      height: 20px;
    }

    .form-input {
      flex: 1;
      border: none;
      background: transparent;
      padding: 12px 10px;
      font-size: 14px;
      color: #1f2937;
      outline: none;
    }

    .form-input::placeholder {
      color: #9ca3af;
    }

    .toggle-password {
      background: none;
      border: none;
      padding: 8px 10px;
      cursor: pointer;
      color: #9ca3af;
      display: flex;
      align-items: center;
    }

    .toggle-password:hover {
      color: #6b7280;
    }

    .error-text {
      font-size: 12px;
      color: #ef4444;
    }

    .password-hints {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 8px;
      padding: 12px 14px;
      background: #f9fafb;
      border-radius: 10px;
    }

    .hint {
      display: flex;
      align-items: center;
      gap: 6px;
      font-size: 12px;
      color: #9ca3af;
    }

    .hint mat-icon {
      font-size: 16px;
      width: 16px;
      height: 16px;
    }

    .hint.valid {
      color: #10b981;
    }

    .error-alert {
      display: flex;
      align-items: center;
      gap: 10px;
      padding: 12px 14px;
      background: #fef2f2;
      border: 1px solid #fecaca;
      border-radius: 10px;
      color: #dc2626;
      font-size: 13px;
    }

    .error-alert mat-icon {
      font-size: 18px;
      width: 18px;
      height: 18px;
    }

    .submit-btn {
      display: flex;
      align-items: center;
      justify-content: center;
      gap: 8px;
      width: 100%;
      padding: 14px 24px;
      background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
      color: white;
      border: none;
      border-radius: 10px;
      font-size: 15px;
      font-weight: 600;
      cursor: pointer;
      transition: all 0.2s ease;
      margin-top: 4px;
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
      font-size: 18px;
      width: 18px;
      height: 18px;
    }

    .terms-text {
      text-align: center;
      font-size: 12px;
      color: #9ca3af;
      margin: 0;
    }

    .terms-text a {
      color: #667eea;
      text-decoration: none;
    }

    .terms-text a:hover {
      text-decoration: underline;
    }

    .auth-footer {
      text-align: center;
      margin-top: 24px;
      padding-top: 20px;
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

    @media (max-width: 540px) {
      .auth-card {
        padding: 32px 24px;
        border-radius: 20px;
      }

      .form-row {
        grid-template-columns: 1fr;
      }

      .password-hints {
        grid-template-columns: 1fr;
      }

      .auth-title {
        font-size: 22px;
      }
    }
  `]
})
export class RegisterComponent {
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private router = inject(Router);

  hidePassword = signal(true);
  isLoading = signal(false);
  errorMessage = signal('');

  registerForm: FormGroup = this.fb.group({
    fullName: ['', [Validators.required]],
    tenantName: ['', [Validators.required]],
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(8)]]
  });

  hasUppercase(): boolean {
    return /[A-Z]/.test(this.registerForm.get('password')?.value || '');
  }

  hasNumber(): boolean {
    return /[0-9]/.test(this.registerForm.get('password')?.value || '');
  }

  hasSpecial(): boolean {
    return /[^a-zA-Z0-9]/.test(this.registerForm.get('password')?.value || '');
  }

  onSubmit(): void {
    if (this.registerForm.invalid) return;

    this.isLoading.set(true);
    this.errorMessage.set('');

    const formValue = this.registerForm.value;
    const request = {
      email: formValue.email,
      password: formValue.password,
      fullName: formValue.fullName,
      companyName: formValue.tenantName
    };
    this.authService.register(request).subscribe({
      next: () => {
        this.router.navigate(['/dashboard']);
      },
      error: (error) => {
        this.isLoading.set(false);
        this.errorMessage.set(error.error?.message || 'Registration failed. Please try again.');
      }
    });
  }
}
