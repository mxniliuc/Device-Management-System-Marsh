import { HttpInterceptorFn } from '@angular/common/http';
import { AUTH_TOKEN_STORAGE_KEY } from '../services/auth.service';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  if (!req.url.startsWith('/api/')) {
    return next(req);
  }
  if (
    req.url.includes('/api/auth/register') ||
    req.url.includes('/api/auth/login')
  ) {
    return next(req);
  }
  let token: string | null = null;
  try {
    token = localStorage.getItem(AUTH_TOKEN_STORAGE_KEY);
  } catch {
    /* ignore */
  }
  if (token) {
    req = req.clone({ setHeaders: { Authorization: `Bearer ${token}` } });
  }
  return next(req);
};
