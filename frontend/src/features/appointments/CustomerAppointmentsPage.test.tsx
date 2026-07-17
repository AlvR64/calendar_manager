import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import type { ReactNode } from 'react';
import { MemoryRouter, Route, Routes } from 'react-router-dom';
import { afterEach, describe, expect, it, vi } from 'vitest';

import type { AppointmentDetailsResponse, AppointmentSummaryResponse } from '@/api/contracts';
import { setAuthSession } from '@/auth/authStorage';
import * as appointmentApi from '@/features/appointments/appointmentApi';
import { CustomerAppointmentsPage } from '@/pages/customer/CustomerAppointmentsPage';

vi.mock('@/features/appointments/appointmentApi', () => ({
  cancelCustomerAppointment: vi.fn(),
  listCustomerAppointments: vi.fn(),
}));

const scheduledAppointment: AppointmentSummaryResponse = {
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
    email: 'customer@example.test',
    firstName: 'Clara',
    id: 'customer-1',
    lastName: null,
  },
  customerNotes: 'Notas',
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

const cancelledAppointment: AppointmentSummaryResponse = {
  ...scheduledAppointment,
  cancelledAtUtc: '2026-07-17T10:00:00Z',
  cancellationReason: 'No puedo ir',
  id: 'appointment-2',
  status: 'CancelledByCustomer',
};

describe('customer appointments page', () => {
  afterEach(() => {
    window.localStorage.clear();
    vi.clearAllMocks();
  });

  it('lists upcoming and cancelled appointments', async () => {
    setCustomerSession();
    vi.mocked(appointmentApi.listCustomerAppointments).mockResolvedValue([scheduledAppointment, cancelledAppointment]);

    renderWithProviders(<CustomerAppointmentsPage />);

    expect(await screen.findByText('Clara')).toBeInTheDocument();
    expect(screen.getByText('customer@example.test')).toBeInTheDocument();
    expect(await screen.findAllByText('Barberia Centro · Ana')).not.toHaveLength(0);
    expect(screen.getAllByText('2026-07-20 · 10:00 - 10:30')).not.toHaveLength(0);
    expect(screen.getByText('No puedo ir')).toBeInTheDocument();
    expect(screen.getAllByRole('link', { name: /ver detalle/i })[0]).toHaveAttribute('href', '/appointments/appointment-1');
    expect(appointmentApi.listCustomerAppointments).toHaveBeenCalledWith('customer-token', { from: undefined, status: undefined, to: undefined });
  });

  it('shows an empty state', async () => {
    setCustomerSession();
    vi.mocked(appointmentApi.listCustomerAppointments).mockResolvedValue([]);

    renderWithProviders(<CustomerAppointmentsPage />);

    expect(await screen.findByText(/no tienes appointments todavia/i)).toBeInTheDocument();
  });

  it('cancels an appointment and refreshes the list', async () => {
    setCustomerSession();
    vi.mocked(appointmentApi.listCustomerAppointments)
      .mockResolvedValueOnce([scheduledAppointment])
      .mockResolvedValueOnce([{ ...scheduledAppointment, cancelledAtUtc: '2026-07-17T10:00:00Z', cancellationReason: 'No puedo ir', status: 'CancelledByCustomer' }]);
    vi.mocked(appointmentApi.cancelCustomerAppointment).mockResolvedValue(toDetails({
      ...scheduledAppointment,
      cancelledAtUtc: '2026-07-17T10:00:00Z',
      cancellationReason: 'No puedo ir',
      status: 'CancelledByCustomer',
    }));

    renderWithProviders(<CustomerAppointmentsPage />);

    const user = userEvent.setup();
    await user.click(await screen.findByRole('button', { name: /cancelar/i }));
    await user.type(screen.getByLabelText(/motivo opcional/i), 'No puedo ir');
    await user.click(screen.getByRole('button', { name: /confirmar cancelacion/i }));

    await waitFor(() => {
      expect(appointmentApi.cancelCustomerAppointment).toHaveBeenCalledWith('appointment-1', { cancellationReason: 'No puedo ir' }, 'customer-token');
    });
    await waitFor(() => {
      expect(appointmentApi.listCustomerAppointments).toHaveBeenCalledTimes(2);
    });
    expect(await screen.findByText('CancelledByCustomer')).toBeInTheDocument();
    expect(screen.getByText('No puedo ir')).toBeInTheDocument();
  });
});

function setCustomerSession() {
  setAuthSession({
    accountType: 'Customer',
    email: 'customer@example.test',
    expiresAtUtc: '2026-07-20T00:00:00Z',
    firstName: 'Clara',
    id: 'customer-1',
    lastName: null,
    token: 'customer-token',
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
      <MemoryRouter initialEntries={['/customer/appointments']}>
        <Routes>
          <Route element={children} path="/customer/appointments" />
        </Routes>
      </MemoryRouter>
    </QueryClientProvider>,
  );
}

function toDetails(appointment: AppointmentSummaryResponse): AppointmentDetailsResponse {
  return {
    ...appointment,
    customer: {
      email: 'customer@example.test',
      firstName: 'Clara',
      id: 'customer-1',
      lastName: null,
    },
  };
}
