import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { render, screen } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { afterEach, expect, test, vi } from 'vitest';

import type { AdminDashboardSummaryResponse } from '@/api/contracts';
import { setAuthSession } from '@/auth/authStorage';
import { AdminDashboardPage } from '@/features/dashboard/AdminDashboardPage';
import { getAdminDashboardSummary } from '@/features/dashboard/dashboardApi';

vi.mock('@/features/dashboard/dashboardApi', () => ({
  getAdminDashboardSummary: vi.fn(),
}));

afterEach(() => {
  localStorage.clear();
  vi.clearAllMocks();
});

test('renders the operational agenda and dashboard signals', async () => {
  setAdminSession();
  vi.mocked(getAdminDashboardSummary).mockResolvedValue(summary());

  renderDashboard();

  expect(await screen.findByRole('heading', { name: "Today's agenda" })).toBeInTheDocument();
  expect(screen.getByText('Haircut')).toBeInTheDocument();
  expect(screen.getByRole('article')).toHaveTextContent('Marta Lopez');
  expect(screen.getByRole('link', { name: /open agenda/i })).toHaveAttribute('href', '/admin/appointments');
  expect(getAdminDashboardSummary).toHaveBeenCalledWith('admin-token');
});

test('renders the empty agenda and status states', async () => {
  setAdminSession();
  vi.mocked(getAdminDashboardSummary).mockResolvedValue(summary({ statusCounts: [], upcomingAppointments: [] }));

  renderDashboard();

  expect(await screen.findByText('No upcoming Appointments')).toBeInTheDocument();
  expect(screen.getByText('No Appointments in this window yet.')).toBeInTheDocument();
});

test('renders a recoverable error state', async () => {
  setAdminSession();
  vi.mocked(getAdminDashboardSummary).mockRejectedValue(new Error('Network error'));

  renderDashboard();

  expect(await screen.findByRole('alert')).toHaveTextContent('Agenda unavailable');
  expect(screen.getByRole('button', { name: /try again/i })).toBeInTheDocument();
});

function renderDashboard() {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } });
  return render(
    <QueryClientProvider client={queryClient}>
      <MemoryRouter><AdminDashboardPage /></MemoryRouter>
    </QueryClientProvider>,
  );
}

function setAdminSession() {
  setAuthSession({ accessToken: 'admin-token', accountType: 'Admin', expiresAtUtc: '2099-01-01T00:00:00Z' });
}

function summary(overrides: Partial<AdminDashboardSummaryResponse> = {}): AdminDashboardSummaryResponse {
  return {
    currencyCode: 'EUR',
    estimatedRevenueAmount: 54,
    rangeEndLocalDate: '2026-09-17',
    rangeStartLocalDate: '2026-09-11',
    statusCounts: [{ count: 2, status: 'Scheduled' }],
    todayAppointmentCount: 3,
    upcomingAppointments: [{
      business: { id: 'business-1', name: 'Studio', slug: 'studio', timeZoneId: 'Europe/Madrid' },
      createdAtUtc: '2026-09-11T08:00:00Z',
      customer: { email: 'marta@example.test', firstName: 'Marta', id: 'customer-1', lastName: 'Lopez' },
      endAtUtc: '2026-09-11T10:30:00Z',
      endTime: '10:30:00',
      id: 'appointment-1',
      localDate: '2026-09-11',
      service: { currencyCodeSnapshot: 'EUR', durationMinutesSnapshot: 30, id: 'service-1', nameSnapshot: 'Haircut', priceAmountSnapshot: 18 },
      staffMember: { displayName: 'Ana', id: 'staff-1' },
      startAtUtc: '2026-09-11T10:00:00Z',
      startTime: '10:00:00',
      status: 'Scheduled',
    }],
    ...overrides,
  };
}
