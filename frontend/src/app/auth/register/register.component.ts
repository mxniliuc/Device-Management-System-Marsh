import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { safeReturnUrl } from '../../core/util/return-url';
import { passwordsMatch } from './register-passwords.validator';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './register.component.html',
  styleUrl: './register.component.scss',
})
export class RegisterComponent {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  readonly submitting = signal(false);
  readonly formError = signal<string | null>(null);

  readonly form = this.fb.nonNullable.group(
    {
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(8)]],
      confirmPassword: ['', [Validators.required, Validators.minLength(8)]],
      name: ['', [Validators.required, Validators.maxLength(200)]],
      role: ['', [Validators.required, Validators.maxLength(100)]],
      location: ['', [Validators.required, Validators.maxLength(500)]],
    },
    { validators: passwordsMatch },
  );

  loginQueryParams(): Record<string, string> {
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
      .register({
        email: v.email.trim(),
        password: v.password,
        confirmPassword: v.confirmPassword,
        name: v.name.trim(),
        role: v.role.trim(),
        location: v.location.trim(),
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
          const detail =
            typeof err.error === 'object' && err.error && 'detail' in err.error
              ? String((err.error as { detail?: string }).detail)
              : err.message;
          this.formError.set(detail ?? 'Registration failed.');
        },
      });
  }
}
