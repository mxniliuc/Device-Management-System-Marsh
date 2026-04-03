import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { Device, DeviceWritePayload } from '../models/device.model';

@Injectable({ providedIn: 'root' })
export class DeviceService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = '/api/devices';

  getAll(): Observable<Device[]> {
    return this.http.get<Device[]>(this.baseUrl);
  }

  getById(id: string): Observable<Device> {
    return this.http.get<Device>(`${this.baseUrl}/${id}`);
  }

  create(body: DeviceWritePayload): Observable<Device> {
    return this.http.post<Device>(this.baseUrl, body);
  }

  update(id: string, body: DeviceWritePayload): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${id}`, body);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
