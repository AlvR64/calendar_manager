import type { AdminDashboardSummaryResponse } from '@/api/contracts';
import { apiRequest } from '@/api/httpClient';

export function getAdminDashboardSummary(accessToken: string): Promise<AdminDashboardSummaryResponse> {
  return apiRequest<AdminDashboardSummaryResponse>('/api/admin/dashboard-summary', { token: accessToken });
}
