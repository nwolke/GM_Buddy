import { AxiosError } from 'axios';

export class ApiError extends Error {
  readonly statusCode: number;
  readonly correlationId?: string;
  readonly details?: string;

  constructor(statusCode: number, message: string, correlationId?: string, details?: string) {
    super(message);
    this.name = 'ApiError';
    this.statusCode = statusCode;
    this.correlationId = correlationId;
    this.details = details;
  }

  get userMessage(): string {
    switch (this.statusCode) {
      case 401:
        return 'Session expired. Please log in again.';
      case 403:
        return 'You do not have permission to perform this action.';
      case 400:
        return this.details || 'Invalid request. Please check your input.';
      case 404:
        return this.details || 'The requested resource was not found.';
      case 409:
        return this.details || 'This conflicts with an existing resource.';
      default:
        return 'Something went wrong. Please try again later.';
    }
  }
}

export function extractApiError(error: unknown): ApiError {
  if (error instanceof ApiError) {
    return error;
  }

  if (error instanceof AxiosError) {
    const status = error.response?.status ?? 0;
    const data = error.response?.data;
    const correlationId = error.response?.headers?.['x-correlation-id'];

    // No response at all — network error
    if (!error.response) {
      return new ApiError(0, 'Unable to reach the server. Please check your connection.');
    }

    // ProblemDetails format from our middleware
    if (data && typeof data === 'object' && 'title' in data) {
      return new ApiError(
        status,
        data.title as string,
        correlationId ?? data.extensions?.correlationId,
        data.detail as string | undefined,
      );
    }

    // Plain string error from legacy responses
    if (typeof data === 'string') {
      return new ApiError(status, data, correlationId, data);
    }

    return new ApiError(status, error.message, correlationId);
  }

  if (error instanceof Error) {
    return new ApiError(0, error.message);
  }

  return new ApiError(0, 'An unknown error occurred.');
}
