import { Link } from 'react-router-dom';

import type { CustomerAuthSession } from '@/auth/authStorage';
import { routes } from '@/lib/routes';

export function CustomerSessionBadge({ session }: { session: CustomerAuthSession }) {
  const displayName = [session.firstName, session.lastName].filter(Boolean).join(' ') || session.email;

  return (
    <div className="inline-flex items-center gap-3 rounded-2xl border border-indigo-100 bg-white/90 px-4 py-3 text-sm shadow-sm backdrop-blur">
      <div className="flex h-9 w-9 items-center justify-center rounded-full bg-indigo-600 text-sm font-black uppercase text-white">
        {displayName.slice(0, 1)}
      </div>
      <div className="min-w-0">
        <p className="truncate font-black text-slate-950">{displayName}</p>
        <p className="truncate text-xs font-bold text-slate-500">{session.email}</p>
      </div>
      <Link className="hidden rounded-full bg-slate-950 px-3 py-2 text-xs font-black text-white transition hover:bg-indigo-700 sm:inline-flex" to={routes.customerAppointments}>
        Mis appointments
      </Link>
    </div>
  );
}
