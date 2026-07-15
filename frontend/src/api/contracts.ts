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

export type RegisterBusinessRequest = {
  businessName: string;
  businessSlug: string;
  timeZoneId: string;
  currencyCode: string;
  adminEmail: string;
  adminPassword: string;
  adminDisplayName: string;
};

export type RegisterBusinessResponse = {
  businessId: string;
  businessSlug: string;
  adminId: string;
  adminEmail: string;
  createdAtUtc: string;
};

export type RegisterCustomerRequest = {
  email: string;
  password: string;
  firstName: string;
  lastName?: string | null;
  phoneNumber?: string | null;
};

export type RegisterCustomerResponse = {
  customerId: string;
  email: string;
  firstName: string;
  lastName?: string | null;
  phoneNumber?: string | null;
  createdAtUtc: string;
};

export type BusinessResponse = {
  id: string;
  name: string;
  slug: string;
  description?: string | null;
  contactEmail?: string | null;
  contactPhoneNumber?: string | null;
  websiteUrl?: string | null;
  addressLine1?: string | null;
  addressLine2?: string | null;
  city?: string | null;
  postalCode?: string | null;
  countryCode?: string | null;
  timeZoneId: string;
  currencyCode: string;
  maxAdvanceBookingDays: number;
};

export type UpdateBusinessDetailsRequest = {
  name: string;
  description?: string | null;
  contactEmail?: string | null;
  contactPhoneNumber?: string | null;
  websiteUrl?: string | null;
  addressLine1?: string | null;
  addressLine2?: string | null;
  city?: string | null;
  postalCode?: string | null;
  countryCode?: string | null;
  timeZoneId: string;
  currencyCode: string;
};

export type UpdateBusinessBookingWindowRequest = {
  maxAdvanceBookingDays: number;
};

export type BusinessBookingWindowResponse = {
  maxAdvanceBookingDays: number;
};

export type ServiceResponse = {
  id: string;
  businessId: string;
  name: string;
  description?: string | null;
  durationMinutes: number;
  priceAmount: number;
  isActive: boolean;
  sortOrder: number;
  createdAtUtc: string;
};

export type CreateServiceRequest = {
  name: string;
  description?: string | null;
  durationMinutes: number;
  priceAmount: number;
  sortOrder: number;
};

export type UpdateServiceRequest = CreateServiceRequest;

export type UpdateServiceActiveStateRequest = {
  isActive: boolean;
};

export type StaffMemberResponse = {
  id: string;
  businessId: string;
  displayName: string;
  email?: string | null;
  phoneNumber?: string | null;
  bio?: string | null;
  isActive: boolean;
  sortOrder: number;
  createdAtUtc: string;
};

export type CreateStaffMemberRequest = {
  displayName: string;
  email?: string | null;
  phoneNumber?: string | null;
  bio?: string | null;
  sortOrder: number;
};

export type UpdateStaffMemberRequest = CreateStaffMemberRequest;

export type UpdateStaffMemberActiveStateRequest = {
  isActive: boolean;
};

export type StaffMemberServiceAssignmentResponse = {
  staffMemberId: string;
  serviceId: string;
  isActive: boolean;
  createdAtUtc: string;
};

export type UpdateStaffMemberServiceActiveStateRequest = {
  isActive: boolean;
};

export type StaffMemberAvailabilityResponse = {
  id: string;
  staffMemberId: string;
  dayOfWeek: number;
  startTime: string;
  endTime: string;
  isActive: boolean;
  createdAtUtc: string;
};

export type CreateStaffMemberAvailabilityRequest = {
  dayOfWeek: number;
  startTime: string;
  endTime: string;
};

export type UpdateStaffMemberAvailabilityRequest = CreateStaffMemberAvailabilityRequest;

export type StaffMemberAvailabilityExceptionResponse = {
  id: string;
  staffMemberId: string;
  localDate: string;
  isClosed: boolean;
  startTime?: string | null;
  endTime?: string | null;
  reason?: string | null;
  createdAtUtc: string;
};

export type CreateStaffMemberAvailabilityExceptionRequest = {
  localDate: string;
  isClosed: boolean;
  startTime?: string | null;
  endTime?: string | null;
  reason?: string | null;
};

export type UpdateStaffMemberAvailabilityExceptionRequest = CreateStaffMemberAvailabilityExceptionRequest;

export type AvailableSlotResponse = {
  staffMemberId: string;
  localDate: string;
  localStartTime: string;
  localEndTime: string;
  startAtUtc: string;
  endAtUtc: string;
};
