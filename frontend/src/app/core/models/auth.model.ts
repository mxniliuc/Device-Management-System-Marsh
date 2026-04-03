export interface LoginResponse {
  token: string;
  expiresAtUtc: string;
  userId: string;
  email: string;
}
