import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import type { ReactNode } from 'react';
import { afterEach, describe, expect, it, vi } from 'vitest';

import type { StaffMemberResponse } from '@/api/contracts';
import { ApiError } from '@/api/httpClient';
import { setAuthSession } from '@/auth/authStorage';
import * as staffMemberApi from '@/features/staffMembers/staffMemberApi';
import { StaffMembersPage } from '@/pages/admin/StaffMembersPage';

vi.mock('@/features/staffMembers/staffMemberApi', () => ({
  createStaffMember: vi.fn(),
  deleteStaffMember: vi.fn(),
  getStaffMember: vi.fn(),
  listStaffMembers: vi.fn(),
  updateStaffMember: vi.fn(),
  updateStaffMemberActiveState: vi.fn(),
}));

const ana: StaffMemberResponse = {
  bio: 'Especialista en cortes',
  businessId: 'business-1',
  createdAtUtc: '2026-07-15T18:00:00Z',
  displayName: 'Ana Ruiz',
  email: 'ana@example.com',
  id: 'staff-1',
  isActive: true,
  phoneNumber: '+34911111111',
  sortOrder: 1,
};

const mario: StaffMemberResponse = {
  bio: null,
  businessId: 'business-1',
  createdAtUtc: '2026-07-15T18:00:00Z',
  displayName: 'Mario Lopez',
  email: null,
  id: 'staff-2',
  isActive: false,
  phoneNumber: null,
  sortOrder: 2,
};

describe('staff members page', () => {
  afterEach(() => {
    window.localStorage.clear();
    vi.clearAllMocks();
    vi.unstubAllGlobals();
  });

  it('loads active and inactive staff members', async () => {
    setAdminSession();
    vi.mocked(staffMemberApi.listStaffMembers).mockResolvedValue([mario, ana]);

    renderWithProviders(<StaffMembersPage />);

    expect(await screen.findByText('Ana Ruiz')).toBeInTheDocument();
    expect(screen.getByText('Mario Lopez')).toBeInTheDocument();
    expect(screen.getByText('Inactive')).toBeInTheDocument();
    expect(staffMemberApi.listStaffMembers).toHaveBeenCalledWith('admin-token');
  });

  it('creates a staff member', async () => {
    setAdminSession();
    vi.mocked(staffMemberApi.listStaffMembers).mockResolvedValue([]);
    vi.mocked(staffMemberApi.createStaffMember).mockResolvedValue(ana);

    renderWithProviders(<StaffMembersPage />);

    expect(await screen.findByText('Aun no hay staff members')).toBeInTheDocument();

    const user = userEvent.setup();
    await user.type(screen.getByLabelText(/display name/i), 'Ana Ruiz');
    await user.type(screen.getByLabelText(/^email$/i), 'ana@example.com');
    await user.type(screen.getByLabelText(/phone number/i), '+34911111111');
    await user.type(screen.getByLabelText(/bio/i), 'Especialista en cortes');
    await user.clear(screen.getByLabelText(/sort order/i));
    await user.type(screen.getByLabelText(/sort order/i), '1');
    await user.click(screen.getByRole('button', { name: /crear staff member/i }));

    await waitFor(() => {
      expect(staffMemberApi.createStaffMember).toHaveBeenCalled();
    });
    expect(vi.mocked(staffMemberApi.createStaffMember).mock.calls[0]?.[0]).toEqual(
      expect.objectContaining({
        bio: 'Especialista en cortes',
        displayName: 'Ana Ruiz',
        email: 'ana@example.com',
        phoneNumber: '+34911111111',
        sortOrder: 1,
      }),
    );
    expect(vi.mocked(staffMemberApi.createStaffMember).mock.calls[0]?.[1]).toBe('admin-token');
  });

  it('edits and deactivates a staff member', async () => {
    setAdminSession();
    vi.mocked(staffMemberApi.listStaffMembers).mockResolvedValue([ana]);
    vi.mocked(staffMemberApi.updateStaffMember).mockResolvedValue({ ...ana, displayName: 'Ana Premium' });
    vi.mocked(staffMemberApi.updateStaffMemberActiveState).mockResolvedValue({ ...ana, isActive: false });

    renderWithProviders(<StaffMembersPage />);

    expect(await screen.findByText('Ana Ruiz')).toBeInTheDocument();

    const user = userEvent.setup();
    await user.click(screen.getByRole('button', { name: /editar/i }));
    await user.clear(screen.getByLabelText(/display name/i));
    await user.type(screen.getByLabelText(/display name/i), 'Ana Premium');
    await user.click(screen.getByRole('button', { name: /guardar cambios/i }));

    await waitFor(() => {
      expect(staffMemberApi.updateStaffMember).toHaveBeenCalled();
    });
    expect(vi.mocked(staffMemberApi.updateStaffMember).mock.calls[0]?.[0]).toBe('staff-1');
    expect(vi.mocked(staffMemberApi.updateStaffMember).mock.calls[0]?.[1]).toEqual(expect.objectContaining({ displayName: 'Ana Premium' }));

    await user.click(screen.getByRole('button', { name: /desactivar/i }));
    await waitFor(() => {
      expect(staffMemberApi.updateStaffMemberActiveState).toHaveBeenCalledWith('staff-1', { isActive: false }, 'admin-token');
    });
  });

  it('shows delete conflicts', async () => {
    setAdminSession();
    vi.stubGlobal('confirm', vi.fn(() => true));
    vi.mocked(staffMemberApi.listStaffMembers).mockResolvedValue([ana]);
    vi.mocked(staffMemberApi.deleteStaffMember).mockRejectedValue(
      new ApiError('Conflict', 409, { detail: 'The staff member cannot be deleted because it has appointments.' }),
    );

    renderWithProviders(<StaffMembersPage />);

    expect(await screen.findByText('Ana Ruiz')).toBeInTheDocument();

    await userEvent.click(screen.getByRole('button', { name: /eliminar/i }));

    expect(await screen.findByRole('alert')).toHaveTextContent('The staff member cannot be deleted because it has appointments.');
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
