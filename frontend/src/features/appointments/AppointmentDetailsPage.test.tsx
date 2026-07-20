import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { render, screen } from '@testing-library/react';
import type { ReactNode } from 'react';
import { MemoryRouter, Route, Routes } from 'react-router-dom';
import { afterEach, describe, expect, it, vi } from 'vitest';

import type { AppointmentDetailsResponse } from '@/api/contracts';
import { ApiError } from '@/api/httpClient';
import { setAuthSession } from '@/auth/authStorage';
import * as appointmentApi from '@/features/appointments/appointmentApi';
import { AppointmentDetailsPage } from '@/pages/public/AppointmentDetailsPage';

vi.mock('@/features/appointments/appointmentApi', () => ({
  getAppointmentDetails: vi.fn(),
}));

const appointment: AppointmentDetailsResponse = {
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
  customerNotes: 'Notas del customer',
  endAtUtc: '2026-07-20T08:30:00Z',
  endTime: '10:30:00',
  id: 'appointment-1',
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

describe('appointment details page', () => {
  afterEach(() => {
    window.localStorage.clear();
    vi.clearAllMocks();
  });

  it('asks visitors to log in without fetching appointment details', () => {
    renderWithProviders(<AppointmentDetailsPage />);

    expect(screen.getByText(/entra para ver el appointment/i)).toBeInTheDocument();
    expect(screen.getByRole('link', { name: /entrar como customer/i })).toHaveAttribute('href', expect.stringContaining('/auth/customer/login?returnTo='));
    expect(screen.getByRole('link', { name: /entrar como admin/i })).toHaveAttribute('href', expect.stringContaining('/auth/admin/login?returnTo='));
    expect(appointmentApi.getAppointmentDetails).not.toHaveBeenCalled();
  });

  it('renders appointment details with local business time', async () => {
    setCustomerSession();
    vi.mocked(appointmentApi.getAppointmentDetails).mockResolvedValue(appointment);

    renderWithProviders(<AppointmentDetailsPage />);

    expect(await screen.findAllByText('Corte')).not.toHaveLength(0);
    expect(screen.getByText('10:00 - 10:30')).toBeInTheDocument();
    expect(screen.getByText('2026-07-20 · Europe/Madrid')).toBeInTheDocument();
    expect(screen.getByText('Ana')).toBeInTheDocument();
    expect(screen.getAllByText('Clara Diaz')).not.toHaveLength(0);
    expect(screen.getByText('Notas del customer')).toBeInTheDocument();
    expect(appointmentApi.getAppointmentDetails).toHaveBeenCalledWith('appointment-1', 'customer-token');
  });

  it('uses the only active session after admin login clears customer session', async () => {
    setCustomerSession();
    setAdminSession();
    vi.mocked(appointmentApi.getAppointmentDetails).mockResolvedValue(appointment);

    renderWithProviders(<AppointmentDetailsPage />);

    expect(await screen.findAllByText('Corte')).not.toHaveLength(0);
    expect(appointmentApi.getAppointmentDetails).toHaveBeenCalledWith('appointment-1', 'admin-token');
  });

  it('shows a loading state while details are loading', () => {
    setCustomerSession();
    vi.mocked(appointmentApi.getAppointmentDetails).mockReturnValue(new Promise(() => {}));

    renderWithProviders(<AppointmentDetailsPage />);

    expect(screen.getByText(/cargando appointment/i)).toBeInTheDocument();
  });

  it('shows not found state', async () => {
    setCustomerSession();
    vi.mocked(appointmentApi.getAppointmentDetails).mockRejectedValue(new ApiError('Not found', 404, { title: 'Appointment not found.' }));

    renderWithProviders(<AppointmentDetailsPage />);

    expect(await screen.findByText(/appointment no encontrado/i)).toBeInTheDocument();
  });

  it('shows forbidden state', async () => {
    setCustomerSession();
    vi.mocked(appointmentApi.getAppointmentDetails).mockRejectedValue(new ApiError('Forbidden', 403, { title: 'Forbidden' }));

    renderWithProviders(<AppointmentDetailsPage />);

    expect(await screen.findByText(/sin acceso a este appointment/i)).toBeInTheDocument();
  });
});

function setCustomerSession() {
  setAuthSession({
    accountType: 'Customer',
    email: 'clara@example.test',
    expiresAtUtc: '2026-07-20T00:00:00Z',
    firstName: 'Clara',
    id: 'customer-1',
    lastName: 'Diaz',
    token: 'customer-token',
    tokenType: 'Bearer',
  });
}

function setAdminSession() {
  setAuthSession({
    accountType: 'Admin',
    businessId: 'business-1',
    displayName: 'Admin One',
    email: 'admin@example.test',
    expiresAtUtc: '2026-07-20T00:00:00Z',
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
      <MemoryRouter initialEntries={['/appointments/appointment-1']}>
        <Routes>
          <Route element={children} path="/appointments/:appointmentId" />
        </Routes>
      </MemoryRouter>
    </QueryClientProvider>,
  );
}
