import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter, Route, Routes, useLocation } from 'react-router-dom';
import { afterEach, describe, expect, it } from 'vitest';

import { HomePage } from '@/pages/public/HomePage';

describe('home page marketplace search', () => {
  afterEach(() => {
    window.localStorage.clear();
  });

  it('navigates to search with shared query params', async () => {
    render(
      <MemoryRouter initialEntries={['/']}>
        <Routes>
          <Route element={<HomePage />} path="/" />
          <Route element={<LocationView />} path="/search" />
        </Routes>
      </MemoryRouter>,
    );

    const user = userEvent.setup();
    await user.type(screen.getByLabelText(/que necesitas/i), 'fisio');
    await user.type(screen.getByLabelText(/ciudad/i), 'Madrid');
    await user.selectOptions(screen.getByLabelText(/categoria/i), 'Fisioterapia');
    await user.click(screen.getByRole('button', { name: /^buscar$/i }));

    expect(await screen.findByTestId('location')).toHaveTextContent('/search?query=fisio&city=Madrid&category=Fisioterapia');
  });
});

function LocationView() {
  const location = useLocation();
  return <div data-testid="location">{location.pathname}{location.search}</div>;
}
