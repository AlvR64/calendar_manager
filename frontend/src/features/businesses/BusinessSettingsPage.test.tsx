import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import type { ReactNode } from 'react';
import { MemoryRouter, Route, Routes } from 'react-router-dom';
import { afterEach, describe, expect, it, vi } from 'vitest';

import type { BusinessResponse } from '@/api/contracts';
import { getAuthSession, setAuthSession } from '@/auth/authStorage';
import * as businessApi from '@/features/businesses/businessApi';
import { AdminLayout } from '@/layouts/AdminLayout';
import { BusinessSettingsPage } from '@/pages/admin/BusinessSettingsPage';

vi.mock('@/features/businesses/businessApi', () => ({
  getBusinessById: vi.fn(),
  updateCurrentBusinessBookingWindow: vi.fn(),
  updateCurrentBusinessDetails: vi.fn(),
}));

const business: BusinessResponse = {
  addressLine1: 'Main street 1',
  addressLine2: null,
  category: 'beauty',
  city: 'Madrid',
  contactEmail: 'hello@studio.test',
  contactPhoneNumber: '+34123456789',
  countryCode: 'ES',
  currencyCode: 'EUR',
  description: 'A central studio.',
  id: 'business-1',
  maxAdvanceBookingDays: 30,
  name: 'Studio Centro',
  postalCode: '28001',
  slug: 'studio-centro',
  timeZoneId: 'Europe/Madrid',
  websiteUrl: 'https://studio.test',
};

describe('business settings', () => {
  afterEach(() => {
    window.localStorage.clear();
    vi.clearAllMocks();
  });

  it('loads business details and submits updates', async () => {
    setAdminSession();
    vi.mocked(businessApi.getBusinessById).mockResolvedValue(business);
    vi.mocked(businessApi.updateCurrentBusinessDetails).mockResolvedValue({ ...business, name: 'Studio Norte' });

    renderWithProviders(<BusinessSettingsPage />);

    expect(await screen.findByDisplayValue('Studio Centro')).toBeInTheDocument();
    expect(screen.getByLabelText(/categoria marketplace/i)).toHaveValue('beauty');
    expect(screen.getByText('Categoria: Estetica')).toBeInTheDocument();

    const user = userEvent.setup();
    await user.clear(screen.getByLabelText(/business name/i));
    await user.type(screen.getByLabelText(/business name/i), 'Studio Norte');
    await user.selectOptions(screen.getByLabelText(/categoria marketplace/i), 'physiotherapy');
    await user.clear(screen.getByLabelText(/country code/i));
    await user.type(screen.getByLabelText(/country code/i), 'es');
    await user.click(screen.getByRole('button', { name: /guardar details/i }));

    await waitFor(() => {
      expect(businessApi.updateCurrentBusinessDetails).toHaveBeenCalled();
    });
    expect(vi.mocked(businessApi.updateCurrentBusinessDetails).mock.calls[0]?.[0]).toEqual(
      expect.objectContaining({ category: 'physiotherapy', countryCode: 'ES', name: 'Studio Norte' }),
    );
    expect(vi.mocked(businessApi.updateCurrentBusinessDetails).mock.calls[0]?.[1]).toBe('admin-token');
  });

  it('updates the booking window', async () => {
    setAdminSession();
    vi.mocked(businessApi.getBusinessById).mockResolvedValue(business);
    vi.mocked(businessApi.updateCurrentBusinessBookingWindow).mockResolvedValue({ maxAdvanceBookingDays: 45 });

    renderWithProviders(<BusinessSettingsPage />);

    expect(await screen.findByDisplayValue('30')).toBeInTheDocument();

    const user = userEvent.setup();
    await user.clear(screen.getByLabelText(/max advance booking days/i));
    await user.type(screen.getByLabelText(/max advance booking days/i), '45');
    await user.click(screen.getByRole('button', { name: /guardar ventana/i }));

    await waitFor(() => {
      expect(businessApi.updateCurrentBusinessBookingWindow).toHaveBeenCalled();
    });
    expect(vi.mocked(businessApi.updateCurrentBusinessBookingWindow).mock.calls[0]?.[0]).toEqual({ maxAdvanceBookingDays: 45 });
    expect(vi.mocked(businessApi.updateCurrentBusinessBookingWindow).mock.calls[0]?.[1]).toBe('admin-token');
  });

  it('clears the admin session on logout', async () => {
    setAdminSession();

    render(
      <MemoryRouter initialEntries={['/admin']}>
        <Routes>
          <Route element={<AdminLayout />} path="/admin">
            <Route index element={<div>Dashboard body</div>} />
          </Route>
          <Route element={<div>Admin login route</div>} path="/auth/admin/login" />
        </Routes>
      </MemoryRouter>,
    );

    expect(screen.getByText('Admin One')).toBeInTheDocument();
    await userEvent.click(screen.getByRole('button', { name: /logout admin/i }));

    expect(await screen.findByText('Admin login route')).toBeInTheDocument();
    expect(getAuthSession('Admin')).toBeNull();
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
      queries: { retry: false },
    },
  });

  return render(<QueryClientProvider client={queryClient}>{children}</QueryClientProvider>);
}
