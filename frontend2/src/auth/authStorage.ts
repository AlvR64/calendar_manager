import type { AccountType } from '@/api/contracts';

export type AuthSession = {
  accessToken: string;
  accountType: AccountType;
  expiresAtUtc: string;
};

const storageKey = 'calendar-manager-frontend2-auth';

export function getAuthSession(accountType: AccountType): AuthSession | null {
  const serialized = localStorage.getItem(storageKey);

  if (!serialized) {
    return null;
  }

  try {
    const session = JSON.parse(serialized) as AuthSession;
    return session.accountType === accountType ? session : null;
  } catch {
    localStorage.removeItem(storageKey);
    return null;
  }
}

export function setAuthSession(session: AuthSession): void {
  localStorage.setItem(storageKey, JSON.stringify(session));
}

export function clearAuthSession(): void {
  localStorage.removeItem(storageKey);
}
