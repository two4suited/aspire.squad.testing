import { apiFetch, setToken } from './client';
import { AuthUser } from '../types';

interface LoginRequest {
  email: string;
  password: string;
}

interface RegisterRequest {
  name: string;
  email: string;
  password: string;
}

interface AuthTokenResponse {
  accessToken: string;
  refreshToken: string;
  expiresIn: number;
  tokenType: string;
}

interface AuthUserResponse {
  userId: string;
  email: string;
  name: string;
  teamId?: string;
  ownerId?: string;
  role: string;
}

export async function login(req: LoginRequest): Promise<AuthUser> {
  const tokenResponse = await apiFetch<AuthTokenResponse>('/api/auth/login', {
    method: 'POST',
    body: JSON.stringify(req),
  });

  // Store the token so it's available for the next request
  setToken(tokenResponse.accessToken);

  // Fetch user profile with the token now set
  const userResponse = await apiFetch<AuthUserResponse>('/api/auth/me');

  return {
    id: userResponse.userId,
    email: userResponse.email,
    name: userResponse.name,
    token: tokenResponse.accessToken,
    teamId: userResponse.teamId,
  };
}

export async function register(req: RegisterRequest): Promise<AuthUser> {
  const tokenResponse = await apiFetch<AuthTokenResponse>('/api/auth/register', {
    method: 'POST',
    body: JSON.stringify(req),
  });

  // Store the token so it's available for the next request
  setToken(tokenResponse.accessToken);

  // Fetch user profile with the token now set
  const userResponse = await apiFetch<AuthUserResponse>('/api/auth/me');

  return {
    id: userResponse.userId,
    email: userResponse.email,
    name: userResponse.name,
    token: tokenResponse.accessToken,
    teamId: userResponse.teamId,
  };
}

export function logout(): Promise<void> {
  return apiFetch<void>('/api/auth/logout', { method: 'POST' });
}
