import { Link, Outlet } from 'react-router-dom';

export function RedesignLayout() {
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
