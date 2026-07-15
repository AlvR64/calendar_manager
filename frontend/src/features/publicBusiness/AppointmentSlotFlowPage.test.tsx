import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { fireEvent, render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import type { ReactNode } from 'react';
import { MemoryRouter, Route, Routes } from 'react-router-dom';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';

import type { AvailableSlotResponse, BusinessProfileResponse } from '@/api/contracts';
import { ApiError } from '@/api/httpClient';
import * as publicBusinessApi from '@/features/publicBusiness/publicBusinessApi';
import { AppointmentSlotFlowPage } from '@/pages/public/AppointmentSlotFlowPage';

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
    vi.mocked(publicBusinessApi.getPublicBusinessProfileBySlug).mockResolvedValue(profile);
    vi.mocked(publicBusinessApi.listPublicAvailableSlots).mockResolvedValue(slots);
  });

  afterEach(() => {
    vi.clearAllMocks();
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
    expect(screen.getByText(/Slot seleccionado: 10:00 - 10:45/i)).toBeInTheDocument();
    expect(screen.getByText(/No se crea appointment todavia/i)).toBeInTheDocument();
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
