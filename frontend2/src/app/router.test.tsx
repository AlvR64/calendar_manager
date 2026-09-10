import { render, screen } from '@testing-library/react';
import { RouterProvider, createMemoryRouter } from 'react-router-dom';
import { expect, test } from 'vitest';

import { RedesignLayout } from '@/layouts/RedesignLayout';
import { RedesignPlaceholderPage } from '@/pages/RedesignPlaceholderPage';

test('renders the frontend2 public shell', () => {
  const router = createMemoryRouter(
    [
      {
        element: <RedesignLayout />,
        children: [{ index: true, element: <RedesignPlaceholderPage area="Public" title="Test route" /> }],
      },
    ],
    { initialEntries: ['/'] },
  );

  render(<RouterProvider router={router} />);

  expect(screen.getByRole('heading', { name: 'Test route' })).toBeInTheDocument();
});
