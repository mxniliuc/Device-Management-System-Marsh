import { CommonModule } from '@angular/common';
import { Component, inject, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { forkJoin } from 'rxjs';
import { Device, deviceTypeLabel } from '../../core/models/device.model';
import { User } from '../../core/models/user.model';
import { DeviceService } from '../../core/services/device.service';
import { UserService } from '../../core/services/user.service';

@Component({
  selector: 'app-device-list',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './device-list.component.html',
  styleUrl: './device-list.component.scss',
})
export class DeviceListComponent implements OnInit {
  private readonly devicesApi = inject(DeviceService);
  private readonly usersApi = inject(UserService);

  readonly loading = signal(true);
  readonly error = signal<string | null>(null);
  readonly devices = signal<Device[]>([]);
  private userNameById = new Map<string, string>();

  readonly typeLabel = deviceTypeLabel;

  ngOnInit(): void {
    this.load();
  }

  userLabel(userId: string | null): string {
    if (!userId) return '— Unassigned';
    return this.userNameById.get(userId) ?? 'Unknown user';
  }

  private load(): void {
    this.loading.set(true);
    this.error.set(null);
    forkJoin({
      devices: this.devicesApi.getAll(),
      users: this.usersApi.getAll(),
    }).subscribe({
      next: ({ devices, users }) => {
        this.userNameById = new Map(users.map((u: User) => [u.id, u.name]));
        this.devices.set(
          [...devices].sort((a, b) => a.name.localeCompare(b.name, undefined, { sensitivity: 'base' })),
        );
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Could not load devices. Is the API running on http://localhost:5084?');
        this.loading.set(false);
      },
    });
  }

  deleteDevice(device: Device, ev: Event): void {
    ev.preventDefault();
    ev.stopPropagation();
    const ok = window.confirm(
      `Remove “${device.name}” from the inventory?\n\nThis cannot be undone.`,
    );
    if (!ok) return;
    this.devicesApi.delete(device.id).subscribe({
      next: () => this.load(),
      error: () => window.alert('Could not delete this device. Please try again.'),
    });
  }
}
