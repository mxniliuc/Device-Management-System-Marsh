/** Allows only same-origin relative paths (no protocol-relative URLs). */
export function safeReturnUrl(raw: string | null | undefined): string | null {
  if (!raw || typeof raw !== 'string') {
    return null;
  }
  const t = raw.trim();
  if (!t.startsWith('/') || t.startsWith('//')) {
    return null;
  }
  return t;
}
