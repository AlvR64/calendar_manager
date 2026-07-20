import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { render, screen } from '@testing-library/react';
import type { ReactNode } from 'react';
import { MemoryRouter, Route, Routes } from 'react-router-dom';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';

import type { BusinessProfileResponse } from '@/api/contracts';
import { ApiError } from '@/api/httpClient';
import * as publicBusinessApi from '@/features/publicBusiness/publicBusinessApi';
import { BusinessProfilePage } from '@/pages/public/BusinessProfilePage';

vi.mock('@/features/publicBusiness/publicBusinessApi', () => ({
  getPublicBusinessById: vi.fn(),
  getPublicBusinessProfileById: vi.fn(),
  getPublicBusinessProfileBySlug: vi.fn(),
  getPublicBusinessService: vi.fn(),
  getPublicBusinessStaffMember: vi.fn(),
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
      category: 'barber',
      city: 'Madrid',
    contactEmail: 'hola@barberia.test',
    contactPhoneNumber: '+34910000000',
    countryCode: 'ES',
    currencyCode: 'EUR',
    description: 'Cortes modernos y barba cuidada en el centro.',
    id: 'business-1',
    maxAdvanceBookingDays: 60,
    name: 'Barberia Centro',
    postalCode: '28013',
    slug: 'barberia-centro',
    timeZoneId: 'Europe/Madrid',
    websiteUrl: 'https://barberia.test',
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

describe('business profile page', () => {
  beforeEach(() => {
    vi.mocked(publicBusinessApi.getPublicBusinessProfileBySlug).mockResolvedValue(profile);
  });

  afterEach(() => {
    vi.clearAllMocks();
  });

  it('loads a public business profile by slug', async () => {
    renderWithProviders(<BusinessProfilePage />);

    expect(await screen.findByRole('heading', { name: 'Barberia Centro' })).toBeInTheDocument();
    expect(screen.getByText('Corte clasico')).toBeInTheDocument();
    expect(screen.getByText('Ana Ruiz')).toBeInTheDocument();
    expect(screen.getAllByText('Barberia')).not.toHaveLength(0);
    expect(screen.getByText(/Staff: Ana Ruiz/i)).toBeInTheDocument();
    expect(screen.getByText('Europe/Madrid')).toBeInTheDocument();
    expect(screen.getByRole('link', { name: /elegir appointment/i })).toHaveAttribute('href', '/b/barberia-centro/appointment');
    expect(screen.getAllByRole('link', { name: /ver slots/i })).toHaveLength(2);
    expect(publicBusinessApi.getPublicBusinessProfileBySlug).toHaveBeenCalledWith('barberia-centro');
  });

  it('shows empty states for services and staff members', async () => {
    vi.mocked(publicBusinessApi.getPublicBusinessProfileBySlug).mockResolvedValue({
      ...profile,
      assignments: [],
      services: [],
      staffMembers: [],
    });

    renderWithProviders(<BusinessProfilePage />);

    expect(await screen.findByText('Sin services disponibles')).toBeInTheDocument();
    expect(screen.getByText('Sin staff members')).toBeInTheDocument();
  });

  it('shows a not found state for missing businesses', async () => {
    vi.mocked(publicBusinessApi.getPublicBusinessProfileBySlug).mockRejectedValue(
      new ApiError('Business not found.', 404, { detail: 'The business was not found or is not active.' }),
    );

    renderWithProviders(<BusinessProfilePage />);

    expect(await screen.findByRole('heading', { name: /no encontramos este business/i })).toBeInTheDocument();
    expect(screen.getByRole('link', { name: /volver al inicio/i })).toHaveAttribute('href', '/');
  });
});

function renderWithProviders(children: ReactNode, initialEntry = '/b/barberia-centro') {
  const queryClient = new QueryClient({
    defaultOptions: {
      queries: { retry: false },
    },
  });

  return render(
    <QueryClientProvider client={queryClient}>
      <MemoryRouter initialEntries={[initialEntry]}>
        <Routes>
          <Route element={children} path="/b/:slug" />
        </Routes>
      </MemoryRouter>
    </QueryClientProvider>,
  );
}
