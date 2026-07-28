import { afterEach, describe, expect, it } from 'vitest';

import { getAuthSession, setAuthSession } from '@/auth/authStorage';

describe('auth storage', () => {
  afterEach(() => {
    window.localStorage.clear();
  });

  it('returns and keeps valid sessions', () => {
    setAuthSession({
      accountType: 'Customer',
      email: 'customer@example.test',
      expiresAtUtc: '2099-07-20T00:00:00Z',
      firstName: 'Clara',
      id: 'customer-1',
      lastName: null,
      token: 'customer-token',
      tokenType: 'Bearer',
    });

    expect(getAuthSession('Customer')).toMatchObject({ accountType: 'Customer', token: 'customer-token' });
  });

  it('clears expired sessions when reading them', () => {
    setAuthSession({
      accountType: 'Admin',
      businessId: 'business-1',
      displayName: 'Admin One',
      email: 'admin@example.test',
      expiresAtUtc: '2000-01-01T00:00:00Z',
      id: 'admin-1',
      token: 'admin-token',
      tokenType: 'Bearer',
    });

    expect(getAuthSession('Admin')).toBeNull();
    expect(window.localStorage.getItem('calendar_manager.admin_session')).toBeNull();
  });

  it('clears sessions with invalid expiry dates when reading them', () => {
    window.localStorage.setItem('calendar_manager.customer_session', JSON.stringify({
      accountType: 'Customer',
      email: 'customer@example.test',
      expiresAtUtc: 'not-a-date',
      firstName: 'Clara',
      id: 'customer-1',
      token: 'customer-token',
      tokenType: 'Bearer',
    }));

    expect(getAuthSession('Customer')).toBeNull();
    expect(window.localStorage.getItem('calendar_manager.customer_session')).toBeNull();
  });
});
