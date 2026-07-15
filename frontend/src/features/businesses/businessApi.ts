import type {
  BusinessBookingWindowResponse,
  BusinessResponse,
  UpdateBusinessBookingWindowRequest,
  UpdateBusinessDetailsRequest,
} from '@/api/contracts';
import { apiRequest } from '@/api/httpClient';

export function getBusinessById(businessId: string) {
  return apiRequest<BusinessResponse>(`/api/businesses/${businessId}`);
}

export function updateCurrentBusinessDetails(request: UpdateBusinessDetailsRequest, token: string) {
  return apiRequest<BusinessResponse>('/api/businesses/current', {
    body: request,
    method: 'PUT',
    token,
  });
}

export function updateCurrentBusinessBookingWindow(request: UpdateBusinessBookingWindowRequest, token: string) {
  return apiRequest<BusinessBookingWindowResponse>('/api/businesses/current/booking-window', {
    body: request,
    method: 'PUT',
    token,
  });
}
