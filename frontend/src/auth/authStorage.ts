import type { AccountType } from '@/api/contracts';

const sessionKeyByAccountType = {
  Admin: 'calendar_manager.admin_session',
  Customer: 'calendar_manager.customer_session',
} satisfies Record<AccountType, string>;

type BaseAuthSession = {
  expiresAtUtc: string;
  token: string;
  tokenType: string;
};

export type AdminAuthSession = BaseAuthSession & {
  accountType: 'Admin';
  businessId: string;
  displayName: string;
  email: string;
  id: string;
};

export type CustomerAuthSession = BaseAuthSession & {
  accountType: 'Customer';
  email: string;
  firstName: string;
  id: string;
  lastName?: string | null;
};

export type AuthSession = AdminAuthSession | CustomerAuthSession;

export function getAuthSession(accountType: 'Admin'): AdminAuthSession | null;
export function getAuthSession(accountType: 'Customer'): CustomerAuthSession | null;
export function getAuthSession(accountType: AccountType): AuthSession | null;
export function getAuthSession(): AuthSession | null;
export function getAuthSession(accountType?: AccountType): AuthSession | null {
  if (accountType) {
    return readSession(accountType);
  }

  return readSession('Admin') ?? readSession('Customer');
}

export function setAuthSession(session: AuthSession): void {
  window.localStorage.setItem(sessionKeyByAccountType[session.accountType], JSON.stringify(session));
}

export function clearAuthSession(accountType?: AccountType): void {
  if (accountType) {
    window.localStorage.removeItem(sessionKeyByAccountType[accountType]);
    return;
  }

  window.localStorage.removeItem(sessionKeyByAccountType.Admin);
  window.localStorage.removeItem(sessionKeyByAccountType.Customer);
}

function readSession(accountType: AccountType): AuthSession | null {
  const value = window.localStorage.getItem(sessionKeyByAccountType[accountType]);

  if (!value) {
    return null;
  }

  try {
    const session = JSON.parse(value) as AuthSession;
    return session.accountType === accountType && session.token ? session : null;
  } catch {
    window.localStorage.removeItem(sessionKeyByAccountType[accountType]);
    return null;
  }
}
