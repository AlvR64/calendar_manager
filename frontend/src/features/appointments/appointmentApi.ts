import type { AppointmentDetailsResponse, AppointmentResponse, CreateAppointmentRequest } from '@/api/contracts';
import { apiRequest } from '@/api/httpClient';

export function createAppointment(request: CreateAppointmentRequest, token: string) {
  return apiRequest<AppointmentResponse>('/api/appointments', {
    body: request,
    method: 'POST',
    token,
  });
}

export function getAppointmentDetails(appointmentId: string, token: string) {
  return apiRequest<AppointmentDetailsResponse>(`/api/appointments/${appointmentId}`, { token });
}
