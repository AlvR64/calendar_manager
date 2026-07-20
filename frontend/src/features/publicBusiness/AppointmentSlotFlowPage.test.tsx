import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { fireEvent, render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import type { ReactNode } from 'react';
import { MemoryRouter, Route, Routes } from 'react-router-dom';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';

import type { AvailableSlotResponse, BusinessProfileResponse } from '@/api/contracts';
import { ApiError } from '@/api/httpClient';
import { setAuthSession } from '@/auth/authStorage';
import * as appointmentApi from '@/features/appointments/appointmentApi';
import * as publicBusinessApi from '@/features/publicBusiness/publicBusinessApi';
import { AppointmentSlotFlowPage } from '@/pages/public/AppointmentSlotFlowPage';

vi.mock('@/features/appointments/appointmentApi', () => ({
  createAppointment: vi.fn(),
}));

vi.mock('@/features/publicBusiness/publicBusinessApi', () => ({
  getPublicBusinessById: vi.fn(),
  getPublicBusinessProfileById: vi.fn(),
  getPublicBusinessProfileBySlug: vi.fn(),
  getPublicBusinessService: vi.fn(),
  getPublicBusinessStaffMember: vi.fn(),
  listPublicAvailableSlots: vi.fn(),
  listPublicBusinessServices: vi.fn(),
  listPublicBusinessStaffMembers: vi.fn(),
}));

const profile: BusinessProfileResponse = {
  assignments: [
    { serviceId: 'service-1', staffMemberId: 'staff-1' },
    { serviceId: 'service-2', staffMemberId: 'staff-2' },
  ],
  business: {
    addressLine1: 'Calle Mayor 1',
    addressLine2: null,
    city: 'Madrid',
    contactEmail: 'hola@barberia.test',
    contactPhoneNumber: null,
    countryCode: 'ES',
    currencyCode: 'EUR',
    description: 'Cortes modernos y barba cuidada en el centro.',
    id: 'business-1',
    maxAdvanceBookingDays: 60,
    name: 'Barberia Centro',
    postalCode: '28013',
    slug: 'barberia-centro',
    timeZoneId: 'Europe/Madrid',
    websiteUrl: null,
  },
  services: [
    {
      description: 'Corte con lavado incluido.',
      durationMinutes: 45,
      id: 'service-1',
      name: 'Corte clasico',
      priceAmount: 25,
      sortOrder: 1,
    },
    {
      description: null,
      durationMinutes: 30,
      id: 'service-2',
      name: 'Barba premium',
      priceAmount: 18,
      sortOrder: 2,
    },
  ],
  staffMembers: [
    {
      bio: 'Especialista en cortes clasicos.',
      displayName: 'Ana Ruiz',
      id: 'staff-1',
      sortOrder: 1,
    },
    {
      bio: null,
      displayName: 'Mario Lopez',
      id: 'staff-2',
      sortOrder: 2,
    },
  ],
};

const slots: AvailableSlotResponse[] = [
  {
    endAtUtc: '2026-07-20T08:45:00Z',
    endTime: '10:45:00',
    localDate: '2026-07-20',
    staffMemberId: 'staff-1',
    startAtUtc: '2026-07-20T08:00:00Z',
    startTime: '10:00:00',
  },
  {
    endAtUtc: '2026-07-20T09:00:00Z',
    endTime: '11:00:00',
    localDate: '2026-07-20',
    staffMemberId: 'staff-1',
    startAtUtc: '2026-07-20T08:15:00Z',
    startTime: '10:15:00',
  },
];

describe('appointment slot flow page', () => {
  beforeEach(() => {
    window.localStorage.clear();
    vi.mocked(publicBusinessApi.getPublicBusinessProfileBySlug).mockResolvedValue(profile);
    vi.mocked(publicBusinessApi.listPublicAvailableSlots).mockResolvedValue(slots);
    vi.mocked(appointmentApi.createAppointment).mockResolvedValue({
      businessId: 'business-1',
      createdAtUtc: '2026-07-16T10:00:00Z',
      currencyCodeSnapshot: 'EUR',
      customerId: 'customer-1',
      customerNotes: 'Notas del customer',
      endAtUtc: '2026-07-20T08:45:00Z',
      id: 'appointment-1',
      priceAmountSnapshot: 25,
      serviceDurationMinutesSnapshot: 45,
      serviceId: 'service-1',
      serviceNameSnapshot: 'Corte clasico',
      staffMemberId: 'staff-1',
      startAtUtc: '2026-07-20T08:00:00Z',
      status: 'Scheduled',
    });
  });

  afterEach(() => {
    vi.clearAllMocks();
    window.localStorage.clear();
  });

  it('loads profile context and renders available slots', async () => {
    renderWithProviders(<AppointmentSlotFlowPage />);

    expect(await screen.findByRole('heading', { name: /elige un slot disponible/i })).toBeInTheDocument();
    expect(screen.getByDisplayValue('Corte clasico')).toBeInTheDocument();
    expect(await screen.findByRole('button', { name: /10:00 - 10:45/i })).toBeInTheDocument();
    expect(screen.getByText(/Timezone: Europe\/Madrid/i)).toBeInTheDocument();

    await waitFor(() => {
      expect(publicBusinessApi.listPublicAvailableSlots).toHaveBeenCalledWith('business-1', 'service-1', expect.any(String), undefined);
    });
  });

  it('filters slots by selected staff member and stores selected slot locally', async () => {
    renderWithProviders(<AppointmentSlotFlowPage />);

    expect(await screen.findByRole('button', { name: /10:00 - 10:45/i })).toBeInTheDocument();

    const user = userEvent.setup();
    await user.selectOptions(screen.getByLabelText(/staff member opcional/i), 'staff-1');

    await waitFor(() => {
      expect(publicBusinessApi.listPublicAvailableSlots).toHaveBeenLastCalledWith('business-1', 'service-1', expect.any(String), 'staff-1');
    });

    await user.click(screen.getByRole('button', { name: /10:00 - 10:45/i }));
    const confirmationHeading = screen.getByText(/Entra como customer para confirmar/i);
    const selectedSlotButton = screen.getByRole('button', { name: /10:00 - 10:45/i });

    expect(screen.getByText(/Slot seleccionado: 10:00 - 10:45/i)).toBeInTheDocument();
    expect(confirmationHeading).toBeInTheDocument();
    expect(confirmationHeading.compareDocumentPosition(selectedSlotButton) & Node.DOCUMENT_POSITION_FOLLOWING).toBeTruthy();
  });

  it('shows customer auth links with selected slot preserved', async () => {
    renderWithProviders(<AppointmentSlotFlowPage />);

    const user = userEvent.setup();
    await user.click(await screen.findByRole('button', { name: /10:00 - 10:45/i }));

    const loginLink = screen.getByRole('link', { name: /entrar y confirmar/i });
    expect(loginLink).toHaveAttribute('href', expect.stringContaining('/auth/customer/login?returnTo='));
    expect(decodeURIComponent(loginLink.getAttribute('href') ?? '')).toContain('serviceId=service-1');
    expect(decodeURIComponent(loginLink.getAttribute('href') ?? '')).toContain('staffMemberId=staff-1');
    expect(decodeURIComponent(loginLink.getAttribute('href') ?? '')).toContain('startAtUtc=2026-07-20T08%3A00%3A00Z');
  });

  it('creates appointment when a customer session confirms a selected slot', async () => {
    setAuthSession({
      accountType: 'Customer',
      email: 'customer@demo.test',
      expiresAtUtc: '2026-07-20T00:00:00Z',
      firstName: 'Clara',
      id: 'customer-1',
      lastName: null,
      token: 'customer-token',
      tokenType: 'Bearer',
    });
    renderWithProviders(<AppointmentSlotFlowPage />);

    const user = userEvent.setup();
    await user.click(await screen.findByRole('button', { name: /10:00 - 10:45/i }));
    await user.type(screen.getByLabelText(/notas para el business/i), 'Notas del customer');
    await user.click(screen.getByRole('button', { name: /confirmar appointment/i }));

    await waitFor(() => {
      expect(appointmentApi.createAppointment).toHaveBeenCalledWith({
        businessId: 'business-1',
        customerNotes: 'Notas del customer',
        serviceId: 'service-1',
        staffMemberId: 'staff-1',
        startAtUtc: '2026-07-20T08:00:00Z',
      }, 'customer-token');
    });
    expect(await screen.findByText(/Tu appointment esta scheduled/i)).toBeInTheDocument();
    expect(screen.getByRole('link', { name: /ver detalle del appointment/i })).toHaveAttribute('href', '/appointments/appointment-1');
  });

  it('lets admin view slots without confirming appointments', async () => {
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
    renderWithProviders(<AppointmentSlotFlowPage />);

    const user = userEvent.setup();
    await user.click(await screen.findByRole('button', { name: /10:00 - 10:45/i }));

    expect(screen.getByText(/no puedes confirmar como admin/i)).toBeInTheDocument();
    expect(screen.getByText(/estas navegando como admin one/i)).toBeInTheDocument();
    expect(screen.queryByRole('button', { name: /confirmar appointment/i })).not.toBeInTheDocument();
    expect(appointmentApi.createAppointment).not.toHaveBeenCalled();
  });

  it('shows outside-window state without fetching that date', async () => {
    renderWithProviders(<AppointmentSlotFlowPage />);

    expect(await screen.findByRole('button', { name: /10:00 - 10:45/i })).toBeInTheDocument();
    vi.mocked(publicBusinessApi.listPublicAvailableSlots).mockClear();

    fireEvent.change(screen.getByLabelText(/fecha/i), { target: { value: '2099-01-01' } });

    expect(await screen.findByText('Fecha fuera de ventana')).toBeInTheDocument();
    expect(publicBusinessApi.listPublicAvailableSlots).not.toHaveBeenCalled();
  });

  it('shows empty services and not found states', async () => {
    vi.mocked(publicBusinessApi.getPublicBusinessProfileBySlug).mockResolvedValueOnce({ ...profile, assignments: [], services: [], staffMembers: [] });

    const { unmount } = renderWithProviders(<AppointmentSlotFlowPage />);

    expect(await screen.findByText('Sin services disponibles')).toBeInTheDocument();
    unmount();

    vi.mocked(publicBusinessApi.getPublicBusinessProfileBySlug).mockRejectedValueOnce(
      new ApiError('Business not found.', 404, { detail: 'The business was not found or is not active.' }),
    );

    renderWithProviders(<AppointmentSlotFlowPage />);

    expect(await screen.findByRole('heading', { name: /no podemos cargar este flujo/i })).toBeInTheDocument();
  });
});

function renderWithProviders(children: ReactNode, initialEntry = '/b/barberia-centro/appointment') {
  const queryClient = new QueryClient({
    defaultOptions: {
      queries: { retry: false },
    },
  });

  return render(
    <QueryClientProvider client={queryClient}>
      <MemoryRouter initialEntries={[initialEntry]}>
        <Routes>
          <Route element={children} path="/b/:slug/appointment" />
        </Routes>
      </MemoryRouter>
    </QueryClientProvider>,
  );
}
