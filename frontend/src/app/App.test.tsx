import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { render, screen } from '@testing-library/react';
import { createMemoryRouter, RouterProvider } from 'react-router-dom';
import { afterEach, describe, expect, it } from 'vitest';

import { setAuthSession } from '@/auth/authStorage';
import { PublicLayout } from '@/layouts/PublicLayout';
import { HomePage } from '@/pages/public/HomePage';

describe('frontend shell', () => {
  afterEach(() => {
    window.localStorage.clear();
  });

  it('renders the public home route', () => {
    const queryClient = new QueryClient();
    const router = createMemoryRouter([
      {
        element: <PublicLayout />,
        children: [{ path: '/', element: <HomePage /> }],
      },
    ]);

    render(
      <QueryClientProvider client={queryClient}>
        <RouterProvider router={router} />
      </QueryClientProvider>,
    );

    expect(screen.getByRole('heading', { name: /reserva services/i })).toBeInTheDocument();
    expect(screen.getByRole('link', { name: /ver perfil demo/i })).toHaveAttribute('href', '/b/demo-barber');
  });

  it('shows customer session actions on the public home when logged in', () => {
    setAuthSession({
      accountType: 'Customer',
      email: 'customer@demo.calendar.test',
      expiresAtUtc: '2026-07-20T00:00:00Z',
      firstName: 'Clara',
      id: 'customer-1',
      lastName: 'Diaz',
      token: 'customer-token',
      tokenType: 'Bearer',
    });

    const queryClient = new QueryClient();
    const router = createMemoryRouter([
      {
        element: <PublicLayout />,
        children: [{ path: '/', element: <HomePage /> }],
      },
    ]);

    render(
      <QueryClientProvider client={queryClient}>
        <RouterProvider router={router} />
      </QueryClientProvider>,
    );

    expect(screen.getByText('Clara Diaz')).toBeInTheDocument();
    expect(screen.getByText('customer@demo.calendar.test')).toBeInTheDocument();
    expect(screen.getAllByRole('link', { name: /mis appointments/i })[0]).toHaveAttribute('href', '/customer/appointments');
    expect(screen.queryByRole('link', { name: /customer login/i })).not.toBeInTheDocument();
    expect(screen.queryByRole('link', { name: /para negocios/i })).not.toBeInTheDocument();
    expect(screen.queryByRole('link', { name: /publicar mi business/i })).not.toBeInTheDocument();
  });
});
