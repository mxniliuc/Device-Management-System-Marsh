import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import {
  FormBuilder,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { safeReturnUrl } from '../../core/util/return-url';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss',
})
export class LoginComponent {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  readonly submitting = signal(false);
  readonly formError = signal<string | null>(null);

  readonly form = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required]],
  });

  /** Keeps deep-link target when opening Register from the login page. */
  registerQueryParams(): Record<string, string> {
    const raw = this.route.snapshot.queryParamMap.get('returnUrl');
    const safe = safeReturnUrl(raw);
    return safe ? { returnUrl: safe } : {};
  }

  submit(): void {
    this.formError.set(null);
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const v = this.form.getRawValue();
    this.submitting.set(true);
    this.auth
      .login({
        email: v.email.trim(),
        password: v.password,
      })
      .subscribe({
        next: () => {
          const ret =
            safeReturnUrl(
              this.route.snapshot.queryParamMap.get('returnUrl') ?? undefined,
            ) ?? '/devices';
          void this.router.navigateByUrl(ret);
        },
        error: (err: HttpErrorResponse) => {
          this.submitting.set(false);
          if (err.status === 401) {
            this.formError.set('Invalid email or password.');
            return;
          }
          const detail =
            typeof err.error === 'object' && err.error && 'detail' in err.error
              ? String((err.error as { detail?: string }).detail)
              : err.message;
          this.formError.set(detail ?? 'Sign-in failed.');
        },
      });
  }
}
