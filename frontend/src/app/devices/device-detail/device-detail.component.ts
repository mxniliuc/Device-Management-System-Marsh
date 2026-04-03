import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { map, of, switchMap } from 'rxjs';
import { Device, deviceTypeLabel } from '../../core/models/device.model';
import { User } from '../../core/models/user.model';
import { AuthService } from '../../core/services/auth.service';
import { DeviceService } from '../../core/services/device.service';
import { UserService } from '../../core/services/user.service';

@Component({
  selector: 'app-device-detail',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './device-detail.component.html',
  styleUrl: './device-detail.component.scss',
})
export class DeviceDetailComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly devicesApi = inject(DeviceService);
  private readonly usersApi = inject(UserService);
  private readonly auth = inject(AuthService);

  readonly loading = signal(true);
  readonly error = signal<string | null>(null);
  readonly device = signal<Device | null>(null);
  readonly assignedUserName = signal<string | null>(null);
  readonly assignBusy = signal(false);
  readonly assignmentError = signal<string | null>(null);

  readonly typeLabel = deviceTypeLabel;

  private deviceId: string | null = null;

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) {
      void this.router.navigate(['/devices']);
      return;
    }
    this.deviceId = id;
    this.loadDevice(id);
  }

  private loadDevice(id: string, withSpinner = true): void {
    if (withSpinner) {
      this.loading.set(true);
      this.error.set(null);
    }
    this.assignmentError.set(null);
    this.devicesApi
      .getById(id)
      .pipe(
        switchMap((device: Device) => {
          if (!device.assignedToUserId) {
            return of({ device, user: null as User | null });
          }
          return this.usersApi.getAll().pipe(
            map((users: User[]) => {
              const u =
                users.find((x) => x.id === device.assignedToUserId) ?? null;
              return { device, user: u };
            }),
          );
        }),
      )
      .subscribe({
        next: ({ device, user }: { device: Device; user: User | null }) => {
          this.device.set(device);
          this.assignedUserName.set(user?.name ?? null);
          this.loading.set(false);
          this.assignBusy.set(false);
        },
        error: () => {
          this.error.set('Device not found or could not be loaded.');
          this.loading.set(false);
          this.assignBusy.set(false);
        },
      });
  }

  /** True when the signed-in user holds this device. */
  isAssignedToMe(): boolean {
    const d = this.device();
    const uid = this.auth.userId();
    return !!(d && uid && d.assignedToUserId === uid);
  }

  /** True when another user holds the device. */
  isAssignedToSomeoneElse(): boolean {
    const d = this.device();
    const uid = this.auth.userId();
    return !!(d?.assignedToUserId && uid && d.assignedToUserId !== uid);
  }

  assignToMe(): void {
    const d = this.device();
    const id = this.deviceId;
    if (!d || !id) return;
    this.assignBusy.set(true);
    this.assignmentError.set(null);
    this.devicesApi.assignToSelf(id).subscribe({
      next: () => this.loadDevice(id, false),
      error: (err: HttpErrorResponse) => {
        this.assignBusy.set(false);
        if (err.status === 409) {
          const detail =
            typeof err.error === 'object' && err.error && 'detail' in err.error
              ? String((err.error as { detail?: string }).detail)
              : 'This device is already assigned to someone else.';
          this.assignmentError.set(detail);
          return;
        }
        this.assignmentError.set('Could not assign this device. Try again.');
      },
    });
  }

  unassign(): void {
    const d = this.device();
    const id = this.deviceId;
    if (!d || !id) return;
    this.assignBusy.set(true);
    this.assignmentError.set(null);
    this.devicesApi.unassignSelf(id).subscribe({
      next: () => this.loadDevice(id, false),
      error: () => {
        this.assignBusy.set(false);
        this.assignmentError.set('Could not unassign. Try again.');
      },
    });
  }

  delete(): void {
    const d = this.device();
    if (!d) return;
    const ok = window.confirm(
      `Remove “${d.name}” from the inventory?\n\nThis cannot be undone.`,
    );
    if (!ok) return;
    this.devicesApi.delete(d.id).subscribe({
      next: () => void this.router.navigate(['/devices']),
      error: () => window.alert('Could not delete this device. Please try again.'),
    });
  }
}
