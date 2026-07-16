import type { AppointmentResponse, CreateAppointmentRequest } from '@/api/contracts';
import { apiRequest } from '@/api/httpClient';

export function createAppointment(request: CreateAppointmentRequest, token: string) {
  return apiRequest<AppointmentResponse>('/api/appointments', {
    body: request,
    method: 'POST',
    token,
  });
}
