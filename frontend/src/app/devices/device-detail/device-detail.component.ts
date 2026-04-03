import { CommonModule } from '@angular/common';
import { Component, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { map, of, switchMap } from 'rxjs';
import { Device, deviceTypeLabel } from '../../core/models/device.model';
import { User } from '../../core/models/user.model';
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

  readonly loading = signal(true);
  readonly error = signal<string | null>(null);
  readonly device = signal<Device | null>(null);
  readonly assignedUserName = signal<string | null>(null);

  readonly typeLabel = deviceTypeLabel;

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) {
      void this.router.navigate(['/devices']);
      return;
    }
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
        },
        error: () => {
          this.error.set('Device not found or could not be loaded.');
          this.loading.set(false);
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
