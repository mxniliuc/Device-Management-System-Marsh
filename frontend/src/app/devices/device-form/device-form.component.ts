import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, OnInit, signal } from '@angular/core';
import {
  FormBuilder,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { forkJoin } from 'rxjs';
import {
  Device,
  DeviceType,
  DeviceWritePayload,
} from '../../core/models/device.model';
import { User } from '../../core/models/user.model';
import { DeviceService } from '../../core/services/device.service';
import { UserService } from '../../core/services/user.service';

@Component({
  selector: 'app-device-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './device-form.component.html',
  styleUrl: './device-form.component.scss',
})
export class DeviceFormComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly devicesApi = inject(DeviceService);
  private readonly usersApi = inject(UserService);

  readonly loading = signal(true);
  readonly saving = signal(false);
  readonly loadError = signal<string | null>(null);
  readonly formError = signal<string | null>(null);

  mode: 'create' | 'edit' = 'create';
  private deviceId: string | null = null;
  private existing: Device[] = [];

  readonly users = signal<User[]>([]);

  readonly form = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(200)]],
    manufacturer: ['', [Validators.required, Validators.maxLength(200)]],
    type: ['Phone' as DeviceType, [Validators.required]],
    os: ['', [Validators.required, Validators.maxLength(100)]],
    osVersion: ['', [Validators.required, Validators.maxLength(100)]],
    processor: ['', [Validators.required, Validators.maxLength(120)]],
    ramGb: [
      8,
      [Validators.required, Validators.min(1), Validators.max(4096)],
    ],
    description: ['', [Validators.required, Validators.maxLength(2000)]],
    location: ['', [Validators.required, Validators.maxLength(500)]],
    assignedToUserId: [''],
  });

  readonly typeOptions: DeviceType[] = ['Phone', 'Tablet'];

  ngOnInit(): void {
    const url = this.router.url;
    if (url.endsWith('/new')) {
      this.mode = 'create';
    } else if (url.includes('/edit')) {
      this.mode = 'edit';
      this.deviceId = this.route.snapshot.paramMap.get('id');
      if (!this.deviceId) {
        void this.router.navigate(['/devices']);
        return;
      }
    } else {
      void this.router.navigate(['/devices']);
      return;
    }

    if (this.mode === 'create') {
      forkJoin({
        devices: this.devicesApi.getAll(),
        users: this.usersApi.getAll(),
      }).subscribe({
        next: ({ devices, users }) => {
          this.existing = devices;
          this.users.set([...users].sort((a, b) => a.name.localeCompare(b.name)));
          this.loading.set(false);
        },
        error: () => {
          this.loadError.set('Could not load data. Check that the API is running.');
          this.loading.set(false);
        },
      });
    } else {
      const id = this.deviceId!;
      forkJoin({
        device: this.devicesApi.getById(id),
        devices: this.devicesApi.getAll(),
        users: this.usersApi.getAll(),
      }).subscribe({
        next: ({ device, devices, users }) => {
          this.existing = devices;
          this.users.set([...users].sort((a, b) => a.name.localeCompare(b.name)));
          this.form.patchValue({
            name: device.name,
            manufacturer: device.manufacturer,
            type: device.type,
            os: device.os,
            osVersion: device.osVersion,
            processor: device.processor,
            ramGb: device.ramGb,
            description: device.description,
            location: device.location,
            assignedToUserId: device.assignedToUserId ?? '',
          });
          this.loading.set(false);
        },
        error: () => {
          this.loadError.set('Could not load this device for editing.');
          this.loading.set(false);
        },
      });
    }
  }

  submit(): void {
    this.formError.set(null);
    this.form.markAllAsTouched();
    if (this.form.invalid) {
      this.formError.set('Please fill in every field correctly.');
      return;
    }

    const raw = this.form.getRawValue();
    const payload: DeviceWritePayload = {
      name: raw.name.trim(),
      manufacturer: raw.manufacturer.trim(),
      type: raw.type,
      os: raw.os.trim(),
      osVersion: raw.osVersion.trim(),
      processor: raw.processor.trim(),
      ramGb: Number(raw.ramGb),
      description: raw.description.trim(),
      location: raw.location.trim(),
      assignedToUserId: raw.assignedToUserId ? raw.assignedToUserId : null,
    };

    const dup = this.existing.find(
      (d) =>
        d.id !== this.deviceId &&
        d.name.trim().toLowerCase() === payload.name.toLowerCase() &&
        d.manufacturer.trim().toLowerCase() ===
          payload.manufacturer.toLowerCase(),
    );
    if (dup) {
      this.formError.set(
        'A device with this name and manufacturer already exists. Choose a different name or manufacturer.',
      );
      return;
    }

    this.saving.set(true);

    if (this.mode === 'create') {
      this.devicesApi.create(payload).subscribe({
        next: (created: Device) => {
          this.saving.set(false);
          void this.router.navigate(['/devices', created.id]);
        },
        error: (err: unknown) => this.onHttpError(err),
      });
    } else {
      this.devicesApi.update(this.deviceId!, payload).subscribe({
        next: () => {
          this.saving.set(false);
          void this.router.navigate(['/devices', this.deviceId!]);
        },
        error: (err: unknown) => this.onHttpError(err),
      });
    }
  }

  private onHttpError(err: unknown): void {
    this.saving.set(false);
    const msg = parseApiError(err);
    this.formError.set(msg);
  }

  fieldError(field: string): string | null {
    const c = this.form.get(field);
    if (!c || !c.touched || !c.errors) return null;
    if (c.errors['required']) return 'Required';
    if (c.errors['maxlength'])
      return `Max ${c.errors['maxlength'].requiredLength} characters`;
    if (c.errors['min']) return `Minimum ${c.errors['min'].min}`;
    if (c.errors['max']) return `Maximum ${c.errors['max'].max}`;
    return null;
  }
}

function parseApiError(err: unknown): string {
  if (err instanceof HttpErrorResponse) {
    const body = err.error as { detail?: string; title?: string } | string | null;
    if (body && typeof body === 'object' && typeof body.detail === 'string') {
      return body.detail;
    }
    if (typeof body === 'string' && body.length > 0) return body;
    return err.message || 'Request failed';
  }
  return 'Something went wrong';
}
