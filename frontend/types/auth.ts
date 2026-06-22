export interface LoginRequest {
  username: string;
  password: string;
}

export interface LoginResponse {
  accessToken: string;
  refreshToken: string;
  accessTokenExpiry: string;
  userId: string;
  username: string;
  fullName: string;
  permissions: string[];
  mustChangePassword: boolean;
}
