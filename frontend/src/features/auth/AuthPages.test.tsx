import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import type { ReactNode } from 'react';
import { MemoryRouter, Route, Routes } from 'react-router-dom';
import { afterEach, describe, expect, it, vi } from 'vitest';

import { ApiError } from '@/api/httpClient';
import { getAuthSession } from '@/auth/authStorage';
import * as authApi from '@/features/auth/authApi';
import { BusinessAdminLoginPage } from '@/pages/auth/BusinessAdminLoginPage';
import { BusinessAdminRegisterPage } from '@/pages/auth/BusinessAdminRegisterPage';
import { CustomerLoginPage } from '@/pages/auth/CustomerLoginPage';
import { CustomerRegisterPage } from '@/pages/auth/CustomerRegisterPage';

vi.mock('@/features/auth/authApi', () => ({
  loginAdmin: vi.fn(),
  loginCustomer: vi.fn(),
  registerBusiness: vi.fn(),
  registerCustomer: vi.fn(),
}));

describe('auth pages', () => {
  afterEach(() => {
    window.localStorage.clear();
    vi.clearAllMocks();
  });

  it('stores an admin session and navigates to admin after login', async () => {
    vi.mocked(authApi.loginAdmin).mockResolvedValue({
      accessToken: 'admin-token',
      expiresAtUtc: '2026-07-15T18:00:00Z',
      tokenType: 'Bearer',
      user: {
        businessId: 'business-1',
        displayName: 'Admin One',
        email: 'admin@example.com',
        id: 'admin-1',
        type: 'Admin',
      },
    });

    renderWithProviders(
      <Routes>
        <Route element={<BusinessAdminLoginPage />} path="/auth/admin/login" />
        <Route element={<div>Admin area</div>} path="/admin" />
      </Routes>,
      '/auth/admin/login',
    );

    const user = userEvent.setup();
    await user.type(screen.getByLabelText(/email admin/i), 'admin@example.com');
    await user.type(screen.getByLabelText(/password/i), 'password123');
    await user.click(screen.getByRole('button', { name: /entrar al panel/i }));

    expect(await screen.findByText('Admin area')).toBeInTheDocument();
    expect(getAuthSession('Admin')).toMatchObject({
      accountType: 'Admin',
      businessId: 'business-1',
      token: 'admin-token',
    });
    expect(getAuthSession('Customer')).toBeNull();
  });

  it('shows backend auth errors on customer login', async () => {
    vi.mocked(authApi.loginCustomer).mockRejectedValue(
      new ApiError('Unauthorized', 401, { detail: 'Invalid customer credentials.' }),
    );

    renderWithProviders(
      <Routes>
        <Route element={<CustomerLoginPage />} path="/auth/customer/login" />
      </Routes>,
      '/auth/customer/login',
    );

    const user = userEvent.setup();
    await user.type(screen.getByLabelText(/^email$/i), 'customer@example.com');
    await user.type(screen.getByLabelText(/password/i), 'password123');
    await user.click(screen.getByRole('button', { name: /entrar como customer/i }));

    expect(await screen.findByRole('alert')).toHaveTextContent('Invalid customer credentials.');
  });

  it('validates required customer registration fields', async () => {
    renderWithProviders(
      <Routes>
        <Route element={<CustomerRegisterPage />} path="/auth/customer/register" />
      </Routes>,
      '/auth/customer/register',
    );

    await userEvent.click(screen.getByRole('button', { name: /crear cuenta customer/i }));

    expect(await screen.findByText('Introduce tu nombre.')).toBeInTheDocument();
    expect(screen.getByText('Introduce un email valido.')).toBeInTheDocument();
    expect(authApi.registerCustomer).not.toHaveBeenCalled();
  });

  it('routes business registration success to admin login', async () => {
    vi.mocked(authApi.registerBusiness).mockResolvedValue({
      adminEmail: 'admin@example.com',
      adminId: 'admin-1',
      businessId: 'business-1',
      businessSlug: 'studio-centro',
      createdAtUtc: '2026-07-15T18:00:00Z',
    });

    renderWithProviders(
      <Routes>
        <Route element={<BusinessAdminRegisterPage />} path="/auth/business/register" />
        <Route element={<BusinessAdminLoginPage />} path="/auth/admin/login" />
      </Routes>,
      '/auth/business/register',
    );

    const user = userEvent.setup();
    await user.type(screen.getByLabelText(/business name/i), 'Studio Centro');
    await user.type(screen.getByLabelText(/public slug/i), 'studio-centro');
    await user.clear(screen.getByLabelText(/timezone/i));
    await user.type(screen.getByLabelText(/timezone/i), 'Europe/Madrid');
    await user.clear(screen.getByLabelText(/currency/i));
    await user.type(screen.getByLabelText(/currency/i), 'eur');
    await user.type(screen.getByLabelText(/admin display name/i), 'Admin One');
    await user.type(screen.getByLabelText(/admin email/i), 'admin@example.com');
    await user.type(screen.getByLabelText(/admin password/i), 'password123');
    await user.click(screen.getByRole('button', { name: /crear business/i }));

    await waitFor(() => {
      expect(authApi.registerBusiness).toHaveBeenCalled();
    });
    expect(vi.mocked(authApi.registerBusiness).mock.calls[0]?.[0]).toEqual(
      expect.objectContaining({ businessSlug: 'studio-centro', currencyCode: 'EUR' }),
    );
    expect(await screen.findByText('Business registrado. Entra con el admin que acabas de crear.')).toBeInTheDocument();
  });
});

function renderWithProviders(children: ReactNode, initialEntry: string) {
  const queryClient = new QueryClient({
    defaultOptions: {
      queries: { retry: false },
    },
  });

  return render(
    <QueryClientProvider client={queryClient}>
      <MemoryRouter initialEntries={[initialEntry]}>{children}</MemoryRouter>
    </QueryClientProvider>,
  );
}
