import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import type { ReactNode } from 'react';
import { useState } from 'react';
import { Link } from 'react-router-dom';

import type { AppointmentSummaryResponse } from '@/api/contracts';
import { getApiErrorMessage } from '@/api/apiErrors';
import { getAuthSession } from '@/auth/authStorage';
import { cancelCustomerAppointment, listCustomerAppointments } from '@/features/appointments/appointmentApi';
import { ApiErrorAlert, inputClassName, labelClassName, primaryButtonClassName } from '@/features/auth/authUi';
import { routes } from '@/lib/routes';

const customerAppointmentsQueryKey = (token: string, from: string, to: string, status: string) => ['customer-appointments', token, from, to, status] as const;

export function CustomerAppointmentsPage() {
  const session = getAuthSession('Customer');
  const queryClient = useQueryClient();
  const [fromDate, setFromDate] = useState('');
  const [toDate, setToDate] = useState('');
  const [status, setStatus] = useState('');
  const [appointmentToCancel, setAppointmentToCancel] = useState<AppointmentSummaryResponse | null>(null);
  const [cancellationReason, setCancellationReason] = useState('');
  const token = session?.token ?? '';

  const appointmentsQuery = useQuery({
    enabled: Boolean(token),
    queryFn: () => listCustomerAppointments(token, {
      from: toStartOfDayUtc(fromDate),
      status: status || undefined,
      to: toEndOfDayUtc(toDate),
    }),
    queryKey: customerAppointmentsQueryKey(token, fromDate, toDate, status),
  });

  const cancelMutation = useMutation({
    mutationFn: (appointment: AppointmentSummaryResponse) => cancelCustomerAppointment(appointment.id, { cancellationReason: normalizeOptionalText(cancellationReason) }, token),
    onSuccess: async () => {
      setAppointmentToCancel(null);
      setCancellationReason('');
      await queryClient.invalidateQueries({ queryKey: ['customer-appointments'] });
    },
  });

  const appointments = appointmentsQuery.data ?? [];
  const upcoming = appointments.filter((appointment) => isScheduledFuture(appointment));
  const cancelled = appointments.filter((appointment) => isCancelled(appointment));
  const past = appointments.filter((appointment) => !isScheduledFuture(appointment) && !isCancelled(appointment));
  const hasFilters = Boolean(fromDate || toDate || status);

  return (
    <CustomerAppointmentsShell>
      <header className="rounded-[2rem] border border-white/80 bg-white/90 p-6 shadow-2xl shadow-indigo-100/70 backdrop-blur md:p-8">
        <Link className="text-sm font-black text-indigo-700 hover:text-indigo-900" to={routes.home}>Calendar Manager</Link>
        <p className="mt-10 text-sm font-black uppercase tracking-[0.25em] text-indigo-600">Customer appointments</p>
        <h1 className="mt-4 text-5xl font-black leading-[0.95] tracking-tight text-slate-950 md:text-6xl">Tus appointments</h1>
        <p className="mt-5 max-w-3xl text-lg leading-8 text-slate-600">Consulta tus appointments proximos, pasados y cancelados. Puedes cancelar appointments scheduled futuros.</p>
      </header>

      <section className="mt-8 rounded-[2rem] border border-slate-200 bg-white p-6 shadow-sm md:p-7">
        <div className="grid gap-4 md:grid-cols-3">
          <label className={labelClassName}>
            Desde
            <input className={inputClassName} onChange={(event) => setFromDate(event.target.value)} type="date" value={fromDate} />
          </label>
          <label className={labelClassName}>
            Hasta
            <input className={inputClassName} onChange={(event) => setToDate(event.target.value)} type="date" value={toDate} />
          </label>
          <label className={labelClassName}>
            Estado
            <select className={inputClassName} onChange={(event) => setStatus(event.target.value)} value={status}>
              <option value="">Todos</option>
              <option value="Scheduled">Scheduled</option>
              <option value="CancelledByCustomer">Cancelled by customer</option>
              <option value="CancelledByAdmin">Cancelled by admin</option>
              <option value="Completed">Completed</option>
              <option value="NoShow">No-show</option>
            </select>
          </label>
        </div>
      </section>

      {appointmentsQuery.isPending ? (
        <AppointmentsSkeleton />
      ) : appointmentsQuery.isError ? (
        <div className="mt-8"><ApiErrorAlert message={getApiErrorMessage(appointmentsQuery.error)} /></div>
      ) : appointments.length === 0 ? (
        <EmptyState title={hasFilters ? 'No hay appointments para estos filtros' : 'No tienes appointments todavia'} description={hasFilters ? 'Ajusta el rango o estado para ver otros appointments.' : 'Reserva un appointment desde el perfil publico de un business.'} />
      ) : (
        <div className="mt-8 grid gap-8">
          <AppointmentSection appointments={upcoming} emptyDescription="No tienes appointments proximos." onCancel={setAppointmentToCancel} title="Proximos" />
          <AppointmentSection appointments={past} emptyDescription="No hay appointments pasados con estos filtros." onCancel={setAppointmentToCancel} title="Pasados" />
          <AppointmentSection appointments={cancelled} emptyDescription="No hay appointments cancelados con estos filtros." onCancel={setAppointmentToCancel} title="Cancelados" />
        </div>
      )}

      {appointmentToCancel ? (
        <CancelPanel
          appointment={appointmentToCancel}
          error={cancelMutation.error}
          isCancelling={cancelMutation.isPending}
          onCancel={() => cancelMutation.mutate(appointmentToCancel)}
          onClose={() => {
            setAppointmentToCancel(null);
            setCancellationReason('');
            cancelMutation.reset();
          }}
          onReasonChange={setCancellationReason}
          reason={cancellationReason}
        />
      ) : null}
    </CustomerAppointmentsShell>
  );
}

function AppointmentSection({ appointments, emptyDescription, onCancel, title }: { appointments: AppointmentSummaryResponse[]; emptyDescription: string; onCancel: (appointment: AppointmentSummaryResponse) => void; title: string }) {
  return (
    <section className="rounded-[2rem] border border-slate-200 bg-white p-6 shadow-sm md:p-7">
      <div className="flex items-center justify-between gap-4">
        <h2 className="text-2xl font-black tracking-tight text-slate-950">{title}</h2>
        <span className="rounded-full bg-slate-100 px-3 py-1 text-sm font-black text-slate-500">{appointments.length}</span>
      </div>
      {appointments.length === 0 ? (
        <p className="mt-5 rounded-[1.5rem] border border-dashed border-slate-300 bg-slate-50 p-6 text-center text-sm font-semibold text-slate-500">{emptyDescription}</p>
      ) : (
        <div className="mt-5 grid gap-4">
          {appointments.map((appointment) => <AppointmentCard appointment={appointment} key={appointment.id} onCancel={onCancel} />)}
        </div>
      )}
    </section>
  );
}

function AppointmentCard({ appointment, onCancel }: { appointment: AppointmentSummaryResponse; onCancel: (appointment: AppointmentSummaryResponse) => void }) {
  const canCancel = isScheduledFuture(appointment);

  return (
    <article className="rounded-[1.5rem] border border-slate-200 bg-slate-50 p-5">
      <div className="grid gap-5 lg:grid-cols-[1fr_auto] lg:items-center">
        <div>
          <p className="text-sm font-black uppercase tracking-[0.2em] text-indigo-600">{appointment.status}</p>
          <h3 className="mt-2 text-2xl font-black text-slate-950">{appointment.service.nameSnapshot}</h3>
          <p className="mt-2 text-sm font-bold text-slate-600">{appointment.business.name} · {appointment.staffMember.displayName}</p>
          <p className="mt-3 text-lg font-black text-slate-950">{appointment.localDate} · {formatTime(appointment.startTime)} - {formatTime(appointment.endTime)}</p>
          <p className="mt-1 text-xs font-bold text-slate-400">{appointment.business.timeZoneId} · UTC {formatUtc(appointment.startAtUtc)}</p>
          {appointment.cancellationReason ? <p className="mt-3 rounded-2xl bg-amber-50 px-4 py-3 text-sm font-bold text-amber-800">{appointment.cancellationReason}</p> : null}
        </div>
        <div className="flex flex-col gap-3 sm:flex-row lg:flex-col">
          <Link className="rounded-2xl border border-slate-200 bg-white px-5 py-3 text-center text-sm font-black text-slate-700 hover:border-indigo-300" to={routes.appointmentDetail(appointment.id)}>Ver detalle</Link>
          {canCancel ? <button className="rounded-2xl bg-red-600 px-5 py-3 text-sm font-black text-white hover:bg-red-700" onClick={() => onCancel(appointment)} type="button">Cancelar</button> : null}
        </div>
      </div>
    </article>
  );
}

function CancelPanel({ appointment, error, isCancelling, onCancel, onClose, onReasonChange, reason }: { appointment: AppointmentSummaryResponse; error: Error | null; isCancelling: boolean; onCancel: () => void; onClose: () => void; onReasonChange: (value: string) => void; reason: string }) {
  return (
    <div className="fixed inset-0 z-50 flex items-end bg-slate-950/40 px-4 py-6 sm:items-center sm:justify-center">
      <section className="w-full max-w-xl rounded-[2rem] bg-white p-6 shadow-2xl">
        <p className="text-sm font-black uppercase tracking-[0.2em] text-red-600">Cancelar appointment</p>
        <h2 className="mt-3 text-3xl font-black text-slate-950">{appointment.service.nameSnapshot}</h2>
        <p className="mt-2 text-sm font-bold text-slate-500">{appointment.localDate} · {formatTime(appointment.startTime)} - {formatTime(appointment.endTime)}</p>
        <label className={`${labelClassName} mt-5`}>
          Motivo opcional
          <textarea className={`${inputClassName} min-h-28`} maxLength={500} onChange={(event) => onReasonChange(event.target.value)} value={reason} />
        </label>
        <div className="mt-4"><ApiErrorAlert message={error ? getApiErrorMessage(error) : undefined} /></div>
        <div className="mt-6 flex flex-col gap-3 sm:flex-row">
          <button className="rounded-2xl border border-slate-200 bg-white px-5 py-3 text-sm font-black text-slate-700 hover:border-slate-400" disabled={isCancelling} onClick={onClose} type="button">Volver</button>
          <button className={`${primaryButtonClassName} bg-red-600 hover:bg-red-700`} disabled={isCancelling} onClick={onCancel} type="button">{isCancelling ? 'Cancelando...' : 'Confirmar cancelacion'}</button>
        </div>
      </section>
    </div>
  );
}

function EmptyState({ description, title }: { description: string; title: string }) {
  return (
    <section className="mt-8 rounded-[2rem] border border-slate-200 bg-white p-10 text-center shadow-sm">
      <h2 className="text-3xl font-black text-slate-950">{title}</h2>
      <p className="mx-auto mt-3 max-w-xl text-sm font-semibold text-slate-500">{description}</p>
    </section>
  );
}

function AppointmentsSkeleton() {
  return (
    <div className="mt-8 grid gap-8">
      {Array.from({ length: 3 }).map((_, index) => <div className="h-56 animate-pulse rounded-[2rem] bg-slate-200" key={index} />)}
    </div>
  );
}

function CustomerAppointmentsShell({ children }: { children: ReactNode }) {
  return <main className="min-h-screen bg-[radial-gradient(circle_at_top_left,#e0e7ff,transparent_32%),linear-gradient(135deg,#ffffff_0%,#f8fafc_55%,#eef2ff_100%)] px-6 py-8 text-slate-950"><div className="mx-auto max-w-6xl">{children}</div></main>;
}

function isScheduledFuture(appointment: AppointmentSummaryResponse) {
  return appointment.status === 'Scheduled' && new Date(appointment.startAtUtc) > new Date();
}

function isCancelled(appointment: AppointmentSummaryResponse) {
  return appointment.status.startsWith('Cancelled') || Boolean(appointment.cancelledAtUtc);
}

function formatTime(value: string) {
  return value.slice(0, 5);
}

function formatUtc(value: string) {
  return new Date(value).toISOString().slice(11, 16);
}

function normalizeOptionalText(value: string) {
  const trimmed = value.trim();
  return trimmed.length > 0 ? trimmed : null;
}

function toStartOfDayUtc(date: string) {
  return date ? `${date}T00:00:00.000Z` : undefined;
}

function toEndOfDayUtc(date: string) {
  return date ? `${date}T23:59:59.999Z` : undefined;
}
