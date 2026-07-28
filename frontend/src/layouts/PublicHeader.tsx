import { useState } from 'react';
import { Link, useLocation, useNavigate } from 'react-router-dom';

import { clearAuthSession, getAuthSession, type AuthSession } from '@/auth/authStorage';
import { routes } from '@/lib/routes';

type PublicHeaderProps = {
  className?: string;
  maxWidthClassName?: string;
};

export function PublicHeader({ className = '', maxWidthClassName = 'max-w-6xl' }: PublicHeaderProps) {
  const [session, setSession] = useState(() => getAuthSession());
  const location = useLocation();
  const navigate = useNavigate();

  function handleLogout() {
    if (!session) {
      return;
    }

    clearAuthSession(session.accountType);
    setSession(null);
    navigate(`${location.pathname}${location.search}`, { replace: true, state: { loggedOutAt: Date.now() } });
  }

  return (
    <header className={className}>
      <div className={`mx-auto flex ${maxWidthClassName} flex-col gap-4 px-6 py-6 sm:flex-row sm:items-center sm:justify-between`}>
        <Link className="text-2xl font-black tracking-tight text-slate-950" to={routes.home}>
          Calendar Manager
        </Link>
        <nav className="flex flex-wrap items-center gap-2 sm:justify-end" aria-label="Navegacion publica">
          {session ? <SessionSummary onLogout={handleLogout} session={session} /> : <AnonymousActions />}
        </nav>
      </div>
    </header>
  );
}

function AnonymousActions() {
  return (
    <>
      <Link className="rounded-full border border-slate-200 bg-white/85 px-4 py-2 text-sm font-black text-slate-700 shadow-sm transition hover:border-indigo-200 hover:text-indigo-700" to={routes.customerLogin}>
        Customer login
      </Link>
      <Link className="rounded-full bg-slate-950 px-4 py-2 text-sm font-black text-white shadow-lg shadow-slate-300/60 transition hover:bg-indigo-700" to={routes.businessRegister}>
        Publicar business
      </Link>
    </>
  );
}

function SessionSummary({ onLogout, session }: { onLogout: () => void; session: AuthSession }) {
  const displayName = session.accountType === 'Admin'
    ? session.displayName
    : [session.firstName, session.lastName].filter(Boolean).join(' ') || session.email;
  const destination = session.accountType === 'Admin' ? routes.adminDashboard : routes.customerAppointments;
  const actionLabel = session.accountType === 'Admin' ? 'Panel admin' : 'Mis appointments';

  return (
    <div className="flex flex-wrap items-center gap-2 rounded-2xl border border-indigo-100 bg-white/90 px-3 py-2 text-sm shadow-sm backdrop-blur">
      <div className="flex h-9 w-9 items-center justify-center rounded-full bg-indigo-600 text-sm font-black uppercase text-white">
        {displayName.slice(0, 1)}
      </div>
      <div className="min-w-0 max-w-48">
        <p className="truncate font-black text-slate-950">{displayName}</p>
        <p className="truncate text-xs font-bold text-slate-500">{session.accountType} · {session.email}</p>
      </div>
      <Link className="rounded-full bg-slate-950 px-3 py-2 text-xs font-black text-white transition hover:bg-indigo-700" to={destination}>
        {actionLabel}
      </Link>
      <button className="rounded-full border border-red-200 bg-white px-3 py-2 text-xs font-black text-red-700 transition hover:bg-red-50" onClick={onLogout} type="button">
        Logout
      </button>
    </div>
  );
}
