import { useAuthStore } from '@/lib/auth-store';
import type { ProblemDetails } from '@/types/api';

// Empty by default: in development Vite proxies /api to the backend, so requests stay on one origin.
const baseUrl = (import.meta.env.VITE_API_URL ?? '').replace(/\/+$/, '');

export class ApiError extends Error {
  readonly status: number;
  readonly problem: ProblemDetails | null;
  /** True when the request carried a token. A 401 then means the session is no longer valid. */
  readonly hadSession: boolean;

  constructor(status: number, problem: ProblemDetails | null, fallbackMessage: string, hadSession = false) {
    super(messageFrom(problem) ?? fallbackMessage);
    this.name = 'ApiError';
    this.status = status;
    this.problem = problem;
    this.hadSession = hadSession;
  }
}

function messageFrom(problem: ProblemDetails | null): string | null {
  if (!problem) return null;
  if (problem.detail) return problem.detail;
  const firstValidation = problem.errors ? Object.values(problem.errors).flat()[0] : undefined;
  return firstValidation ?? problem.title ?? null;
}

async function readProblem(response: Response): Promise<ProblemDetails | null> {
  try {
    const parsed: unknown = JSON.parse(await response.text());
    return typeof parsed === 'object' && parsed !== null ? (parsed as ProblemDetails) : null;
  } catch {
    return null;
  }
}

type Method = 'GET' | 'POST' | 'PUT' | 'DELETE';
type QueryValue = string | number | boolean | null | undefined;

/** Appends a query string, skipping empty values. */
export function withQuery(path: string, params: Record<string, QueryValue>): string {
  const search = new URLSearchParams();
  for (const [key, value] of Object.entries(params)) {
    if (value !== undefined && value !== null && value !== '') search.set(key, String(value));
  }
  const query = search.toString();
  return query ? `${path}?${query}` : path;
}

async function request<T>(method: Method, path: string, body?: unknown, signal?: AbortSignal): Promise<T> {
  const { token, signOut } = useAuthStore.getState();

  const headers: Record<string, string> = { Accept: 'application/json' };
  if (body !== undefined) headers['Content-Type'] = 'application/json';
  if (token) headers.Authorization = `Bearer ${token}`;

  let response: Response;
  try {
    response = await fetch(`${baseUrl}${path}`, {
      method,
      headers,
      body: body === undefined ? undefined : JSON.stringify(body),
      signal,
    });
  } catch (cause) {
    if (cause instanceof DOMException && cause.name === 'AbortError') throw cause;
    throw new ApiError(0, null, 'Cannot reach the server. Check your connection and try again.', Boolean(token));
  }

  if (!response.ok) {
    // An expired or revoked token: forget it right away so no further request reuses it.
    if (response.status === 401 && token) signOut();
    throw new ApiError(response.status, await readProblem(response), `Request failed (${response.status}).`, Boolean(token));
  }

  const text = await response.text();
  return (text ? JSON.parse(text) : undefined) as T;
}

export const api = {
  get: <T>(path: string, signal?: AbortSignal) => request<T>('GET', path, undefined, signal),
  post: <T>(path: string, body?: unknown) => request<T>('POST', path, body ?? {}),
  put: <T>(path: string, body?: unknown) => request<T>('PUT', path, body ?? {}),
  delete: <T>(path: string) => request<T>('DELETE', path),
};
