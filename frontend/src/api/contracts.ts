export type AccountType = 'Admin' | 'Customer';

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

export type AvailableSlotResponse = {
  staffMemberId: string;
  localDate: string;
  localStartTime: string;
  localEndTime: string;
  startAtUtc: string;
  endAtUtc: string;
};
