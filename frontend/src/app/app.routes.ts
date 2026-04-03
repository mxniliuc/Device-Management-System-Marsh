import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', redirectTo: 'devices', pathMatch: 'full' },
  {
    path: 'devices',
    loadComponent: () =>
      import('./devices/device-list/device-list.component').then(
        (m) => m.DeviceListComponent,
      ),
  },
  {
    path: 'devices/new',
    loadComponent: () =>
      import('./devices/device-form/device-form.component').then(
        (m) => m.DeviceFormComponent,
      ),
  },
  {
    path: 'devices/:id/edit',
    loadComponent: () =>
      import('./devices/device-form/device-form.component').then(
        (m) => m.DeviceFormComponent,
      ),
  },
  {
    path: 'devices/:id',
    loadComponent: () =>
      import('./devices/device-detail/device-detail.component').then(
        (m) => m.DeviceDetailComponent,
      ),
  },
];
