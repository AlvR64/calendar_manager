export type AccountType = 'Admin' | 'Customer';

export type LoginAdminRequest = {
  email: string;
  password: string;
};

export type LoginCustomerRequest = {
  email: string;
  password: string;
};

export type LoginAdminResponse = {
  accessToken: string;
  tokenType: string;
  expiresAtUtc: string;
  user: {
    type: 'Admin';
    id: string;
    businessId: string;
    email: string;
    displayName: string;
  };
};

export type LoginCustomerResponse = {
  accessToken: string;
  tokenType: string;
  expiresAtUtc: string;
  user: {
    type: 'Customer';
    id: string;
    email: string;
    firstName: string;
    lastName?: string | null;
  };
};

export type AppointmentBusinessResponse = {
  id: string;
  name: string;
  slug: string;
  timeZoneId: string;
};

export type AppointmentCustomerResponse = {
  email: string;
  firstName: string;
  id: string;
  lastName?: string | null;
};

export type AppointmentServiceResponse = {
  currencyCodeSnapshot: string;
  durationMinutesSnapshot: number;
  id: string;
  nameSnapshot: string;
  priceAmountSnapshot: number;
};

export type AppointmentStaffMemberResponse = {
  displayName: string;
  id: string;
};

export type AppointmentSummaryResponse = {
  business: AppointmentBusinessResponse;
  cancelledAtUtc?: string | null;
  cancellationReason?: string | null;
  createdAtUtc: string;
  customer: AppointmentCustomerResponse;
  customerNotes?: string | null;
  endAtUtc: string;
  endTime: string;
  id: string;
  internalNotes?: string | null;
  localDate: string;
  service: AppointmentServiceResponse;
  staffMember: AppointmentStaffMemberResponse;
  startAtUtc: string;
  startTime: string;
  status: string;
};

export type AdminDashboardStatusCountResponse = {
  count: number;
  status: string;
};

export type AdminDashboardSummaryResponse = {
  currencyCode: string;
  estimatedRevenueAmount: number;
  rangeEndLocalDate: string;
  rangeStartLocalDate: string;
  statusCounts: AdminDashboardStatusCountResponse[];
  todayAppointmentCount: number;
  upcomingAppointments: AppointmentSummaryResponse[];
};
