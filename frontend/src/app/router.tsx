import { createBrowserRouter } from 'react-router-dom';

import { ProtectedRoute } from '@/auth/ProtectedRoute';
import { AdminLayout } from '@/layouts/AdminLayout';
import { AuthLayout } from '@/layouts/AuthLayout';
import { PublicLayout } from '@/layouts/PublicLayout';
import { AdminDashboardPage } from '@/pages/admin/AdminDashboardPage';
import { AdminAppointmentsPage } from '@/pages/admin/AdminAppointmentsPage';
import { AvailabilityPage } from '@/pages/admin/AvailabilityPage';
import { BusinessSettingsPage } from '@/pages/admin/BusinessSettingsPage';
import { ServicesPage } from '@/pages/admin/ServicesPage';
import { StaffMembersPage } from '@/pages/admin/StaffMembersPage';
import { BusinessAdminLoginPage } from '@/pages/auth/BusinessAdminLoginPage';
import { BusinessAdminRegisterPage } from '@/pages/auth/BusinessAdminRegisterPage';
import { CustomerLoginPage } from '@/pages/auth/CustomerLoginPage';
import { CustomerRegisterPage } from '@/pages/auth/CustomerRegisterPage';
import { CustomerAppointmentsPage } from '@/pages/customer/CustomerAppointmentsPage';
import { AppointmentDetailsPage } from '@/pages/public/AppointmentDetailsPage';
import { AppointmentSlotFlowPage } from '@/pages/public/AppointmentSlotFlowPage';
import { BusinessProfilePage } from '@/pages/public/BusinessProfilePage';
import { HomePage } from '@/pages/public/HomePage';

export const router = createBrowserRouter([
  {
    element: <PublicLayout />,
    children: [
      { path: '/', element: <HomePage /> },
      { path: '/appointments/:appointmentId', element: <AppointmentDetailsPage /> },
      { path: '/b/:slug', element: <BusinessProfilePage /> },
      { path: '/b/:slug/appointment', element: <AppointmentSlotFlowPage /> },
    ],
  },
  {
    element: <AuthLayout />,
    children: [
      { path: '/auth/customer/login', element: <CustomerLoginPage /> },
      { path: '/auth/customer/register', element: <CustomerRegisterPage /> },
      { path: '/auth/admin/login', element: <BusinessAdminLoginPage /> },
      { path: '/auth/business/register', element: <BusinessAdminRegisterPage /> },
    ],
  },
  {
    path: '/customer/appointments',
    element: (
      <ProtectedRoute accountType="Customer">
        <CustomerAppointmentsPage />
      </ProtectedRoute>
    ),
  },
  {
    path: '/admin',
    element: (
      <ProtectedRoute accountType="Admin">
        <AdminLayout />
      </ProtectedRoute>
    ),
    children: [
      { index: true, element: <AdminDashboardPage /> },
      { path: 'business-settings', element: <BusinessSettingsPage /> },
      { path: 'services', element: <ServicesPage /> },
      { path: 'staff-members', element: <StaffMembersPage /> },
      { path: 'availability', element: <AvailabilityPage /> },
      { path: 'appointments', element: <AdminAppointmentsPage /> },
    ],
  },
]);
