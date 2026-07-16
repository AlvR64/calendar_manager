import type {
  AdminAppointmentDetailsResponse,
  AppointmentDetailsResponse,
  AppointmentResponse,
  AppointmentSummaryResponse,
  CancelAppointmentRequest,
  CreateAppointmentRequest,
  UpdateAppointmentInternalNotesRequest,
  UpdateAppointmentStatusRequest,
} from '@/api/contracts';
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

export type CustomerAppointmentFilters = {
  from?: string;
  status?: string;
  to?: string;
};

export type AdminAppointmentFilters = {
  from: string;
  serviceId?: string;
  staffMemberId?: string;
  status?: string;
  to: string;
};

export function listAdminAppointments(token: string, filters: AdminAppointmentFilters) {
  const searchParams = new URLSearchParams();
  searchParams.set('from', filters.from);
  searchParams.set('to', filters.to);

  if (filters.staffMemberId) {
    searchParams.set('staffMemberId', filters.staffMemberId);
  }

  if (filters.serviceId) {
    searchParams.set('serviceId', filters.serviceId);
  }

  if (filters.status) {
    searchParams.set('status', filters.status);
  }

  return apiRequest<AppointmentSummaryResponse[]>(`/api/appointments?${searchParams.toString()}`, { token });
}

export function cancelAdminAppointment(appointmentId: string, request: CancelAppointmentRequest, token: string) {
  return apiRequest<AdminAppointmentDetailsResponse>(`/api/appointments/${appointmentId}/cancel`, {
    body: request,
    method: 'POST',
    token,
  });
}

export function updateAdminAppointmentStatus(appointmentId: string, request: UpdateAppointmentStatusRequest, token: string) {
  return apiRequest<AdminAppointmentDetailsResponse>(`/api/appointments/${appointmentId}/status`, {
    body: request,
    method: 'PUT',
    token,
  });
}

export function updateAdminAppointmentInternalNotes(appointmentId: string, request: UpdateAppointmentInternalNotesRequest, token: string) {
  return apiRequest<AdminAppointmentDetailsResponse>(`/api/appointments/${appointmentId}/internal-notes`, {
    body: request,
    method: 'PUT',
    token,
  });
}

export function listCustomerAppointments(token: string, filters: CustomerAppointmentFilters = {}) {
  const searchParams = new URLSearchParams();
  if (filters.from) {
    searchParams.set('from', filters.from);
  }

  if (filters.to) {
    searchParams.set('to', filters.to);
  }

  if (filters.status) {
    searchParams.set('status', filters.status);
  }

  const queryString = searchParams.toString();
  return apiRequest<AppointmentSummaryResponse[]>(`/api/customers/current/appointments${queryString ? `?${queryString}` : ''}`, { token });
}

export function cancelCustomerAppointment(appointmentId: string, request: CancelAppointmentRequest, token: string) {
  return apiRequest<AppointmentDetailsResponse>(`/api/customers/current/appointments/${appointmentId}/cancel`, {
    body: request,
    method: 'POST',
    token,
  });
}
