import { Link } from 'react-router-dom';

export function HomePage() {
  return (
    <main className="mx-auto flex min-h-screen max-w-6xl flex-col px-6 py-8">
      <header className="flex items-center justify-between">
        <div className="text-2xl font-black">Calendar Manager</div>
        <Link className="rounded-full bg-slate-950 px-5 py-2 text-sm font-bold text-white" to="/auth/business/register">
          Para negocios
        </Link>
      </header>
      <section className="flex flex-1 items-center py-20">
        <div className="max-w-3xl">
          <p className="text-sm font-black uppercase tracking-[0.25em] text-indigo-600">Marketplace de appointments</p>
          <h1 className="mt-5 text-5xl font-black tracking-tight text-slate-950 md:text-7xl">
            Reserva services con staff members disponibles.
          </h1>
          <p className="mt-6 max-w-2xl text-lg text-slate-600">
            Placeholder inicial para implementar `designs/homepage.op`.
          </p>
        </div>
      </section>
    </main>
  );
}
