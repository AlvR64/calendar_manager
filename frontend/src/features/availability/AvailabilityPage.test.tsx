import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { render, screen, waitFor, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import type { ReactNode } from 'react';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';

import type { BusinessResponse, StaffMemberAvailabilityExceptionResponse, StaffMemberAvailabilityResponse, StaffMemberResponse } from '@/api/contracts';
import { ApiError } from '@/api/httpClient';
import { setAuthSession } from '@/auth/authStorage';
import * as availabilityApi from '@/features/availability/availabilityApi';
import * as businessApi from '@/features/businesses/businessApi';
import * as staffMemberApi from '@/features/staffMembers/staffMemberApi';
import { AvailabilityPage } from '@/pages/admin/AvailabilityPage';

vi.mock('@/features/availability/availabilityApi', () => ({
  createStaffMemberAvailability: vi.fn(),
  createStaffMemberAvailabilityException: vi.fn(),
  deleteStaffMemberAvailability: vi.fn(),
  deleteStaffMemberAvailabilityException: vi.fn(),
  listStaffMemberAvailabilities: vi.fn(),
  listStaffMemberAvailabilityExceptions: vi.fn(),
  updateStaffMemberAvailability: vi.fn(),
  updateStaffMemberAvailabilityException: vi.fn(),
}));

vi.mock('@/features/businesses/businessApi', () => ({
  getBusinessById: vi.fn(),
  updateCurrentBusinessBookingWindow: vi.fn(),
  updateCurrentBusinessDetails: vi.fn(),
}));

vi.mock('@/features/staffMembers/staffMemberApi', () => ({
  createStaffMember: vi.fn(),
  deleteStaffMember: vi.fn(),
  getStaffMember: vi.fn(),
  listStaffMembers: vi.fn(),
  updateStaffMember: vi.fn(),
  updateStaffMemberActiveState: vi.fn(),
}));

const business: BusinessResponse = {
  city: 'Madrid',
  contactEmail: 'contact@example.com',
  contactPhoneNumber: null,
  countryCode: 'ES',
  currencyCode: 'EUR',
  description: null,
  id: 'business-1',
  maxAdvanceBookingDays: 60,
  name: 'Barberia Centro',
  postalCode: '28013',
  slug: 'barberia-centro',
  timeZoneId: 'Europe/Madrid',
  websiteUrl: null,
};

const ana: StaffMemberResponse = {
  bio: null,
  businessId: 'business-1',
  createdAtUtc: '2026-07-15T18:00:00Z',
  displayName: 'Ana Ruiz',
  email: 'ana@example.com',
  id: 'staff-1',
  isActive: true,
  phoneNumber: null,
  sortOrder: 1,
};

const mario: StaffMemberResponse = {
  bio: null,
  businessId: 'business-1',
  createdAtUtc: '2026-07-15T18:00:00Z',
  displayName: 'Mario Lopez',
  email: null,
  id: 'staff-2',
  isActive: false,
  phoneNumber: null,
  sortOrder: 2,
};

const mondayMorning: StaffMemberAvailabilityResponse = {
  createdAtUtc: '2026-07-15T18:00:00Z',
  dayOfWeek: 1,
  endTime: '13:00:00',
  id: 'availability-1',
  isActive: true,
  staffMemberId: 'staff-1',
  startTime: '09:00:00',
};

const closedDay: StaffMemberAvailabilityExceptionResponse = {
  createdAtUtc: '2026-07-15T18:00:00Z',
  endTime: null,
  id: 'exception-1',
  isClosed: true,
  localDate: '2026-07-20',
  reason: 'Vacaciones',
  staffMemberId: 'staff-1',
  startTime: null,
};

describe('availability page', () => {
  beforeEach(() => {
    setAdminSession();
    vi.mocked(businessApi.getBusinessById).mockResolvedValue(business);
    vi.mocked(staffMemberApi.listStaffMembers).mockResolvedValue([ana, mario]);
    vi.mocked(availabilityApi.listStaffMemberAvailabilities).mockResolvedValue([]);
    vi.mocked(availabilityApi.listStaffMemberAvailabilityExceptions).mockResolvedValue([]);
  });

  afterEach(() => {
    window.localStorage.clear();
    vi.clearAllMocks();
    vi.unstubAllGlobals();
  });

  it('loads staff members, timezone, weekly availability, and exceptions', async () => {
    vi.mocked(availabilityApi.listStaffMemberAvailabilities).mockResolvedValue([mondayMorning]);
    vi.mocked(availabilityApi.listStaffMemberAvailabilityExceptions).mockResolvedValue([closedDay]);

    renderWithProviders(<AvailabilityPage />);

    expect(await screen.findByText(/Europe\/Madrid/)).toBeInTheDocument();
    expect(screen.getByDisplayValue('Ana Ruiz')).toBeInTheDocument();
    expect(await screen.findByRole('heading', { name: 'Lunes' })).toBeInTheDocument();
    expect(screen.getByText((_, element) => element?.textContent === '09:00 - 13:00')).toBeInTheDocument();
    expect(screen.getByText('2026-07-20')).toBeInTheDocument();
    expect(screen.getByText('Closed day')).toBeInTheDocument();
    expect(availabilityApi.listStaffMemberAvailabilities).toHaveBeenCalledWith('staff-1', 'admin-token');
  });

  it('creates weekly availability', async () => {
    vi.mocked(availabilityApi.createStaffMemberAvailability).mockResolvedValue(mondayMorning);

    renderWithProviders(<AvailabilityPage />);

    expect(await screen.findByText('Sin disponibilidad semanal')).toBeInTheDocument();

    await userEvent.click(screen.getByRole('button', { name: /crear bloque/i }));

    await waitFor(() => {
      expect(availabilityApi.createStaffMemberAvailability).toHaveBeenCalledWith(
        'staff-1',
        { dayOfWeek: 1, endTime: '17:00:00', startTime: '09:00:00' },
        'admin-token',
      );
    });
  });

  it('updates and deletes weekly availability', async () => {
    vi.stubGlobal('confirm', vi.fn(() => true));
    vi.mocked(availabilityApi.listStaffMemberAvailabilities).mockResolvedValue([mondayMorning]);
    vi.mocked(availabilityApi.updateStaffMemberAvailability).mockResolvedValue({ ...mondayMorning, endTime: '14:00:00' });
    vi.mocked(availabilityApi.deleteStaffMemberAvailability).mockResolvedValue(undefined);

    renderWithProviders(<AvailabilityPage />);

    const weeklySection = await screen.findByRole('heading', { name: /disponibilidad semanal/i }).then((heading) => heading.closest('section')!);
    const user = userEvent.setup();
    await user.click(within(weeklySection).getByRole('button', { name: /editar/i }));
    await user.clear(within(weeklySection).getByLabelText(/end time/i));
    await user.type(within(weeklySection).getByLabelText(/end time/i), '14:00');
    await user.click(screen.getByRole('button', { name: /guardar bloque/i }));

    await waitFor(() => {
      expect(availabilityApi.updateStaffMemberAvailability).toHaveBeenCalledWith(
        'staff-1',
        'availability-1',
        { dayOfWeek: 1, endTime: '14:00:00', startTime: '09:00:00' },
        'admin-token',
      );
    });

    await user.click(within(weeklySection).getByRole('button', { name: /eliminar/i }));
    await waitFor(() => {
      expect(availabilityApi.deleteStaffMemberAvailability).toHaveBeenCalledWith('staff-1', 'availability-1', 'admin-token');
    });
  });

  it('creates a special-hour exception', async () => {
    vi.mocked(availabilityApi.createStaffMemberAvailabilityException).mockResolvedValue({
      ...closedDay,
      endTime: '14:00:00',
      isClosed: false,
      reason: 'Horario especial',
      startTime: '10:00:00',
    });

    renderWithProviders(<AvailabilityPage />);

    expect(await screen.findByText('Sin excepciones')).toBeInTheDocument();

    const user = userEvent.setup();
    await user.type(screen.getByLabelText(/local date/i), '2026-07-20');
    await user.click(screen.getByLabelText(/closed all day/i));
    await user.type(screen.getAllByLabelText(/start time/i)[1], '10:00');
    await user.type(screen.getAllByLabelText(/end time/i)[1], '14:00');
    await user.type(screen.getByLabelText(/reason/i), 'Horario especial');
    await user.click(screen.getByRole('button', { name: /crear excepcion/i }));

    await waitFor(() => {
      expect(availabilityApi.createStaffMemberAvailabilityException).toHaveBeenCalledWith(
        'staff-1',
        {
          endTime: '14:00:00',
          isClosed: false,
          localDate: '2026-07-20',
          reason: 'Horario especial',
          startTime: '10:00:00',
        },
        'admin-token',
      );
    });
  });

  it('shows exception conflicts', async () => {
    vi.mocked(availabilityApi.createStaffMemberAvailabilityException).mockRejectedValue(
      new ApiError('Conflict', 409, { detail: 'The availability exception overlaps with an existing exception.' }),
    );

    renderWithProviders(<AvailabilityPage />);

    expect(await screen.findByText('Sin excepciones')).toBeInTheDocument();

    const user = userEvent.setup();
    await user.type(screen.getByLabelText(/local date/i), '2026-07-20');
    await user.click(screen.getByRole('button', { name: /crear excepcion/i }));

    expect(await screen.findByRole('alert')).toHaveTextContent('The availability exception overlaps with an existing exception.');
  });
});

function setAdminSession() {
  setAuthSession({
    accountType: 'Admin',
    businessId: 'business-1',
    displayName: 'Admin One',
    email: 'admin@example.com',
    expiresAtUtc: '2026-07-15T18:00:00Z',
    id: 'admin-1',
    token: 'admin-token',
    tokenType: 'Bearer',
  });
}

function renderWithProviders(children: ReactNode) {
  const queryClient = new QueryClient({
    defaultOptions: {
      mutations: { retry: false },
      queries: { retry: false },
    },
  });

  return render(<QueryClientProvider client={queryClient}>{children}</QueryClientProvider>);
}
