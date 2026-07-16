import { useQuery } from '@tanstack/react-query';
import type { ReactNode } from 'react';
import { Link } from 'react-router-dom';

import type { AdminDashboardStatusCountResponse, AppointmentSummaryResponse } from '@/api/contracts';
import { getApiErrorMessage } from '@/api/apiErrors';
import { getAuthSession } from '@/auth/authStorage';
import { ApiErrorAlert } from '@/features/auth/authUi';
import { getAdminDashboardSummary } from '@/features/dashboard/dashboardApi';
import { routes } from '@/lib/routes';

export function AdminDashboardPage() {
  const session = getAuthSession('Admin');
  const summaryQuery = useQuery({
    queryFn: () => getAdminDashboardSummary(session!.token),
    queryKey: ['admin', 'dashboard-summary'],
  });

  if (summaryQuery.isPending) {
    return <DashboardSkeleton />;
  }

  if (summaryQuery.isError) {
    return (
      <DashboardFrame>
        <ApiErrorAlert message={getApiErrorMessage(summaryQuery.error)} />
      </DashboardFrame>
    );
  }

  const summary = summaryQuery.data;

  return (
    <DashboardFrame>
      <div className="grid gap-5 md:grid-cols-3">
        <MetricCard label="Appointments hoy" value={String(summary.todayAppointmentCount)} helper="Segun timezone del business" />
        <MetricCard label="Ingresos estimados" value={formatMoney(summary.estimatedRevenueAmount, summary.currencyCode)} helper={`${summary.rangeStartLocalDate} - ${summary.rangeEndLocalDate}`} />
        <MetricCard label="Proximos" value={String(summary.upcomingAppointments.length)} helper="Appointments no cancelados" />
      </div>

      <div className="mt-6 grid gap-6 xl:grid-cols-[1.4fr_0.8fr]">
        <section className="rounded-[2rem] border border-slate-200 bg-white p-5 shadow-sm md:p-6">
          <div className="flex flex-col gap-3 border-b border-slate-100 pb-5 sm:flex-row sm:items-center sm:justify-between">
            <div>
              <p className="text-sm font-black uppercase tracking-[0.18em] text-indigo-600">Upcoming</p>
              <h2 className="mt-2 text-2xl font-black tracking-tight">Proximos appointments</h2>
            </div>
            <Link className="rounded-xl border border-slate-200 px-4 py-2 text-sm font-black text-slate-700 transition hover:bg-slate-50" to={routes.adminAppointments}>
              Ver agenda completa
            </Link>
          </div>
          <div className="mt-5 grid gap-3">
            {summary.upcomingAppointments.length === 0 ? (
              <EmptyPanel title="Sin proximos appointments" description="Cuando entren reservas reales apareceran aqui." />
            ) : (
              summary.upcomingAppointments.map((appointment) => <UpcomingAppointmentCard appointment={appointment} key={appointment.id} />)
            )}
          </div>
        </section>

        <section className="rounded-[2rem] border border-slate-200 bg-white p-5 shadow-sm md:p-6">
          <p className="text-sm font-black uppercase tracking-[0.18em] text-indigo-600">Status</p>
          <h2 className="mt-2 text-2xl font-black tracking-tight">Conteo por status</h2>
          <div className="mt-5 grid gap-3">
            {summary.statusCounts.length === 0 ? (
              <EmptyPanel title="Sin datos" description="No hay appointments en el rango." />
            ) : (
              summary.statusCounts.map((statusCount) => <StatusCountRow key={statusCount.status} statusCount={statusCount} />)
            )}
          </div>
        </section>
      </div>
    </DashboardFrame>
  );
}

function DashboardFrame({ children }: { children: ReactNode }) {
  return (
    <div>
      <div className="mb-8 flex flex-col gap-4 md:flex-row md:items-end md:justify-between">
        <div>
          <p className="text-sm font-black uppercase tracking-[0.22em] text-indigo-600">Admin dashboard</p>
          <h1 className="mt-3 text-4xl font-black tracking-tight text-slate-950">Resumen operativo</h1>
          <p className="mt-3 max-w-3xl text-slate-600">Appointments de hoy, proximas reservas, ingresos estimados y status del rango activo.</p>
        </div>
      </div>
      {children}
    </div>
  );
}

function MetricCard({ helper, label, value }: { helper: string; label: string; value: string }) {
  return (
    <article className="rounded-[2rem] border border-slate-200 bg-white p-5 shadow-sm">
      <p className="text-sm font-black uppercase tracking-[0.16em] text-slate-500">{label}</p>
      <div className="mt-3 text-4xl font-black tracking-tight text-slate-950">{value}</div>
      <p className="mt-2 text-sm font-bold text-slate-500">{helper}</p>
    </article>
  );
}

function UpcomingAppointmentCard({ appointment }: { appointment: AppointmentSummaryResponse }) {
  const customerName = [appointment.customer.firstName, appointment.customer.lastName].filter(Boolean).join(' ');

  return (
    <article className="rounded-2xl border border-slate-200 p-4">
      <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <p className="text-lg font-black">{appointment.localDate} · {formatTime(appointment.startTime)} - {formatTime(appointment.endTime)}</p>
          <p className="mt-1 text-sm font-bold text-slate-500">{appointment.service.nameSnapshot} · {appointment.staffMember.displayName}</p>
          <p className="mt-1 text-sm text-slate-500">{customerName || appointment.customer.email}</p>
        </div>
        <span className="rounded-full bg-emerald-100 px-3 py-1 text-xs font-black text-emerald-700">{appointment.status}</span>
      </div>
    </article>
  );
}

function StatusCountRow({ statusCount }: { statusCount: AdminDashboardStatusCountResponse }) {
  return (
    <div className="flex items-center justify-between rounded-2xl border border-slate-200 px-4 py-3">
      <span className="font-black text-slate-700">{statusCount.status}</span>
      <span className="rounded-full bg-slate-100 px-3 py-1 text-sm font-black text-slate-700">{statusCount.count}</span>
    </div>
  );
}

function EmptyPanel({ description, title }: { description: string; title: string }) {
  return (
    <div className="rounded-[1.5rem] border border-dashed border-slate-300 bg-slate-50 p-6 text-center">
      <h3 className="text-lg font-black">{title}</h3>
      <p className="mt-2 text-sm text-slate-500">{description}</p>
    </div>
  );
}

function DashboardSkeleton() {
  return (
    <DashboardFrame>
      <div className="grid gap-5 md:grid-cols-3">
        <div className="h-36 animate-pulse rounded-[2rem] bg-slate-200" />
        <div className="h-36 animate-pulse rounded-[2rem] bg-slate-200" />
        <div className="h-36 animate-pulse rounded-[2rem] bg-slate-200" />
      </div>
      <div className="mt-6 h-80 animate-pulse rounded-[2rem] bg-slate-200" />
    </DashboardFrame>
  );
}

function formatTime(value: string) {
  return value.slice(0, 5);
}

function formatMoney(amount: number, currencyCode: string) {
  return new Intl.NumberFormat('es-ES', { currency: currencyCode, style: 'currency' }).format(amount);
}
