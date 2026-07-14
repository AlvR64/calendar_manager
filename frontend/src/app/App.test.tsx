import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { render, screen } from '@testing-library/react';
import { createMemoryRouter, RouterProvider } from 'react-router-dom';
import { describe, expect, it } from 'vitest';

import { PublicLayout } from '@/layouts/PublicLayout';
import { HomePage } from '@/pages/public/HomePage';

describe('frontend shell', () => {
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
  });
});
