import type {
  CreateStaffMemberAvailabilityExceptionRequest,
  CreateStaffMemberAvailabilityRequest,
  StaffMemberAvailabilityExceptionResponse,
  StaffMemberAvailabilityResponse,
  UpdateStaffMemberAvailabilityExceptionRequest,
  UpdateStaffMemberAvailabilityRequest,
} from '@/api/contracts';
import { apiRequest } from '@/api/httpClient';

export function listStaffMemberAvailabilities(staffMemberId: string, token: string) {
  return apiRequest<StaffMemberAvailabilityResponse[]>(`/api/staff-members/${staffMemberId}/availability`, { token });
}

export function createStaffMemberAvailability(staffMemberId: string, request: CreateStaffMemberAvailabilityRequest, token: string) {
  return apiRequest<StaffMemberAvailabilityResponse>(`/api/staff-members/${staffMemberId}/availability`, {
    body: request,
    method: 'POST',
    token,
  });
}

export function updateStaffMemberAvailability(
  staffMemberId: string,
  availabilityId: string,
  request: UpdateStaffMemberAvailabilityRequest,
  token: string,
) {
  return apiRequest<StaffMemberAvailabilityResponse>(`/api/staff-members/${staffMemberId}/availability/${availabilityId}`, {
    body: request,
    method: 'PUT',
    token,
  });
}

export function deleteStaffMemberAvailability(staffMemberId: string, availabilityId: string, token: string) {
  return apiRequest<void>(`/api/staff-members/${staffMemberId}/availability/${availabilityId}`, {
    method: 'DELETE',
    token,
  });
}

export function listStaffMemberAvailabilityExceptions(staffMemberId: string, token: string) {
  return apiRequest<StaffMemberAvailabilityExceptionResponse[]>(`/api/staff-members/${staffMemberId}/availability-exceptions`, { token });
}

export function createStaffMemberAvailabilityException(
  staffMemberId: string,
  request: CreateStaffMemberAvailabilityExceptionRequest,
  token: string,
) {
  return apiRequest<StaffMemberAvailabilityExceptionResponse>(`/api/staff-members/${staffMemberId}/availability-exceptions`, {
    body: request,
    method: 'POST',
    token,
  });
}

export function updateStaffMemberAvailabilityException(
  staffMemberId: string,
  exceptionId: string,
  request: UpdateStaffMemberAvailabilityExceptionRequest,
  token: string,
) {
  return apiRequest<StaffMemberAvailabilityExceptionResponse>(`/api/staff-members/${staffMemberId}/availability-exceptions/${exceptionId}`, {
    body: request,
    method: 'PUT',
    token,
  });
}

export function deleteStaffMemberAvailabilityException(staffMemberId: string, exceptionId: string, token: string) {
  return apiRequest<void>(`/api/staff-members/${staffMemberId}/availability-exceptions/${exceptionId}`, {
    method: 'DELETE',
    token,
  });
}
