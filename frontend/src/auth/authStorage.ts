import type { AccountType } from '@/api/contracts';

const authTokenKey = 'calendar_manager.auth_token';
const accountTypeKey = 'calendar_manager.account_type';

export type AuthSession = {
  accountType: AccountType;
  token: string;
};

export function getAuthSession(): AuthSession | null {
  const token = window.localStorage.getItem(authTokenKey);
  const accountType = window.localStorage.getItem(accountTypeKey) as AccountType | null;

  if (!token || !accountType) {
    return null;
  }

  return { accountType, token };
}

export function setAuthSession(session: AuthSession): void {
  window.localStorage.setItem(authTokenKey, session.token);
  window.localStorage.setItem(accountTypeKey, session.accountType);
}

export function clearAuthSession(): void {
  window.localStorage.removeItem(authTokenKey);
  window.localStorage.removeItem(accountTypeKey);
}
