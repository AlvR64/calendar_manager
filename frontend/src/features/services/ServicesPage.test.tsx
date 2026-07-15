import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import type { ReactNode } from 'react';
import { afterEach, describe, expect, it, vi } from 'vitest';

import type { ServiceResponse } from '@/api/contracts';
import { ApiError } from '@/api/httpClient';
import { setAuthSession } from '@/auth/authStorage';
import * as serviceApi from '@/features/services/serviceApi';
import { ServicesPage } from '@/pages/admin/ServicesPage';

vi.mock('@/features/services/serviceApi', () => ({
  createService: vi.fn(),
  deleteService: vi.fn(),
  getService: vi.fn(),
  listServices: vi.fn(),
  updateService: vi.fn(),
  updateServiceActiveState: vi.fn(),
}));

const haircut: ServiceResponse = {
  businessId: 'business-1',
  createdAtUtc: '2026-07-15T18:00:00Z',
  description: 'Corte completo',
  durationMinutes: 45,
  id: 'service-1',
  isActive: true,
  name: 'Haircut',
  priceAmount: 30,
  sortOrder: 1,
};

const color: ServiceResponse = {
  businessId: 'business-1',
  createdAtUtc: '2026-07-15T18:00:00Z',
  description: null,
  durationMinutes: 90,
  id: 'service-2',
  isActive: false,
  name: 'Color',
  priceAmount: 75,
  sortOrder: 2,
};

describe('services page', () => {
  afterEach(() => {
    window.localStorage.clear();
    vi.clearAllMocks();
    vi.unstubAllGlobals();
  });

  it('loads active and inactive services', async () => {
    setAdminSession();
    vi.mocked(serviceApi.listServices).mockResolvedValue([color, haircut]);

    renderWithProviders(<ServicesPage />);

    expect(await screen.findByText('Haircut')).toBeInTheDocument();
    expect(screen.getByText('Color')).toBeInTheDocument();
    expect(screen.getByText('Inactive')).toBeInTheDocument();
    expect(serviceApi.listServices).toHaveBeenCalledWith('admin-token');
  });

  it('creates a service', async () => {
    setAdminSession();
    vi.mocked(serviceApi.listServices).mockResolvedValue([]);
    vi.mocked(serviceApi.createService).mockResolvedValue(haircut);

    renderWithProviders(<ServicesPage />);

    expect(await screen.findByText('Aun no hay services')).toBeInTheDocument();

    const user = userEvent.setup();
    await user.type(screen.getByLabelText(/service name/i), 'Haircut');
    await user.clear(screen.getByLabelText(/duration/i));
    await user.type(screen.getByLabelText(/duration/i), '45');
    await user.clear(screen.getByLabelText(/price/i));
    await user.type(screen.getByLabelText(/price/i), '30');
    await user.clear(screen.getByLabelText(/sort order/i));
    await user.type(screen.getByLabelText(/sort order/i), '1');
    await user.click(screen.getByRole('button', { name: /crear service/i }));

    await waitFor(() => {
      expect(serviceApi.createService).toHaveBeenCalled();
    });
    expect(vi.mocked(serviceApi.createService).mock.calls[0]?.[0]).toEqual(
      expect.objectContaining({ durationMinutes: 45, name: 'Haircut', priceAmount: 30, sortOrder: 1 }),
    );
    expect(vi.mocked(serviceApi.createService).mock.calls[0]?.[1]).toBe('admin-token');
  });

  it('edits and deactivates a service', async () => {
    setAdminSession();
    vi.mocked(serviceApi.listServices).mockResolvedValue([haircut]);
    vi.mocked(serviceApi.updateService).mockResolvedValue({ ...haircut, name: 'Premium haircut' });
    vi.mocked(serviceApi.updateServiceActiveState).mockResolvedValue({ ...haircut, isActive: false });

    renderWithProviders(<ServicesPage />);

    expect(await screen.findByText('Haircut')).toBeInTheDocument();

    const user = userEvent.setup();
    await user.click(screen.getByRole('button', { name: /editar/i }));
    await user.clear(screen.getByLabelText(/service name/i));
    await user.type(screen.getByLabelText(/service name/i), 'Premium haircut');
    await user.click(screen.getByRole('button', { name: /guardar cambios/i }));

    await waitFor(() => {
      expect(serviceApi.updateService).toHaveBeenCalled();
    });
    expect(vi.mocked(serviceApi.updateService).mock.calls[0]?.[0]).toBe('service-1');
    expect(vi.mocked(serviceApi.updateService).mock.calls[0]?.[1]).toEqual(expect.objectContaining({ name: 'Premium haircut' }));

    await user.click(screen.getByRole('button', { name: /desactivar/i }));
    await waitFor(() => {
      expect(serviceApi.updateServiceActiveState).toHaveBeenCalledWith('service-1', { isActive: false }, 'admin-token');
    });
  });

  it('shows delete conflicts', async () => {
    setAdminSession();
    vi.stubGlobal('confirm', vi.fn(() => true));
    vi.mocked(serviceApi.listServices).mockResolvedValue([haircut]);
    vi.mocked(serviceApi.deleteService).mockRejectedValue(
      new ApiError('Conflict', 409, { detail: 'The service cannot be deleted because it has appointments.' }),
    );

    renderWithProviders(<ServicesPage />);

    expect(await screen.findByText('Haircut')).toBeInTheDocument();

    await userEvent.click(screen.getByRole('button', { name: /eliminar/i }));

    expect(await screen.findByRole('alert')).toHaveTextContent('The service cannot be deleted because it has appointments.');
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
