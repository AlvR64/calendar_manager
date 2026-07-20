import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import type { ReactNode } from 'react';
import { MemoryRouter, Route, Routes, useLocation } from 'react-router-dom';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';

import type { PublicBusinessSearchResponse } from '@/api/contracts';
import * as publicBusinessApi from '@/features/publicBusiness/publicBusinessApi';
import { MarketplaceSearchPage } from '@/pages/public/MarketplaceSearchPage';

vi.mock('@/features/publicBusiness/publicBusinessApi', () => ({
  searchPublicBusinesses: vi.fn(),
}));

const searchResponse: PublicBusinessSearchResponse = {
  hasNextPage: true,
  items: [
    {
      category: 'barber',
      city: 'Madrid',
      countryCode: 'ES',
      currencyCode: 'EUR',
      description: 'Cortes modernos y barba cuidada.',
      featuredServices: [
        { durationMinutes: 45, id: 'service-1', name: 'Corte clasico', priceAmount: 25 },
        { durationMinutes: 30, id: 'service-2', name: 'Barba premium', priceAmount: 18 },
      ],
      id: 'business-1',
      name: 'Barberia Centro',
      slug: 'barberia-centro',
      startingPriceAmount: 18,
      timeZoneId: 'Europe/Madrid',
    },
  ],
  page: 2,
  pageSize: 9,
  totalCount: 12,
};

describe('marketplace search page', () => {
  beforeEach(() => {
    vi.mocked(publicBusinessApi.searchPublicBusinesses).mockResolvedValue(searchResponse);
  });

  afterEach(() => {
    vi.clearAllMocks();
  });

  it('loads results from URL filters and renders marketplace cards', async () => {
    renderWithProviders(<MarketplaceSearchPage />, '/search?query=barber&city=Madrid&category=barber&service=Corte&page=2');

    expect(await screen.findByRole('heading', { name: 'Barberia Centro' })).toBeInTheDocument();
    expect(publicBusinessApi.searchPublicBusinesses).toHaveBeenCalledWith({
      category: 'barber',
      city: 'Madrid',
      page: 2,
      pageSize: 9,
      query: 'barber',
      service: 'Corte',
    });
    expect(screen.getByText('Corte clasico')).toBeInTheDocument();
    expect(screen.getAllByText('Barberia')).not.toHaveLength(0);
    expect(screen.getByText('Categoria: Barberia')).toBeInTheDocument();
    expect(screen.getByText(/desde 18/i)).toBeInTheDocument();
    expect(screen.getByRole('link', { name: /ver perfil/i })).toHaveAttribute('href', '/b/barberia-centro');
    expect(screen.getByRole('link', { name: /reservar appointment/i })).toHaveAttribute('href', '/b/barberia-centro/appointment');
  });

  it('updates the page query param with next pagination', async () => {
    renderWithProviders(
      <>
        <MarketplaceSearchPage />
        <LocationView />
      </>,
      '/search?query=barber&page=2',
    );

    expect(await screen.findByRole('heading', { name: 'Barberia Centro' })).toBeInTheDocument();
    await userEvent.click(screen.getByRole('link', { name: /siguiente/i }));

    expect(await screen.findByTestId('location')).toHaveTextContent('/search?query=barber&page=3');
    await waitFor(() => {
      expect(publicBusinessApi.searchPublicBusinesses).toHaveBeenCalledWith(expect.objectContaining({ page: 3, query: 'barber' }));
    });
  });

  it('shows an empty state when no businesses match', async () => {
    vi.mocked(publicBusinessApi.searchPublicBusinesses).mockResolvedValue({
      hasNextPage: false,
      items: [],
      page: 1,
      pageSize: 9,
      totalCount: 0,
    });

    renderWithProviders(<MarketplaceSearchPage />, '/search?city=Valencia');

    expect(await screen.findByRole('heading', { name: /no encontramos businesses/i })).toBeInTheDocument();
    expect(screen.getByRole('link', { name: /ver todos/i })).toHaveAttribute('href', '/search');
  });

  it('shows an API error state', async () => {
    vi.mocked(publicBusinessApi.searchPublicBusinesses).mockRejectedValue(new Error('Network error'));

    renderWithProviders(<MarketplaceSearchPage />);

    expect(await screen.findByRole('alert')).toHaveTextContent('No se pudo completar la operacion. Intentalo de nuevo.');
  });
});

function renderWithProviders(children: ReactNode, initialEntry = '/search') {
  const queryClient = new QueryClient({
    defaultOptions: {
      queries: { retry: false },
    },
  });

  return render(
    <QueryClientProvider client={queryClient}>
      <MemoryRouter initialEntries={[initialEntry]}>
        <Routes>
          <Route element={children} path="/search" />
        </Routes>
      </MemoryRouter>
    </QueryClientProvider>,
  );
}

function LocationView() {
  const location = useLocation();
  return <div data-testid="location">{location.pathname}{location.search}</div>;
}
