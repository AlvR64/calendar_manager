import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import type { ReactNode } from 'react';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';

import type { ServiceResponse, StaffMemberResponse } from '@/api/contracts';
import { ApiError } from '@/api/httpClient';
import { setAuthSession } from '@/auth/authStorage';
import * as serviceApi from '@/features/services/serviceApi';
import * as staffMemberServiceApi from '@/features/staffMemberServices/staffMemberServiceApi';
import * as staffMemberApi from '@/features/staffMembers/staffMemberApi';
import { ServicesPage } from '@/pages/admin/ServicesPage';

vi.mock('@/features/services/serviceApi', () => ({
  createService: vi.fn(),
  deleteService: vi.fn(),
  getService: vi.fn(),
  listServices: vi.fn(),
  updateService: vi.fn(),
  updateServiceActiveState: vi.fn(),
}));

vi.mock('@/features/staffMembers/staffMemberApi', () => ({
  createStaffMember: vi.fn(),
  deleteStaffMember: vi.fn(),
  getStaffMember: vi.fn(),
  listStaffMembers: vi.fn(),
  updateStaffMember: vi.fn(),
  updateStaffMemberActiveState: vi.fn(),
}));

vi.mock('@/features/staffMemberServices/staffMemberServiceApi', () => ({
  assignServiceToStaffMember: vi.fn(),
  assignStaffMemberToService: vi.fn(),
  listServiceStaffMemberAssignments: vi.fn(),
  listStaffMemberServiceAssignments: vi.fn(),
  unassignServiceFromStaffMember: vi.fn(),
  updateStaffMemberServiceAssignmentActiveState: vi.fn(),
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

const ana: StaffMemberResponse = {
  bio: null,
  businessId: 'business-1',
  createdAtUtc: '2026-07-15T18:00:00Z',
  displayName: 'Ana Ruiz',
  email: 'ana@example.com',
  id: 'staff-1',
  isActive: true,
  phoneNumber: null,
  sortOrder: 1,
};

const mario: StaffMemberResponse = {
  bio: null,
  businessId: 'business-1',
  createdAtUtc: '2026-07-15T18:00:00Z',
  displayName: 'Mario Lopez',
  email: null,
  id: 'staff-2',
  isActive: true,
  phoneNumber: null,
  sortOrder: 2,
};

describe('services page', () => {
  beforeEach(() => {
    vi.mocked(staffMemberApi.listStaffMembers).mockResolvedValue([ana]);
    vi.mocked(staffMemberServiceApi.listServiceStaffMemberAssignments).mockResolvedValue([]);
  });

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

  it('assigns, toggles, and unassigns staff members for a service', async () => {
    setAdminSession();
    vi.stubGlobal('confirm', vi.fn(() => true));
    vi.mocked(serviceApi.listServices).mockResolvedValue([haircut]);
    vi.mocked(staffMemberApi.listStaffMembers).mockResolvedValue([ana, mario]);
    vi.mocked(staffMemberServiceApi.listServiceStaffMemberAssignments).mockResolvedValue([
      {
        createdAtUtc: '2026-07-15T18:00:00Z',
        isActive: true,
        serviceId: 'service-1',
        staffMemberId: 'staff-1',
      },
    ]);
    vi.mocked(staffMemberServiceApi.assignStaffMemberToService).mockResolvedValue({
      createdAtUtc: '2026-07-15T18:00:00Z',
      isActive: true,
      serviceId: 'service-1',
      staffMemberId: 'staff-2',
    });
    vi.mocked(staffMemberServiceApi.updateStaffMemberServiceAssignmentActiveState).mockResolvedValue({
      createdAtUtc: '2026-07-15T18:00:00Z',
      isActive: false,
      serviceId: 'service-1',
      staffMemberId: 'staff-1',
    });
    vi.mocked(staffMemberServiceApi.unassignServiceFromStaffMember).mockResolvedValue(undefined);

    renderWithProviders(<ServicesPage />);

    expect(await screen.findByText('Ana Ruiz')).toBeInTheDocument();

    const user = userEvent.setup();
    await user.selectOptions(screen.getByRole('combobox'), 'staff-2');
    await user.click(screen.getByRole('button', { name: /asignar staff/i }));
    await waitFor(() => {
      expect(staffMemberServiceApi.assignStaffMemberToService).toHaveBeenCalledWith('service-1', 'staff-2', 'admin-token');
    });

    await user.click(screen.getByRole('button', { name: /desactivar assignment/i }));
    await waitFor(() => {
      expect(staffMemberServiceApi.updateStaffMemberServiceAssignmentActiveState).toHaveBeenCalledWith('staff-1', 'service-1', { isActive: false }, 'admin-token');
    });

    await user.click(screen.getByRole('button', { name: /desasignar/i }));
    await waitFor(() => {
      expect(staffMemberServiceApi.unassignServiceFromStaffMember).toHaveBeenCalledWith('staff-1', 'service-1', 'admin-token');
    });
  });
});

function setAdminSession() {
  setAuthSession({
    accountType: 'Admin',
    businessId: 'business-1',
    displayName: 'Admin One',
    email: 'admin@example.com',
    expiresAtUtc: '2099-07-15T18:00:00Z',
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
