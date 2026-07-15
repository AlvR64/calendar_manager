import type {
  CreateStaffMemberRequest,
  StaffMemberResponse,
  UpdateStaffMemberActiveStateRequest,
  UpdateStaffMemberRequest,
} from '@/api/contracts';
import { apiRequest } from '@/api/httpClient';

export function listStaffMembers(token: string) {
  return apiRequest<StaffMemberResponse[]>('/api/staff-members', { token });
}

export function getStaffMember(staffMemberId: string, token: string) {
  return apiRequest<StaffMemberResponse>(`/api/staff-members/${staffMemberId}`, { token });
}

export function createStaffMember(request: CreateStaffMemberRequest, token: string) {
  return apiRequest<StaffMemberResponse>('/api/staff-members', {
    body: request,
    method: 'POST',
    token,
  });
}

export function updateStaffMember(staffMemberId: string, request: UpdateStaffMemberRequest, token: string) {
  return apiRequest<StaffMemberResponse>(`/api/staff-members/${staffMemberId}`, {
    body: request,
    method: 'PUT',
    token,
  });
}

export function updateStaffMemberActiveState(staffMemberId: string, request: UpdateStaffMemberActiveStateRequest, token: string) {
  return apiRequest<StaffMemberResponse>(`/api/staff-members/${staffMemberId}/active-state`, {
    body: request,
    method: 'PUT',
    token,
  });
}

export function deleteStaffMember(staffMemberId: string, token: string) {
  return apiRequest<void>(`/api/staff-members/${staffMemberId}`, {
    method: 'DELETE',
    token,
  });
}
