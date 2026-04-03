import { HttpClient } from '@angular/common/http';
import { inject, Injectable, signal } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { LoginResponse } from '../models/auth.model';

/** localStorage key — shared with auth interceptor (no DI cycle). */
export const AUTH_TOKEN_STORAGE_KEY = 'device_management_jwt';

export const AUTH_USER_ID_STORAGE_KEY = 'device_management_user_id';

function parseJwtSub(token: string): string | null {
  try {
    const payload = token.split('.')[1];
    if (!payload) {
      return null;
    }
    const base64 = payload.replace(/-/g, '+').replace(/_/g, '/');
    const pad = base64.length % 4;
    const padded = pad ? base64 + '='.repeat(4 - pad) : base64;
    const json = JSON.parse(atob(padded)) as { sub?: string };
    return typeof json.sub === 'string' ? json.sub : null;
  } catch {
    return null;
  }
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);

  /** Current JWT; persisted across reloads. */
  readonly token = signal<string | null>(this.readStoredToken());

  /** Profile id from login/register (or JWT `sub` if missing). */
  readonly userId = signal<string | null>(this.readInitialUserId());

  register(body: {
    email: string;
    password: string;
    confirmPassword: string;
    name: string;
    role: string;
    location: string;
  }): Observable<LoginResponse> {
    return this.http
      .post<LoginResponse>('/api/auth/register', body)
      .pipe(tap((r) => this.persistSession(r.token, r.userId)));
  }

  login(body: { email: string; password: string }): Observable<LoginResponse> {
    return this.http
      .post<LoginResponse>('/api/auth/login', body)
      .pipe(tap((r) => this.persistSession(r.token, r.userId)));
  }

  logout(): void {
    localStorage.removeItem(AUTH_TOKEN_STORAGE_KEY);
    localStorage.removeItem(AUTH_USER_ID_STORAGE_KEY);
    this.token.set(null);
    this.userId.set(null);
  }

  private readStoredToken(): string | null {
    try {
      return localStorage.getItem(AUTH_TOKEN_STORAGE_KEY);
    } catch {
      return null;
    }
  }

  private readInitialUserId(): string | null {
    try {
      const stored = localStorage.getItem(AUTH_USER_ID_STORAGE_KEY);
      if (stored) {
        return stored;
      }
      const token = localStorage.getItem(AUTH_TOKEN_STORAGE_KEY);
      return token ? parseJwtSub(token) : null;
    } catch {
      return null;
    }
  }

  private persistSession(token: string, userId: string): void {
    localStorage.setItem(AUTH_TOKEN_STORAGE_KEY, token);
    localStorage.setItem(AUTH_USER_ID_STORAGE_KEY, userId);
    this.token.set(token);
    this.userId.set(userId);
  }
}
