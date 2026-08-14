export interface UserProfile {
  id: string;
  fullName: string;
  email: string;
  roles: string[];
  createdAt: string;
}

export interface AuthResponse {
  token: string;
  fullName: string;
  email: string;
  roles: string[];
  expiresAt: string;
}
