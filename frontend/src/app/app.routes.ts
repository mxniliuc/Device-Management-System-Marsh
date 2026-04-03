import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  { path: '', redirectTo: 'devices', pathMatch: 'full' },
  {
    path: 'register',
    loadComponent: () =>
      import('./auth/register/register.component').then((m) => m.RegisterComponent),
  },
  {
    path: 'login',
    loadComponent: () =>
      import('./auth/login/login.component').then((m) => m.LoginComponent),
  },
  {
    path: 'devices',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./devices/device-list/device-list.component').then(
        (m) => m.DeviceListComponent,
      ),
  },
  {
    path: 'devices/new',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./devices/device-form/device-form.component').then(
        (m) => m.DeviceFormComponent,
      ),
  },
  {
    path: 'devices/:id/edit',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./devices/device-form/device-form.component').then(
        (m) => m.DeviceFormComponent,
      ),
  },
  {
    path: 'devices/:id',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./devices/device-detail/device-detail.component').then(
        (m) => m.DeviceDetailComponent,
      ),
  },
];
