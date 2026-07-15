import type {
  LoginAdminRequest,
  LoginAdminResponse,
  LoginCustomerRequest,
  LoginCustomerResponse,
  RegisterBusinessRequest,
  RegisterBusinessResponse,
  RegisterCustomerRequest,
  RegisterCustomerResponse,
} from '@/api/contracts';
import { apiRequest } from '@/api/httpClient';

export function loginAdmin(request: LoginAdminRequest) {
  return apiRequest<LoginAdminResponse>('/api/auth/admin/login', {
    body: request,
    method: 'POST',
  });
}

export function loginCustomer(request: LoginCustomerRequest) {
  return apiRequest<LoginCustomerResponse>('/api/auth/customer/login', {
    body: request,
    method: 'POST',
  });
}

export function registerBusiness(request: RegisterBusinessRequest) {
  return apiRequest<RegisterBusinessResponse>('/api/auth/register-business', {
    body: request,
    method: 'POST',
  });
}

export function registerCustomer(request: RegisterCustomerRequest) {
  return apiRequest<RegisterCustomerResponse>('/api/auth/register-customer', {
    body: request,
    method: 'POST',
  });
}
