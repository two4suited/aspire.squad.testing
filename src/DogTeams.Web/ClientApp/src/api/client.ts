/**
 * Base API client.
 *
 * Base URL is resolved from Aspire-injected env variables first,
 * then falls back to VITE_API_URL, then to a local default.
 *
 * Auth token is read from localStorage and attached as a Bearer header.
 * 401 responses clear the stored session and redirect to /login.
 */

const TOKEN_KEY = 'dogteams_token';

function getBaseUrl(): string {
  // Aspire injects service URLs as environment variables at build time via a proxy
  // The Vite dev server proxies /api → the backend, so we just use relative paths in dev
  return import.meta.env.VITE_API_URL ?? '';
}

function getToken(): string | null {
  return localStorage.getItem(TOKEN_KEY);
}

function handleUnauthorized(): never {
  localStorage.removeItem(TOKEN_KEY);
  localStorage.removeItem('dogteams_user');
  window.location.href = '/login';
  throw new Error('Unauthorized');
}

export async function apiFetch<T>(path: string, init: RequestInit = {}): Promise<T> {
  const token = getToken();

  const headers: HeadersInit = {
    'Content-Type': 'application/json',
    ...(token ? { Authorization: `Bearer ${token}` } : {}),
    ...(init.headers ?? {}),
  };

  try {
    const response = await fetch(`${getBaseUrl()}${path}`, { ...init, headers });

    // Only redirect to login for 401 on authenticated endpoints (those with a token)
    // For login/register endpoints, let the error propagate normally
    if (response.status === 401 && token) {
      handleUnauthorized();
    }

    if (!response.ok) {
      const text = await response.text().catch(() => response.statusText);
      // Ensure error messages always contain descriptive text
      const errorMsg = text || `HTTP ${response.status}`;
      throw new Error(errorMsg);
    }

    // 204 No Content
    if (response.status === 204) return undefined as unknown as T;

    return response.json() as Promise<T>;
  } catch (err) {
    console.error('[apiFetch] Error fetching', path, ':', err);
    throw err;
  }
}

export function setToken(token: string): void {
  localStorage.setItem(TOKEN_KEY, token);
}
