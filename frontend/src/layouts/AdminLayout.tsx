import { NavLink, Outlet } from 'react-router-dom';

const navItems = [
  { to: '/admin', label: 'Dashboard', end: true },
  { to: '/admin/business-settings', label: 'Business settings' },
  { to: '/admin/services', label: 'Services' },
  { to: '/admin/staff-members', label: 'Staff members' },
  { to: '/admin/availability', label: 'Availability' },
];

export function AdminLayout() {
  return (
    <div className="min-h-screen bg-slate-50 text-slate-950 lg:flex">
      <aside className="border-b border-slate-200 bg-white p-5 lg:min-h-screen lg:w-72 lg:border-b-0 lg:border-r">
        <div className="text-xl font-black">Calendar Manager</div>
        <div className="mt-6 rounded-3xl border border-slate-200 bg-slate-50 p-4">
          <div className="text-xs font-bold text-slate-500">Business actual</div>
          <div className="mt-1 text-lg font-black">Studio Centro</div>
          <div className="mt-1 text-xs text-slate-500">Un admin gestiona un solo business</div>
        </div>
        <nav className="mt-6 grid gap-2">
          {navItems.map((item) => (
            <NavLink
              className={({ isActive }) =>
                [
                  'rounded-2xl px-4 py-3 text-sm font-bold transition',
                  isActive ? 'bg-slate-950 text-white' : 'text-slate-600 hover:bg-slate-100 hover:text-slate-950',
                ].join(' ')
              }
              end={item.end}
              key={item.to}
              to={item.to}
            >
              {item.label}
            </NavLink>
          ))}
        </nav>
      </aside>
      <main className="flex-1 p-6 lg:p-8">
        <Outlet />
      </main>
    </div>
  );
}
