import { ApiError } from '@/api/httpClient';

export function getApiErrorMessage(error: unknown) {
  if (!(error instanceof ApiError)) {
    return 'No se pudo completar la operacion. Intentalo de nuevo.';
  }

  if (error.details.detail) {
    return error.details.detail;
  }

  if (error.details.errors) {
    const [firstError] = Object.values(error.details.errors).flat();
    if (firstError) {
      return firstError;
    }
  }

  return error.details.title ?? 'No se pudo completar la operacion. Intentalo de nuevo.';
}
