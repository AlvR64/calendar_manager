import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { render, screen } from '@testing-library/react';
import type { ReactNode } from 'react';
import { MemoryRouter, Route, Routes } from 'react-router-dom';
import { afterEach, describe, expect, it, vi } from 'vitest';

import type { AdminDashboardSummaryResponse, AppointmentSummaryResponse } from '@/api/contracts';
import { setAuthSession } from '@/auth/authStorage';
import * as dashboardApi from '@/features/dashboard/dashboardApi';
import { AdminDashboardPage } from '@/pages/admin/AdminDashboardPage';

vi.mock('@/features/dashboard/dashboardApi', () => ({
  getAdminDashboardSummary: vi.fn(),
}));

const appointment: AppointmentSummaryResponse = {
  business: {
    id: 'business-1',
    name: 'Barberia Centro',
    slug: 'barberia-centro',
    timeZoneId: 'Europe/Madrid',
  },
  cancelledAtUtc: null,
  cancellationReason: null,
  createdAtUtc: '2026-07-16T10:00:00Z',
  customer: {
    email: 'clara@example.test',
    firstName: 'Clara',
    id: 'customer-1',
    lastName: 'Diaz',
  },
  customerNotes: null,
  endAtUtc: '2026-07-20T08:30:00Z',
  endTime: '10:30:00',
  id: 'appointment-1',
  internalNotes: null,
  localDate: '2026-07-20',
  service: {
    currencyCodeSnapshot: 'EUR',
    durationMinutesSnapshot: 30,
    id: 'service-1',
    nameSnapshot: 'Corte',
    priceAmountSnapshot: 18,
  },
  staffMember: {
    displayName: 'Ana',
    id: 'staff-1',
  },
  startAtUtc: '2026-07-20T08:00:00Z',
  startTime: '10:00:00',
  status: 'Scheduled',
};

describe('admin dashboard page', () => {
  afterEach(() => {
    window.localStorage.clear();
    vi.clearAllMocks();
  });

  it('renders dashboard metrics and upcoming appointments', async () => {
    const summary = createSummary({ upcomingAppointments: [appointment] });
    arrangeSummary(summary);

    renderWithProviders(<AdminDashboardPage />);

    expect(await screen.findByText('Resumen operativo')).toBeInTheDocument();
    expect(await screen.findByText('2026-07-20 · 10:00 - 10:30')).toBeInTheDocument();
    expect(screen.getByText('3')).toBeInTheDocument();
    expect(screen.getByText(/54,00\s*€/)).toBeInTheDocument();
    expect(screen.getByText('Corte · Ana')).toBeInTheDocument();
    expect(screen.getByRole('link', { name: /ver agenda completa/i })).toHaveAttribute('href', '/admin/appointments');
    expect(dashboardApi.getAdminDashboardSummary).toHaveBeenCalledWith('admin-token');
  });

  it('renders empty dashboard panels', async () => {
    arrangeSummary(createSummary({ statusCounts: [], upcomingAppointments: [] }));

    renderWithProviders(<AdminDashboardPage />);

    expect(await screen.findByText(/sin proximos appointments/i)).toBeInTheDocument();
    expect(screen.getByText(/no hay appointments en el rango/i)).toBeInTheDocument();
  });

  it('renders an error state', async () => {
    setAdminSession();
    vi.mocked(dashboardApi.getAdminDashboardSummary).mockRejectedValue(new Error('Network error'));

    renderWithProviders(<AdminDashboardPage />);

    expect(await screen.findByText(/no se pudo completar la operacion/i)).toBeInTheDocument();
  });
});

function arrangeSummary(summary: AdminDashboardSummaryResponse) {
  setAdminSession();
  vi.mocked(dashboardApi.getAdminDashboardSummary).mockResolvedValue(summary);
}

function createSummary(overrides: Partial<AdminDashboardSummaryResponse> = {}): AdminDashboardSummaryResponse {
  return {
    currencyCode: 'EUR',
    estimatedRevenueAmount: 54,
    rangeEndLocalDate: '2026-07-24',
    rangeStartLocalDate: '2026-07-17',
    statusCounts: [
      { count: 2, status: 'Scheduled' },
      { count: 1, status: 'CancelledByCustomer' },
    ],
    todayAppointmentCount: 3,
    upcomingAppointments: [],
    ...overrides,
  };
}

function setAdminSession() {
  setAuthSession({
    accountType: 'Admin',
    businessId: 'business-1',
    displayName: 'Admin Demo',
    email: 'admin@example.test',
    expiresAtUtc: '2099-07-20T00:00:00Z',
    id: 'admin-1',
    token: 'admin-token',
    tokenType: 'Bearer',
  });
}

function renderWithProviders(children: ReactNode) {
  const queryClient = new QueryClient({
    defaultOptions: {
      queries: { retry: false },
    },
  });

  return render(
    <QueryClientProvider client={queryClient}>
      <MemoryRouter initialEntries={['/admin']}>
        <Routes>
          <Route element={children} path="/admin" />
        </Routes>
      </MemoryRouter>
    </QueryClientProvider>,
  );
}
