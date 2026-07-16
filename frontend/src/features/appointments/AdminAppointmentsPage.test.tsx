import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import type { ReactNode } from 'react';
import { MemoryRouter, Route, Routes } from 'react-router-dom';
import { afterEach, describe, expect, it, vi } from 'vitest';

import type { AppointmentSummaryResponse, ServiceResponse, StaffMemberResponse } from '@/api/contracts';
import { setAuthSession } from '@/auth/authStorage';
import * as appointmentApi from '@/features/appointments/appointmentApi';
import * as serviceApi from '@/features/services/serviceApi';
import * as staffMemberApi from '@/features/staffMembers/staffMemberApi';
import { AdminAppointmentsPage } from '@/pages/admin/AdminAppointmentsPage';

vi.mock('@/features/appointments/appointmentApi', () => ({
  listAdminAppointments: vi.fn(),
}));

vi.mock('@/features/services/serviceApi', () => ({
  listServices: vi.fn(),
}));

vi.mock('@/features/staffMembers/staffMemberApi', () => ({
  listStaffMembers: vi.fn(),
}));

const staffMembers: StaffMemberResponse[] = [
  {
    bio: null,
    businessId: 'business-1',
    createdAtUtc: '2026-07-01T00:00:00Z',
    displayName: 'Ana',
    email: 'ana@example.test',
    id: 'staff-1',
    isActive: true,
    phoneNumber: null,
    sortOrder: 1,
  },
];

const services: ServiceResponse[] = [
  {
    businessId: 'business-1',
    createdAtUtc: '2026-07-01T00:00:00Z',
    description: null,
    durationMinutes: 30,
    id: 'service-1',
    isActive: true,
    name: 'Corte',
    priceAmount: 18,
    sortOrder: 1,
  },
];

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
  customerNotes: 'Quiere flequillo',
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

describe('admin appointments page', () => {
  afterEach(() => {
    window.localStorage.clear();
    vi.clearAllMocks();
  });

  it('lists appointments grouped by local date', async () => {
    arrangeSuccessfulQueries([appointment]);

    renderWithProviders(<AdminAppointmentsPage />);

    expect(await screen.findByText('2026-07-20')).toBeInTheDocument();
    expect(screen.getByText('10:00 - 10:30')).toBeInTheDocument();
    expect(screen.getAllByText('Corte')).not.toHaveLength(0);
    expect(screen.getByText('Ana · Clara Diaz')).toBeInTheDocument();
    expect(screen.getByText('Quiere flequillo')).toBeInTheDocument();
    expect(screen.getByRole('link', { name: /ver detalle/i })).toHaveAttribute('href', '/appointments/appointment-1');
    expect(appointmentApi.listAdminAppointments).toHaveBeenCalledWith('admin-token', expect.objectContaining({ from: expect.any(String), to: expect.any(String) }));
  });

  it('applies staff service and status filters', async () => {
    arrangeSuccessfulQueries([appointment]);
    renderWithProviders(<AdminAppointmentsPage />);
    await screen.findByText('2026-07-20');

    const user = userEvent.setup();
    await user.selectOptions(screen.getByLabelText(/staff member/i), 'staff-1');
    await user.selectOptions(screen.getByLabelText(/service/i), 'service-1');
    await user.selectOptions(screen.getByLabelText(/status/i), 'CancelledByCustomer');
    await user.click(screen.getByRole('button', { name: /aplicar filtros/i }));

    await waitFor(() => {
      expect(appointmentApi.listAdminAppointments).toHaveBeenLastCalledWith('admin-token', expect.objectContaining({
        serviceId: 'service-1',
        staffMemberId: 'staff-1',
        status: 'CancelledByCustomer',
      }));
    });
  });

  it('shows an empty state', async () => {
    arrangeSuccessfulQueries([]);

    renderWithProviders(<AdminAppointmentsPage />);

    expect(await screen.findByText(/sin appointments en este rango/i)).toBeInTheDocument();
  });

  it('shows an error state', async () => {
    setAdminSession();
    vi.mocked(staffMemberApi.listStaffMembers).mockResolvedValue(staffMembers);
    vi.mocked(serviceApi.listServices).mockResolvedValue(services);
    vi.mocked(appointmentApi.listAdminAppointments).mockRejectedValue(new Error('Network error'));

    renderWithProviders(<AdminAppointmentsPage />);

    expect(await screen.findByText(/no se pudo completar la operacion/i)).toBeInTheDocument();
  });
});

function arrangeSuccessfulQueries(appointments: AppointmentSummaryResponse[]) {
  setAdminSession();
  vi.mocked(staffMemberApi.listStaffMembers).mockResolvedValue(staffMembers);
  vi.mocked(serviceApi.listServices).mockResolvedValue(services);
  vi.mocked(appointmentApi.listAdminAppointments).mockResolvedValue(appointments);
}

function setAdminSession() {
  setAuthSession({
    accountType: 'Admin',
    businessId: 'business-1',
    displayName: 'Admin Demo',
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
      <MemoryRouter initialEntries={['/admin/appointments']}>
        <Routes>
          <Route element={children} path="/admin/appointments" />
        </Routes>
      </MemoryRouter>
    </QueryClientProvider>,
  );
}
