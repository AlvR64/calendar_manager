import type { StaffMemberServiceAssignmentResponse, UpdateStaffMemberServiceActiveStateRequest } from '@/api/contracts';
import { apiRequest } from '@/api/httpClient';

export function listStaffMemberServiceAssignments(staffMemberId: string, token: string) {
  return apiRequest<StaffMemberServiceAssignmentResponse[]>(`/api/staff-members/${staffMemberId}/services`, { token });
}

export function listServiceStaffMemberAssignments(serviceId: string, token: string) {
  return apiRequest<StaffMemberServiceAssignmentResponse[]>(`/api/services/${serviceId}/staff-members`, { token });
}

export function assignServiceToStaffMember(staffMemberId: string, serviceId: string, token: string) {
  return apiRequest<StaffMemberServiceAssignmentResponse>(`/api/staff-members/${staffMemberId}/services/${serviceId}`, {
    method: 'POST',
    token,
  });
}

export function assignStaffMemberToService(serviceId: string, staffMemberId: string, token: string) {
  return apiRequest<StaffMemberServiceAssignmentResponse>(`/api/services/${serviceId}/staff-members/${staffMemberId}`, {
    method: 'POST',
    token,
  });
}

export function updateStaffMemberServiceAssignmentActiveState(
  staffMemberId: string,
  serviceId: string,
  request: UpdateStaffMemberServiceActiveStateRequest,
  token: string,
) {
  return apiRequest<StaffMemberServiceAssignmentResponse>(`/api/staff-members/${staffMemberId}/services/${serviceId}/active-state`, {
    body: request,
    method: 'PUT',
    token,
  });
}

export function unassignServiceFromStaffMember(staffMemberId: string, serviceId: string, token: string) {
  return apiRequest<void>(`/api/staff-members/${staffMemberId}/services/${serviceId}`, {
    method: 'DELETE',
    token,
  });
}
