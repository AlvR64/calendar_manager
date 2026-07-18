import type {
  AvailableSlotResponse,
  BusinessProfileResponse,
  BusinessResponse,
  BusinessServiceResponse,
  BusinessStaffMemberResponse,
  PublicBusinessSearchResponse,
} from '@/api/contracts';
import { apiRequest } from '@/api/httpClient';

export type PublicBusinessSearchParams = {
  query?: string | null;
  city?: string | null;
  category?: string | null;
  service?: string | null;
  page?: number;
  pageSize?: number;
};

export function searchPublicBusinesses(params: PublicBusinessSearchParams = {}) {
  const searchParams = new URLSearchParams();

  appendOptionalParam(searchParams, 'query', params.query);
  appendOptionalParam(searchParams, 'city', params.city);
  appendOptionalParam(searchParams, 'category', params.category);
  appendOptionalParam(searchParams, 'service', params.service);

  if (params.page) {
    searchParams.set('page', String(params.page));
  }

  if (params.pageSize) {
    searchParams.set('pageSize', String(params.pageSize));
  }

  const queryString = searchParams.toString();
  return apiRequest<PublicBusinessSearchResponse>(`/api/public/businesses${queryString ? `?${queryString}` : ''}`);
}

export function getPublicBusinessById(businessId: string) {
  return apiRequest<BusinessResponse>(`/api/businesses/${businessId}`);
}

export function getPublicBusinessProfileById(businessId: string) {
  return apiRequest<BusinessProfileResponse>(`/api/businesses/${businessId}/profile`);
}

export function getPublicBusinessProfileBySlug(slug: string) {
  return apiRequest<BusinessProfileResponse>(`/api/businesses/by-slug/${slug}/profile`);
}

export function listPublicBusinessServices(businessId: string) {
  return apiRequest<BusinessServiceResponse[]>(`/api/businesses/${businessId}/services`);
}

export function getPublicBusinessService(businessId: string, serviceId: string) {
  return apiRequest<BusinessServiceResponse>(`/api/businesses/${businessId}/services/${serviceId}`);
}

export function listPublicBusinessStaffMembers(businessId: string) {
  return apiRequest<BusinessStaffMemberResponse[]>(`/api/businesses/${businessId}/staff-members`);
}

export function getPublicBusinessStaffMember(businessId: string, staffMemberId: string) {
  return apiRequest<BusinessStaffMemberResponse>(`/api/businesses/${businessId}/staff-members/${staffMemberId}`);
}

export function listPublicAvailableSlots(businessId: string, serviceId: string, date: string, staffMemberId?: string) {
  const query = `date=${encodeURIComponent(date)}`;

  if (staffMemberId) {
    return apiRequest<AvailableSlotResponse[]>(
      `/api/businesses/${businessId}/services/${serviceId}/staff-members/${staffMemberId}/available-slots?${query}`,
    );
  }

  return apiRequest<AvailableSlotResponse[]>(`/api/businesses/${businessId}/services/${serviceId}/available-slots?${query}`);
}

function appendOptionalParam(searchParams: URLSearchParams, key: string, value?: string | null) {
  const normalizedValue = value?.trim();

  if (normalizedValue) {
    searchParams.set(key, normalizedValue);
  }
}
