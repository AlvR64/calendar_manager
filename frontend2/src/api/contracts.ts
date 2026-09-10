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
