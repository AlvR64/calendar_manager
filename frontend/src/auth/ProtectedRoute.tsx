import { type PropsWithChildren } from 'react';
import { Navigate, useLocation } from 'react-router-dom';

import type { AccountType } from '@/api/contracts';
import { getAuthSession } from '@/auth/authStorage';

type ProtectedRouteProps = PropsWithChildren<{
  accountType: AccountType;
}>;

export function ProtectedRoute({ accountType, children }: ProtectedRouteProps) {
  const location = useLocation();
  const session = getAuthSession(accountType);

  if (!session) {
    return <Navigate replace state={{ from: location }} to={accountType === 'Admin' ? '/auth/admin/login' : '/auth/customer/login'} />;
  }

  return children;
}
