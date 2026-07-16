import type { AdminDashboardSummaryResponse } from '@/api/contracts';
import { apiRequest } from '@/api/httpClient';

export type AdminDashboardSummaryFilters = {
  from?: string;
  to?: string;
};

export function getAdminDashboardSummary(token: string, filters: AdminDashboardSummaryFilters = {}) {
  const searchParams = new URLSearchParams();
  if (filters.from) {
    searchParams.set('from', filters.from);
  }

  if (filters.to) {
    searchParams.set('to', filters.to);
  }

  const queryString = searchParams.toString();
  return apiRequest<AdminDashboardSummaryResponse>(`/api/admin/dashboard-summary${queryString ? `?${queryString}` : ''}`, { token });
}
