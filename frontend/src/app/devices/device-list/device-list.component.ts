import { CommonModule } from '@angular/common';
import { Component, DestroyRef, inject, OnInit, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { RouterLink } from '@angular/router';
import { debounceTime, distinctUntilChanged, forkJoin, Subject } from 'rxjs';
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
  private readonly destroyRef = inject(DestroyRef);
  private readonly searchTerms = new Subject<string>();

  readonly loading = signal(true);
  readonly error = signal<string | null>(null);
  readonly devices = signal<Device[]>([]);
  readonly searchQuery = signal('');
  private userNameById = new Map<string, string>();

  readonly typeLabel = deviceTypeLabel;

  ngOnInit(): void {
    this.searchTerms
      .pipe(debounceTime(350), distinctUntilChanged(), takeUntilDestroyed(this.destroyRef))
      .subscribe((q) => this.loadWithSearchQuery(q));

    this.loadWithSearchQuery('');
  }

  userLabel(userId: string | null): string {
    if (!userId) return '— Unassigned';
    return this.userNameById.get(userId) ?? 'Unknown user';
  }

  onSearchInput(ev: Event): void {
    const el = ev.target as HTMLInputElement;
    this.searchQuery.set(el.value);
    this.searchTerms.next(el.value.trim());
  }

  private loadWithSearchQuery(q: string): void {
    this.loading.set(true);
    this.error.set(null);
    const devices$ = q.length > 0 ? this.devicesApi.search(q) : this.devicesApi.getAll();
    forkJoin({
      devices: devices$,
      users: this.usersApi.getAll(),
    }).subscribe({
      next: ({ devices, users }) => {
        this.userNameById = new Map(users.map((u: User) => [u.id, u.name]));
        const sorted =
          q.length > 0
            ? devices
            : [...devices].sort((a, b) =>
                a.name.localeCompare(b.name, undefined, { sensitivity: 'base' }),
              );
        this.devices.set(sorted);
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
      next: () => this.loadWithSearchQuery(this.searchQuery().trim()),
      error: () => window.alert('Could not delete this device. Please try again.'),
    });
  }
}
