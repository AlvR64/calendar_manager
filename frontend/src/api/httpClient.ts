import { env } from '@/config/env';

export type ApiErrorDetails = {
  detail?: string;
  status?: number;
  title?: string;
  type?: string;
};

export class ApiError extends Error {
  public readonly details: ApiErrorDetails;
  public readonly status: number;

  public constructor(message: string, status: number, details: ApiErrorDetails = {}) {
    super(message);
    this.name = 'ApiError';
    this.status = status;
    this.details = details;
  }
}

type RequestOptions = Omit<RequestInit, 'body'> & {
  body?: unknown;
  token?: string | null;
};

export async function apiRequest<TResponse>(path: string, options: RequestOptions = {}): Promise<TResponse> {
  const headers = new Headers(options.headers);
  headers.set('Accept', 'application/json');

  if (options.body !== undefined) {
    headers.set('Content-Type', 'application/json');
  }

  if (options.token) {
    headers.set('Authorization', `Bearer ${options.token}`);
  }

  const response = await fetch(`${env.apiBaseUrl}${path}`, {
    ...options,
    body: options.body === undefined ? undefined : JSON.stringify(options.body),
    headers,
  });

  if (!response.ok) {
    const details = await readJson<ApiErrorDetails>(response);
    throw new ApiError(details.title ?? 'API request failed', response.status, details);
  }

  if (response.status === 204) {
    return undefined as TResponse;
  }

  return readJson<TResponse>(response);
}

async function readJson<T>(response: Response): Promise<T> {
  const text = await response.text();
  return text ? (JSON.parse(text) as T) : ({} as T);
}
