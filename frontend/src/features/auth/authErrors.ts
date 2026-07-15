import { ApiError } from '@/api/httpClient';

export function getAuthErrorMessage(error: unknown) {
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

  if (error.status === 401) {
    return 'Credenciales incorrectas.';
  }

  if (error.status === 403) {
    return 'Esta cuenta no tiene permiso para este acceso.';
  }

  if (error.status === 409) {
    return error.details.title ?? 'Ya existe una cuenta o business con esos datos.';
  }

  return error.details.title ?? 'No se pudo completar la operacion. Intentalo de nuevo.';
}
