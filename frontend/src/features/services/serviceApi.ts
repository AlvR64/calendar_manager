import type { CreateServiceRequest, ServiceResponse, UpdateServiceActiveStateRequest, UpdateServiceRequest } from '@/api/contracts';
import { apiRequest } from '@/api/httpClient';

export function listServices(token: string) {
  return apiRequest<ServiceResponse[]>('/api/services', { token });
}

export function getService(serviceId: string, token: string) {
  return apiRequest<ServiceResponse>(`/api/services/${serviceId}`, { token });
}

export function createService(request: CreateServiceRequest, token: string) {
  return apiRequest<ServiceResponse>('/api/services', {
    body: request,
    method: 'POST',
    token,
  });
}

export function updateService(serviceId: string, request: UpdateServiceRequest, token: string) {
  return apiRequest<ServiceResponse>(`/api/services/${serviceId}`, {
    body: request,
    method: 'PUT',
    token,
  });
}

export function updateServiceActiveState(serviceId: string, request: UpdateServiceActiveStateRequest, token: string) {
  return apiRequest<ServiceResponse>(`/api/services/${serviceId}/active-state`, {
    body: request,
    method: 'PUT',
    token,
  });
}

export function deleteService(serviceId: string, token: string) {
  return apiRequest<void>(`/api/services/${serviceId}`, {
    method: 'DELETE',
    token,
  });
}
