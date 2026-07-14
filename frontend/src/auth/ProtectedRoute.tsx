import { type PropsWithChildren } from 'react';
import { Navigate } from 'react-router-dom';

import type { AccountType } from '@/api/contracts';
import { getAuthSession } from '@/auth/authStorage';

type ProtectedRouteProps = PropsWithChildren<{
  accountType: AccountType;
}>;

export function ProtectedRoute({ accountType, children }: ProtectedRouteProps) {
  const session = getAuthSession();

  if (!session || session.accountType !== accountType) {
    return <Navigate replace to={accountType === 'Admin' ? '/auth/admin/login' : '/auth/customer/login'} />;
  }

  return children;
}
