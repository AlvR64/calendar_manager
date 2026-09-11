import { CalendarDays, ChevronRight, Clock3, Settings2, UsersRound, Wrench } from 'lucide-react';
import { Link, NavLink, Outlet } from 'react-router-dom';

const navigation = [
  { icon: CalendarDays, label: 'Overview', to: '/admin' },
  { icon: Clock3, label: 'Appointments', to: '/admin/appointments' },
  { icon: Wrench, label: 'Services', to: '/admin/services' },
  { icon: UsersRound, label: 'Staff members', to: '/admin/staff-members' },
  { icon: Settings2, label: 'Business settings', to: '/admin/business-settings' },
];

export function AdminShell() {
  return (
    <div className="admin-shell">
      <aside className="admin-sidebar" aria-label="Admin navigation">
        <Link className="admin-brand" to="/admin">
          <span aria-hidden="true" className="admin-brand-mark">CM</span>
          <span>Calendar Manager</span>
        </Link>
        <nav className="admin-navigation">
          {navigation.map(({ icon: Icon, label, to }) => (
            <NavLink className="admin-nav-link" end={to === '/admin'} key={to} to={to}>
              <Icon aria-hidden="true" size={17} strokeWidth={1.7} />
              <span>{label}</span>
            </NavLink>
          ))}
        </nav>
        <Link className="admin-public-link" to="/">
          View public site <ChevronRight aria-hidden="true" size={15} />
        </Link>
      </aside>
      <main className="admin-content">
        <Outlet />
      </main>
    </div>
  );
}
