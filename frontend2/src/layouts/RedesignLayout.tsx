import { Link, Outlet, useLocation } from 'react-router-dom';

export function RedesignLayout() {
  const location = useLocation();

  if (location.pathname.startsWith('/admin')) {
    return <Outlet />;
  }

  return (
    <main className="site-shell">
      <header className="site-header">
        <Link className="brand" to="/">Calendar Manager</Link>
        <span className="environment-label">Redesign candidate</span>
      </header>
      <Outlet />
    </main>
  );
}
