import { useQuery } from '@tanstack/react-query';
import { Link } from 'react-router-dom';
import { useState, type FormEvent } from 'react';

import type { AppointmentSummaryResponse } from '@/api/contracts';
import { getApiErrorMessage } from '@/api/apiErrors';
import { getAuthSession } from '@/auth/authStorage';
import { ApiErrorAlert, inputClassName, labelClassName, primaryButtonClassName } from '@/features/auth/authUi';
import { listAdminAppointments, type AdminAppointmentFilters } from '@/features/appointments/appointmentApi';
import { listServices } from '@/features/services/serviceApi';
import { listStaffMembers } from '@/features/staffMembers/staffMemberApi';
import { routes } from '@/lib/routes';

const staffMembersQueryKey = ['admin', 'staff-members'] as const;
const servicesQueryKey = ['admin', 'services'] as const;

const statusOptions = [
  { label: 'Todos', value: '' },
  { label: 'Scheduled', value: 'Scheduled' },
  { label: 'Completed', value: 'Completed' },
  { label: 'No-show', value: 'NoShow' },
  { label: 'Cancelled by customer', value: 'CancelledByCustomer' },
  { label: 'Cancelled by admin', value: 'CancelledByAdmin' },
];

export function AdminAppointmentsPage() {
  const session = getAuthSession('Admin');
  const [draftFilters, setDraftFilters] = useState<AdminAppointmentFilters>(() => defaultFilters());
  const [appliedFilters, setAppliedFilters] = useState<AdminAppointmentFilters>(() => defaultFilters());

  const staffMembersQuery = useQuery({
    queryFn: () => listStaffMembers(session!.token),
    queryKey: staffMembersQueryKey,
  });

  const servicesQuery = useQuery({
    queryFn: () => listServices(session!.token),
    queryKey: servicesQueryKey,
  });

  const appointmentsQuery = useQuery({
    queryFn: () => listAdminAppointments(session!.token, appliedFilters),
    queryKey: ['admin', 'appointments', appliedFilters],
  });

  function handleFilterChange<Key extends keyof AdminAppointmentFilters>(key: Key, value: AdminAppointmentFilters[Key]) {
    setDraftFilters((current) => ({ ...current, [key]: value }));
  }

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setAppliedFilters(cleanFilters(draftFilters));
  }

  const staffMembers = staffMembersQuery.data ?? [];
  const services = servicesQuery.data ?? [];
  const appointments = appointmentsQuery.data ?? [];
  const isLoading = staffMembersQuery.isPending || servicesQuery.isPending || appointmentsQuery.isPending;
  const error = staffMembersQuery.error ?? servicesQuery.error ?? appointmentsQuery.error;
  const groupedAppointments = groupByLocalDate(appointments);

  return (
    <div>
      <div className="mb-8 flex flex-col gap-4 md:flex-row md:items-end md:justify-between">
        <div>
          <p className="text-sm font-black uppercase tracking-[0.22em] text-indigo-600">Admin appointments</p>
          <h1 className="mt-3 text-4xl font-black tracking-tight text-slate-950">Agenda operativa</h1>
          <p className="mt-3 max-w-3xl text-slate-600">Consulta appointments del business por rango, staff member, service y status. Los horarios son locales del business.</p>
        </div>
        <div className="rounded-2xl border border-indigo-100 bg-indigo-50 px-5 py-3 text-sm font-black text-indigo-700 shadow-sm">
          {appliedFilters.from} - {appliedFilters.to}
        </div>
      </div>

      <form className="rounded-[2rem] border border-slate-200 bg-white p-5 shadow-sm md:p-6" onSubmit={handleSubmit}>
        <div className="grid gap-4 md:grid-cols-2 xl:grid-cols-5">
          <label className={labelClassName}>
            From
            <input className={inputClassName} onChange={(event) => handleFilterChange('from', event.target.value)} required type="date" value={draftFilters.from} />
          </label>
          <label className={labelClassName}>
            To
            <input className={inputClassName} onChange={(event) => handleFilterChange('to', event.target.value)} required type="date" value={draftFilters.to} />
          </label>
          <label className={labelClassName}>
            Staff member
            <select className={inputClassName} onChange={(event) => handleFilterChange('staffMemberId', event.target.value)} value={draftFilters.staffMemberId ?? ''}>
              <option value="">Todos</option>
              {staffMembers.map((staffMember) => (
                <option key={staffMember.id} value={staffMember.id}>
                  {staffMember.displayName}{staffMember.isActive ? '' : ' (inactive)'}
                </option>
              ))}
            </select>
          </label>
          <label className={labelClassName}>
            Service
            <select className={inputClassName} onChange={(event) => handleFilterChange('serviceId', event.target.value)} value={draftFilters.serviceId ?? ''}>
              <option value="">Todos</option>
              {services.map((service) => (
                <option key={service.id} value={service.id}>
                  {service.name}{service.isActive ? '' : ' (inactive)'}
                </option>
              ))}
            </select>
          </label>
          <label className={labelClassName}>
            Status
            <select className={inputClassName} onChange={(event) => handleFilterChange('status', event.target.value)} value={draftFilters.status ?? ''}>
              {statusOptions.map((option) => (
                <option key={option.value} value={option.value}>{option.label}</option>
              ))}
            </select>
          </label>
        </div>
        <div className="mt-5 flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
          <p className="text-sm font-bold text-slate-500">Maximo backend: 90 dias por consulta.</p>
          <button className={`${primaryButtonClassName} sm:w-auto sm:px-8`} type="submit">Aplicar filtros</button>
        </div>
      </form>

      {error ? <div className="mt-6"><ApiErrorAlert message={getApiErrorMessage(error)} /></div> : null}

      {isLoading ? (
        <AdminAppointmentsSkeleton />
      ) : appointments.length === 0 && !error ? (
        <EmptyPanel title="Sin appointments en este rango" description="Cambia el rango o limpia filtros para revisar otras fechas." />
      ) : !error ? (
        <div className="mt-6 grid gap-5">
          {groupedAppointments.map((group) => (
            <section className="rounded-[2rem] border border-slate-200 bg-white p-5 shadow-sm md:p-6" key={group.localDate}>
              <div className="flex flex-col gap-2 border-b border-slate-100 pb-4 sm:flex-row sm:items-center sm:justify-between">
                <div>
                  <p className="text-sm font-black uppercase tracking-[0.18em] text-indigo-600">{formatDateLabel(group.localDate)}</p>
                  <h2 className="mt-1 text-2xl font-black tracking-tight">{group.localDate}</h2>
                </div>
                <span className="rounded-full bg-slate-100 px-4 py-2 text-sm font-black text-slate-700">{group.appointments.length} appointments</span>
              </div>
              <div className="mt-4 grid gap-3">
                {group.appointments.map((appointment) => (
                  <AppointmentCard appointment={appointment} key={appointment.id} />
                ))}
              </div>
            </section>
          ))}
        </div>
      ) : null}
    </div>
  );
}

function AppointmentCard({ appointment }: { appointment: AppointmentSummaryResponse }) {
  const customerName = [appointment.customer.firstName, appointment.customer.lastName].filter(Boolean).join(' ');

  return (
    <article className="rounded-2xl border border-slate-200 p-4 transition hover:border-indigo-200 hover:bg-indigo-50/40">
      <div className="flex flex-col gap-4 lg:flex-row lg:items-center lg:justify-between">
        <div>
          <div className="flex flex-wrap items-center gap-2">
            <span className="text-xl font-black tracking-tight">{formatTime(appointment.startTime)} - {formatTime(appointment.endTime)}</span>
            <StatusPill status={appointment.status} />
          </div>
          <p className="mt-2 text-base font-black text-slate-800">{appointment.service.nameSnapshot}</p>
          <p className="mt-1 text-sm font-bold text-slate-500">{appointment.staffMember.displayName} · {customerName || appointment.customer.email}</p>
          <p className="mt-1 text-sm text-slate-500">{appointment.customer.email}</p>
          {appointment.customerNotes ? <p className="mt-2 rounded-2xl bg-slate-50 px-3 py-2 text-sm text-slate-600">{appointment.customerNotes}</p> : null}
        </div>
        <div className="flex flex-col gap-2 text-sm font-bold text-slate-500 lg:items-end">
          <span>{appointment.service.durationMinutesSnapshot} min · {formatMoney(appointment.service.priceAmountSnapshot, appointment.service.currencyCodeSnapshot)}</span>
          <Link className="rounded-xl border border-slate-200 px-4 py-2 text-center font-black text-slate-700 transition hover:bg-white" to={routes.appointmentDetail(appointment.id)}>
            Ver detalle
          </Link>
        </div>
      </div>
    </article>
  );
}

function StatusPill({ status }: { status: string }) {
  const className = status === 'Scheduled'
    ? 'bg-emerald-100 text-emerald-700'
    : status.startsWith('Cancelled')
      ? 'bg-red-100 text-red-700'
      : 'bg-slate-200 text-slate-700';

  return <span className={`rounded-full px-3 py-1 text-xs font-black ${className}`}>{status}</span>;
}

function AdminAppointmentsSkeleton() {
  return (
    <div className="mt-6 grid gap-5">
      <div className="h-48 animate-pulse rounded-[2rem] bg-slate-200" />
      <div className="h-48 animate-pulse rounded-[2rem] bg-slate-200" />
    </div>
  );
}

function EmptyPanel({ description, title }: { description: string; title: string }) {
  return (
    <div className="mt-6 rounded-[2rem] border border-dashed border-slate-300 bg-white p-10 text-center shadow-sm">
      <h2 className="text-2xl font-black">{title}</h2>
      <p className="mt-2 text-slate-500">{description}</p>
    </div>
  );
}

function groupByLocalDate(appointments: AppointmentSummaryResponse[]) {
  const groups = new Map<string, AppointmentSummaryResponse[]>();
  for (const appointment of appointments) {
    groups.set(appointment.localDate, [...(groups.get(appointment.localDate) ?? []), appointment]);
  }

  return Array.from(groups.entries()).map(([localDate, grouped]) => ({ localDate, appointments: grouped }));
}

function defaultFilters(): AdminAppointmentFilters {
  const from = new Date();
  const to = addDays(from, 14);

  return {
    from: toDateInputValue(from),
    to: toDateInputValue(to),
  };
}

function cleanFilters(filters: AdminAppointmentFilters): AdminAppointmentFilters {
  return {
    from: filters.from,
    serviceId: filters.serviceId || undefined,
    staffMemberId: filters.staffMemberId || undefined,
    status: filters.status || undefined,
    to: filters.to,
  };
}

function addDays(date: Date, days: number) {
  const value = new Date(date);
  value.setDate(value.getDate() + days);
  return value;
}

function toDateInputValue(date: Date) {
  const month = String(date.getMonth() + 1).padStart(2, '0');
  const day = String(date.getDate()).padStart(2, '0');
  return `${date.getFullYear()}-${month}-${day}`;
}

function formatTime(value: string) {
  return value.slice(0, 5);
}

function formatDateLabel(value: string) {
  return new Intl.DateTimeFormat('es-ES', { dateStyle: 'full', timeZone: 'UTC' }).format(new Date(`${value}T00:00:00Z`));
}

function formatMoney(amount: number, currencyCode: string) {
  return new Intl.NumberFormat('es-ES', { currency: currencyCode, style: 'currency' }).format(amount);
}
