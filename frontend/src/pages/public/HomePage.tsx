import { Link } from 'react-router-dom';

import { getAuthSession } from '@/auth/authStorage';
import { CustomerSessionBadge } from '@/features/auth/CustomerSessionBadge';
import { routes } from '@/lib/routes';

export function HomePage() {
  const customerSession = getAuthSession('Customer');

  return (
    <main className="min-h-screen overflow-hidden bg-[radial-gradient(circle_at_12%_12%,#e0e7ff,transparent_30%),linear-gradient(135deg,#ffffff_0%,#f8fafc_45%,#eef2ff_100%)] text-slate-950">
      <header className="mx-auto flex max-w-6xl items-center justify-between px-6 py-8">
        <Link className="text-2xl font-black tracking-tight" to="/">
          Calendar Manager
        </Link>
        <nav className="flex items-center gap-3">
          {customerSession ? (
            <CustomerSessionBadge session={customerSession} />
          ) : (
            <>
              <Link className="hidden rounded-full px-5 py-2 text-sm font-black text-slate-700 hover:bg-white/70 sm:inline-flex" to={routes.customerLogin}>
                Customer login
              </Link>
              <Link className="rounded-full bg-slate-950 px-5 py-2.5 text-sm font-black text-white shadow-lg shadow-slate-300/60 transition hover:bg-indigo-700" to={routes.businessRegister}>
                Para negocios
              </Link>
            </>
          )}
        </nav>
      </header>
      <section className="mx-auto grid max-w-6xl gap-12 px-6 py-14 lg:grid-cols-[1.02fr_0.98fr] lg:items-center lg:py-20">
        <div>
          <p className="text-sm font-black uppercase tracking-[0.25em] text-indigo-600">Marketplace de appointments</p>
          <h1 className="mt-5 text-5xl font-black leading-[0.95] tracking-tight text-slate-950 md:text-7xl">Reserva services con staff members disponibles.</h1>
          <p className="mt-6 max-w-2xl text-lg leading-8 text-slate-600">
            Encuentra el business, revisa services activos, conoce el staff y salta al flujo de appointment cuando estes listo para elegir slot.
          </p>
          <div className="mt-9 flex flex-col gap-3 sm:flex-row">
            <Link className="rounded-2xl bg-indigo-600 px-6 py-4 text-center text-base font-black text-white shadow-xl shadow-indigo-200 transition hover:bg-indigo-700" to="/b/demo-barber">
              Ver perfil demo
            </Link>
            {customerSession ? (
              <Link className="rounded-2xl border border-slate-200 bg-white/80 px-6 py-4 text-center text-base font-black text-slate-800 shadow-sm transition hover:bg-white" to={routes.customerAppointments}>
                Mis appointments
              </Link>
            ) : (
              <Link className="rounded-2xl border border-slate-200 bg-white/80 px-6 py-4 text-center text-base font-black text-slate-800 shadow-sm transition hover:bg-white" to={routes.businessRegister}>
                Publicar mi business
              </Link>
            )}
          </div>
          <div className="mt-10 grid gap-3 sm:grid-cols-3">
            {[
              ['Services claros', 'Duracion y precio visibles antes de reservar.'],
              ['Staff conectado', 'Cada service muestra quien puede atenderlo.'],
              ['Timezone local', 'La experiencia respeta el horario del business.'],
            ].map(([title, description]) => (
              <article className="rounded-2xl border border-white/80 bg-white/70 p-4 shadow-sm backdrop-blur" key={title}>
                <h2 className="text-sm font-black text-slate-950">{title}</h2>
                <p className="mt-2 text-sm leading-6 text-slate-600">{description}</p>
              </article>
            ))}
          </div>
        </div>
        <div className="relative">
          <div className="absolute -left-8 top-10 h-32 w-32 rounded-full bg-indigo-200 blur-3xl" />
          <div className="relative rounded-[2rem] border border-white/80 bg-white/85 p-5 shadow-2xl shadow-indigo-100/80 backdrop-blur md:p-7">
            <div className="rounded-[1.5rem] bg-slate-950 p-5 text-white">
              <p className="text-sm font-black uppercase tracking-[0.2em] text-indigo-200">Hoy disponible</p>
              <h2 className="mt-3 text-3xl font-black">Demo Barber Shop</h2>
              <p className="mt-2 text-sm leading-6 text-slate-300">Cortes, barba y styling con staff activo para appointments publicos.</p>
            </div>
            <div className="mt-5 grid gap-3">
              {[
                ['Corte clasico', '45 min', 'Ana Ruiz'],
                ['Barba premium', '30 min', 'Mario Lopez'],
                ['Corte y barba', '75 min', 'Ana Ruiz + Mario'],
              ].map(([service, duration, staff]) => (
                <div className="flex items-center justify-between rounded-2xl border border-slate-100 bg-white px-4 py-3 shadow-sm" key={service}>
                  <div>
                    <p className="font-black">{service}</p>
                    <p className="text-sm font-semibold text-slate-500">{duration} - {staff}</p>
                  </div>
                  <span className="rounded-full bg-indigo-50 px-3 py-1 text-xs font-black text-indigo-700">Slots</span>
                </div>
              ))}
            </div>
          </div>
        </div>
      </section>
    </main>
  );
}
