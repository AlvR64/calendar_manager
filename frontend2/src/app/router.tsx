import { createBrowserRouter } from 'react-router-dom';

import { ProtectedRoute } from '@/auth/ProtectedRoute';
import { AdminDashboardPage } from '@/features/dashboard/AdminDashboardPage';
import { AdminShell } from '@/layouts/AdminShell';
import { RedesignLayout } from '@/layouts/RedesignLayout';
import { RedesignPlaceholderPage } from '@/pages/RedesignPlaceholderPage';

const publicPage = (title: string) => <RedesignPlaceholderPage area="Public" title={title} />;
const customerPage = (title: string) => (
  <ProtectedRoute accountType="Customer">
    <RedesignPlaceholderPage area="Customer" title={title} />
  </ProtectedRoute>
);
export const router = createBrowserRouter([
  {
    element: <RedesignLayout />,
    children: [
      { path: '/', element: publicPage('A new way to manage time') },
      { path: '/search', element: publicPage('Find a business') },
      { path: '/b/:slug', element: publicPage('Business profile') },
      { path: '/b/:slug/appointment', element: publicPage('Choose an appointment time') },
      { path: '/appointments/:appointmentId', element: publicPage('Appointment details') },
      { path: '/auth/customer/login', element: publicPage('Customer sign in') },
      { path: '/auth/customer/register', element: publicPage('Customer registration') },
      { path: '/auth/admin/login', element: publicPage('Admin sign in') },
      { path: '/auth/business/register', element: publicPage('Business registration') },
      { path: '/customer/appointments', element: customerPage('Your appointments') },
      {
        element: <ProtectedRoute accountType="Admin"><AdminShell /></ProtectedRoute>,
        children: [
          { path: '/admin', element: <AdminDashboardPage /> },
          { path: '/admin/business-settings', element: <RedesignPlaceholderPage area="Admin" title="Business settings" /> },
          { path: '/admin/services', element: <RedesignPlaceholderPage area="Admin" title="Services" /> },
          { path: '/admin/staff-members', element: <RedesignPlaceholderPage area="Admin" title="Staff members" /> },
          { path: '/admin/availability', element: <RedesignPlaceholderPage area="Admin" title="Availability" /> },
          { path: '/admin/appointments', element: <RedesignPlaceholderPage area="Admin" title="Appointments" /> },
        ],
      },
    ],
  },
]);
