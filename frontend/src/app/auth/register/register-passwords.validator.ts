import { AbstractControl, ValidationErrors } from '@angular/forms';

export function passwordsMatch(group: AbstractControl): ValidationErrors | null {
  const p = group.get('password')?.value;
  const c = group.get('confirmPassword')?.value;
  if (p !== c) {
    return { passwordMismatch: true };
  }
  return null;
}
