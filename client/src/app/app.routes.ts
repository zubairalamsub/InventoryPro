import { Routes } from '@angular/router';
import { authGuard, guestGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  {
    path: '',
    redirectTo: 'dashboard',
    pathMatch: 'full'
  },
  {
    path: 'auth',
    canActivate: [guestGuard],
    children: [
      {
        path: 'login',
        loadComponent: () => import('./features/auth/components/login.component').then(m => m.LoginComponent)
      },
      {
        path: 'register',
        loadComponent: () => import('./features/auth/components/register.component').then(m => m.RegisterComponent)
      },
      {
        path: '',
        redirectTo: 'login',
        pathMatch: 'full'
      }
    ]
  },
  {
    path: '',
    canActivate: [authGuard],
    loadComponent: () => import('./layout/layout.component').then(m => m.LayoutComponent),
    children: [
      {
        path: 'dashboard',
        loadComponent: () => import('./features/dashboard/dashboard.component').then(m => m.DashboardComponent)
      },
      {
        path: 'products',
        loadComponent: () => import('./features/products/products.component').then(m => m.ProductsComponent)
      },
      {
        path: 'products/new',
        loadComponent: () => import('./features/products/product-form.component').then(m => m.ProductFormComponent)
      },
      {
        path: 'products/:id',
        loadComponent: () => import('./features/products/product-form.component').then(m => m.ProductFormComponent)
      },
      {
        path: 'inventory',
        loadComponent: () => import('./features/inventory/inventory.component').then(m => m.InventoryComponent)
      },
      {
        path: 'inventory/adjustment',
        loadComponent: () => import('./features/inventory/stock-adjustment.component').then(m => m.StockAdjustmentComponent)
      },
      {
        path: 'inventory/transfer',
        loadComponent: () => import('./features/inventory/stock-transfer.component').then(m => m.StockTransferComponent)
      },
      {
        path: 'sales',
        loadComponent: () => import('./features/sales/sales.component').then(m => m.SalesComponent)
      },
      {
        path: 'sales/new',
        loadComponent: () => import('./features/sales/sale-form.component').then(m => m.SaleFormComponent)
      },
      {
        path: 'customers',
        loadComponent: () => import('./features/customers/customers.component').then(m => m.CustomersComponent)
      },
      {
        path: 'customers/new',
        loadComponent: () => import('./features/customers/customer-form.component').then(m => m.CustomerFormComponent)
      },
      {
        path: 'customers/:id',
        loadComponent: () => import('./features/customers/customer-form.component').then(m => m.CustomerFormComponent)
      }
    ]
  },
  {
    path: '**',
    redirectTo: 'dashboard'
  }
];
