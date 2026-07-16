import { useQuery } from '@tanstack/react-query';
import type { ReactNode } from 'react';
import { Link, useParams } from 'react-router-dom';

import type { AppointmentDetailsResponse } from '@/api/contracts';
import { getApiErrorMessage } from '@/api/apiErrors';
import { ApiError } from '@/api/httpClient';
import { getAuthSession } from '@/auth/authStorage';
import { getAppointmentDetails } from '@/features/appointments/appointmentApi';
import { ApiErrorAlert } from '@/features/auth/authUi';
import { routes } from '@/lib/routes';

const appointmentDetailsQueryKey = (appointmentId: string, token: string) => ['appointment-details', appointmentId, token] as const;

export function AppointmentDetailsPage() {
  const { appointmentId } = useParams();
  const session = getAuthSession();

  const appointmentQuery = useQuery({
    enabled: Boolean(appointmentId && session?.token),
    queryFn: () => getAppointmentDetails(appointmentId!, session!.token),
    queryKey: appointmentDetailsQueryKey(appointmentId ?? 'missing', session?.token ?? 'anonymous'),
  });

  if (!appointmentId) {
    return <AppointmentState title="Appointment no encontrado" description="La URL no incluye un appointment valido." />;
  }

  if (!session) {
    const returnTo = routes.appointmentDetail(appointmentId);

    return (
      <AppointmentShell>
        <div className="rounded-[2rem] border border-indigo-100 bg-white p-8 text-center shadow-xl shadow-indigo-100/60">
          <p className="text-sm font-black uppercase tracking-[0.24em] text-indigo-600">Appointment protegido</p>
          <h1 className="mt-4 text-4xl font-black tracking-tight text-slate-950">Entra para ver el appointment</h1>
          <p className="mx-auto mt-3 max-w-xl text-sm font-semibold leading-6 text-slate-500">Customers pueden ver sus propios appointments y admins los appointments de su business.</p>
          <div className="mt-6 flex flex-col justify-center gap-3 sm:flex-row">
            <Link className="rounded-2xl bg-indigo-600 px-5 py-3 text-sm font-black text-white hover:bg-indigo-700" to={`${routes.customerLogin}?returnTo=${encodeURIComponent(returnTo)}`}>Entrar como customer</Link>
            <Link className="rounded-2xl border border-slate-200 bg-white px-5 py-3 text-sm font-black text-slate-700 hover:border-indigo-300" to={routes.adminLogin}>Entrar como admin</Link>
          </div>
        </div>
      </AppointmentShell>
    );
  }

  if (appointmentQuery.isPending) {
    return <AppointmentDetailsSkeleton />;
  }

  if (appointmentQuery.isError) {
    if (appointmentQuery.error instanceof ApiError && appointmentQuery.error.status === 404) {
      return <AppointmentState title="Appointment no encontrado" description="Puede que el appointment no exista o haya cambiado la URL." />;
    }

    if (appointmentQuery.error instanceof ApiError && appointmentQuery.error.status === 403) {
      return <AppointmentState title="Sin acceso a este appointment" description="Este appointment pertenece a otro customer o a otro business." />;
    }

    return (
      <AppointmentShell>
        <ApiErrorAlert message={getApiErrorMessage(appointmentQuery.error)} />
      </AppointmentShell>
    );
  }

  return <AppointmentDetails appointment={appointmentQuery.data} />;
}

function AppointmentDetails({ appointment }: { appointment: AppointmentDetailsResponse }) {
  const isCancelled = appointment.status.toLowerCase().startsWith('cancelled') || Boolean(appointment.cancelledAtUtc);
  const customerName = [appointment.customer.firstName, appointment.customer.lastName].filter(Boolean).join(' ');

  return (
    <AppointmentShell>
      <header className="rounded-[2rem] border border-white/80 bg-white/90 p-6 shadow-2xl shadow-indigo-100/70 backdrop-blur md:p-8">
        <Link className="text-sm font-black text-indigo-700 hover:text-indigo-900" to={routes.businessProfile(appointment.business.slug)}>{appointment.business.name}</Link>
        <p className="mt-10 text-sm font-black uppercase tracking-[0.25em] text-indigo-600">Appointment detail</p>
        <h1 className="mt-4 text-5xl font-black leading-[0.95] tracking-tight text-slate-950 md:text-6xl">{appointment.service.nameSnapshot}</h1>
        <p className="mt-5 max-w-3xl text-lg leading-8 text-slate-600">Confirmacion recargable con hora local del business y datos principales del appointment.</p>
      </header>

      <section className="mt-8 grid gap-6 lg:grid-cols-[1.1fr_0.9fr]">
        <div className="rounded-[2rem] border border-slate-200 bg-white p-6 shadow-sm md:p-7">
          <div className={isCancelled ? 'rounded-[1.5rem] border border-amber-200 bg-amber-50 p-5' : 'rounded-[1.5rem] border border-emerald-200 bg-emerald-50 p-5'}>
            <p className={isCancelled ? 'text-sm font-black uppercase tracking-[0.2em] text-amber-700' : 'text-sm font-black uppercase tracking-[0.2em] text-emerald-700'}>{appointment.status}</p>
            <h2 className="mt-2 text-4xl font-black tracking-tight text-slate-950">{formatTime(appointment.startTime)} - {formatTime(appointment.endTime)}</h2>
            <p className="mt-2 text-sm font-bold text-slate-600">{appointment.localDate} · {appointment.business.timeZoneId}</p>
            {isCancelled ? <p className="mt-3 text-sm font-bold text-amber-800">{appointment.cancellationReason ?? 'Appointment cancelado.'}</p> : null}
          </div>

          <div className="mt-6 grid gap-4 sm:grid-cols-2">
            <DetailCard label="Service" value={appointment.service.nameSnapshot} description={`${appointment.service.durationMinutesSnapshot} min · ${formatMoney(appointment.service.priceAmountSnapshot, appointment.service.currencyCodeSnapshot)}`} />
            <DetailCard label="Staff member" value={appointment.staffMember.displayName} />
            <DetailCard label="Customer" value={customerName} description={appointment.customer.email} />
            <DetailCard label="Appointment ID" value={appointment.id} />
          </div>
        </div>

        <aside className="rounded-[2rem] border border-slate-200 bg-slate-950 p-6 text-white shadow-sm md:p-7">
          <p className="text-sm font-black uppercase tracking-[0.2em] text-indigo-200">API consistency</p>
          <h2 className="mt-2 text-2xl font-black">UTC instants</h2>
          <dl className="mt-5 grid gap-4 text-sm font-bold">
            <UtcRow label="Start UTC" value={appointment.startAtUtc} />
            <UtcRow label="End UTC" value={appointment.endAtUtc} />
            <UtcRow label="Created UTC" value={appointment.createdAtUtc} />
          </dl>
          {appointment.customerNotes ? (
            <div className="mt-6 rounded-2xl bg-white/10 p-4">
              <p className="text-xs font-black uppercase tracking-[0.18em] text-indigo-200">Customer notes</p>
              <p className="mt-2 text-sm font-semibold leading-6 text-white">{appointment.customerNotes}</p>
            </div>
          ) : null}
        </aside>
      </section>
    </AppointmentShell>
  );
}

function DetailCard({ description, label, value }: { description?: string; label: string; value: string }) {
  return (
    <div className="rounded-[1.25rem] border border-slate-100 bg-slate-50 p-4">
      <p className="text-xs font-black uppercase tracking-[0.18em] text-slate-400">{label}</p>
      <p className="mt-2 break-words text-lg font-black text-slate-950">{value}</p>
      {description ? <p className="mt-1 break-words text-sm font-semibold text-slate-500">{description}</p> : null}
    </div>
  );
}

function UtcRow({ label, value }: { label: string; value: string }) {
  return (
    <div>
      <dt className="text-slate-400">{label}</dt>
      <dd className="mt-1 break-all text-white">{value}</dd>
    </div>
  );
}

function AppointmentDetailsSkeleton() {
  return (
    <AppointmentShell>
      <p className="sr-only">Cargando appointment</p>
      <div className="h-80 animate-pulse rounded-[2rem] bg-slate-200" />
      <div className="mt-8 grid gap-6 lg:grid-cols-[1.1fr_0.9fr]">
        <div className="h-96 animate-pulse rounded-[2rem] bg-slate-200" />
        <div className="h-96 animate-pulse rounded-[2rem] bg-slate-200" />
      </div>
    </AppointmentShell>
  );
}

function AppointmentState({ description, title }: { description: string; title: string }) {
  return (
    <AppointmentShell>
      <div className="rounded-[2rem] border border-slate-200 bg-white p-10 text-center shadow-sm">
        <p className="text-sm font-black uppercase tracking-[0.25em] text-indigo-600">Appointment detail</p>
        <h1 className="mt-4 text-4xl font-black tracking-tight text-slate-950">{title}</h1>
        <p className="mx-auto mt-4 max-w-xl text-slate-600">{description}</p>
        <Link className="mt-7 inline-flex rounded-2xl bg-slate-950 px-6 py-3.5 text-base font-black text-white hover:bg-indigo-700" to={routes.home}>Volver al inicio</Link>
      </div>
    </AppointmentShell>
  );
}

function AppointmentShell({ children }: { children: ReactNode }) {
  return <main className="min-h-screen bg-[radial-gradient(circle_at_top_left,#e0e7ff,transparent_32%),linear-gradient(135deg,#ffffff_0%,#f8fafc_55%,#eef2ff_100%)] px-6 py-8 text-slate-950"><div className="mx-auto max-w-6xl">{children}</div></main>;
}

function formatTime(value: string) {
  return value.slice(0, 5);
}

function formatMoney(value: number, currencyCode: string) {
  return `${value.toFixed(2)} ${currencyCode}`;
}
