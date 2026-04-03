import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import {
  Device,
  DeviceWritePayload,
  GenerateDeviceDescriptionPayload,
} from '../models/device.model';

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

  /** LLM-generated one-line description (server uses Ollama at LlmDescription:BaseUrl by default). */
  generateDescription(
    body: GenerateDeviceDescriptionPayload,
  ): Observable<{ description: string }> {
    return this.http.post<{ description: string }>(
      `${this.baseUrl}/generate-description`,
      body,
    );
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

  /** Assigns the device to the signed-in user (API: unassigned or already yours). */
  assignToSelf(id: string): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/${id}/assign`, {});
  }

  /** Clears assignment when the device is assigned to you. */
  unassignSelf(id: string): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/${id}/unassign`, {});
  }
}
