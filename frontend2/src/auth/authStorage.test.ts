import { afterEach, expect, test } from 'vitest';

import { clearAuthSession, getAuthSession, setAuthSession } from '@/auth/authStorage';

afterEach(() => {
  clearAuthSession();
});

test('keeps Admin and Customer sessions separate', () => {
  setAuthSession({ accessToken: 'token', accountType: 'Admin', expiresAtUtc: '2030-01-01T00:00:00Z' });

  expect(getAuthSession('Admin')?.accessToken).toBe('token');
  expect(getAuthSession('Customer')).toBeNull();
});
