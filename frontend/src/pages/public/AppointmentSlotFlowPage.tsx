import { useQuery } from '@tanstack/react-query';
import type { ReactNode } from 'react';
import { useState } from 'react';
import { Link, useParams } from 'react-router-dom';

import type { AvailableSlotResponse, BusinessProfileResponse, BusinessServiceResponse, BusinessStaffMemberResponse } from '@/api/contracts';
import { getApiErrorMessage } from '@/api/apiErrors';
import { ApiError } from '@/api/httpClient';
import { ApiErrorAlert, inputClassName, labelClassName } from '@/features/auth/authUi';
import { getPublicBusinessProfileBySlug, listPublicAvailableSlots } from '@/features/publicBusiness/publicBusinessApi';
import { routes } from '@/lib/routes';

const businessProfileQueryKey = (slug: string) => ['public', 'business-profile', slug] as const;
const availableSlotsQueryKey = (businessId: string, serviceId: string, date: string, staffMemberId: string) => [
  'public',
  'available-slots',
  businessId,
  serviceId,
  date,
  staffMemberId,
] as const;

export function AppointmentSlotFlowPage() {
  const { slug } = useParams();
  const [selectedServiceId, setSelectedServiceId] = useState('');
  const [selectedStaffMemberId, setSelectedStaffMemberId] = useState('any');
  const [selectedDate, setSelectedDate] = useState('');
  const [selectedSlotKey, setSelectedSlotKey] = useState('');

  const profileQuery = useQuery({
    enabled: Boolean(slug),
    queryFn: () => getPublicBusinessProfileBySlug(slug!),
    queryKey: businessProfileQueryKey(slug ?? 'missing'),
  });

  const profile = profileQuery.data;
  const business = profile?.business;
  const services = profile?.services ?? [];
  const staffMembers = profile?.staffMembers ?? [];
  const today = getTodayInTimeZone(business?.timeZoneId ?? 'UTC');
  const maxDate = business ? addDays(today, business.maxAdvanceBookingDays) : today;
  const activeDate = selectedDate || today;
  const activeServiceId = services.some((service) => service.id === selectedServiceId) ? selectedServiceId : services[0]?.id ?? '';
  const activeService = services.find((service) => service.id === activeServiceId) ?? null;
  const staffForService = profile ? getStaffForService(profile, activeServiceId) : [];
  const activeStaffMemberId = selectedStaffMemberId !== 'any' && staffForService.some((staffMember) => staffMember.id === selectedStaffMemberId)
    ? selectedStaffMemberId
    : 'any';
  const selectedStaffMember = staffMembers.find((staffMember) => staffMember.id === activeStaffMemberId) ?? null;
  const isDateWithinWindow = activeDate >= today && activeDate <= maxDate;

  const slotsQuery = useQuery({
    enabled: Boolean(business?.id && activeServiceId && activeDate && isDateWithinWindow && profileQuery.isSuccess),
    queryFn: () => listPublicAvailableSlots(business!.id, activeServiceId, activeDate, activeStaffMemberId === 'any' ? undefined : activeStaffMemberId),
    queryKey: availableSlotsQueryKey(business?.id ?? 'none', activeServiceId || 'none', activeDate || 'none', activeStaffMemberId),
  });

  const slots = slotsQuery.data ?? [];
  const selectedSlot = slots.find((slot) => slotKey(slot) === selectedSlotKey) ?? null;

  if (!slug) {
    return <SlotFlowNotFound />;
  }

  if (profileQuery.isPending) {
    return <SlotFlowSkeleton />;
  }

  if (profileQuery.isError) {
    if (profileQuery.error instanceof ApiError && profileQuery.error.status === 404) {
      return <SlotFlowNotFound />;
    }

    return (
      <SlotFlowShell>
        <ApiErrorAlert message={getApiErrorMessage(profileQuery.error)} />
      </SlotFlowShell>
    );
  }

  if (!business || !profile) {
    return <SlotFlowNotFound />;
  }

  function handleServiceChange(serviceId: string) {
    setSelectedServiceId(serviceId);
    setSelectedStaffMemberId('any');
    setSelectedSlotKey('');
  }

  function handleStaffMemberChange(staffMemberId: string) {
    setSelectedStaffMemberId(staffMemberId);
    setSelectedSlotKey('');
  }

  function handleDateChange(date: string) {
    setSelectedDate(date);
    setSelectedSlotKey('');
  }

  return (
    <SlotFlowShell>
      <header className="rounded-[2rem] border border-white/80 bg-white/90 p-6 shadow-2xl shadow-indigo-100/70 backdrop-blur md:p-8">
        <Link className="text-sm font-black text-indigo-700 hover:text-indigo-900" to={routes.businessProfile(slug)}>
          {business.name}
        </Link>
        <p className="mt-10 text-sm font-black uppercase tracking-[0.25em] text-indigo-600">Public appointment</p>
        <h1 className="mt-4 text-5xl font-black leading-[0.95] tracking-tight text-slate-950 md:text-6xl">Elige un slot disponible</h1>
        <p className="mt-5 max-w-3xl text-lg leading-8 text-slate-600">
          Selecciona service, staff member opcional y fecha. El flujo se detiene en la seleccion local del slot hasta que exista creacion de appointment.
        </p>
      </header>

      {services.length === 0 ? (
        <EmptyPanel description="Este business todavia no publico services activos para appointments." title="Sin services disponibles" />
      ) : (
        <div className="mt-8 grid gap-8 lg:grid-cols-[390px_1fr] lg:items-start">
          <aside className="rounded-[2rem] border border-slate-200 bg-white p-6 shadow-sm md:p-7">
            <SectionHeader description="Define el contexto antes de pedir slots al backend." eyebrow="Paso 1" title="Preferencias" />
            <div className="mt-5 grid gap-5">
              <label className={labelClassName}>
                Service
                <select className={inputClassName} onChange={(event) => handleServiceChange(event.target.value)} value={activeServiceId}>
                  {services.map((service) => (
                    <option key={service.id} value={service.id}>{service.name}</option>
                  ))}
                </select>
              </label>
              <label className={labelClassName}>
                Staff member opcional
                <select className={inputClassName} onChange={(event) => handleStaffMemberChange(event.target.value)} value={activeStaffMemberId}>
                  <option value="any">Cualquier staff member</option>
                  {staffForService.map((staffMember) => (
                    <option key={staffMember.id} value={staffMember.id}>{staffMember.displayName}</option>
                  ))}
                </select>
              </label>
              {staffMembers.length === 0 ? <p className="rounded-2xl bg-amber-50 px-4 py-3 text-sm font-bold text-amber-800">Este business no tiene staff members publicos.</p> : null}
              {activeService && staffForService.length === 0 ? <p className="rounded-2xl bg-amber-50 px-4 py-3 text-sm font-bold text-amber-800">Este service no tiene staff asignado publicamente.</p> : null}
              <label className={labelClassName}>
                Fecha
                <input className={inputClassName} max={maxDate} min={today} onChange={(event) => handleDateChange(event.target.value)} type="date" value={activeDate} />
              </label>
              <div className="rounded-2xl bg-slate-950 px-4 py-4 text-sm font-bold text-white">
                <p>Timezone: {business.timeZoneId}</p>
                <p className="mt-1 text-slate-300">Ventana: {today} hasta {maxDate}</p>
              </div>
            </div>
          </aside>

          <section className="rounded-[2rem] border border-slate-200 bg-white p-6 shadow-sm md:p-7">
            <SectionHeader description="Slots calculados por backend con disponibilidad, excepciones y appointments existentes." eyebrow="Paso 2" title="Slots disponibles" />
            <SelectionSummary date={activeDate} service={activeService} slot={selectedSlot} staffMember={selectedStaffMember} />

            {!isDateWithinWindow ? (
              <EmptyPanel description={`Selecciona una fecha entre ${today} y ${maxDate}.`} title="Fecha fuera de ventana" />
            ) : slotsQuery.isPending ? (
              <div className="mt-5 grid gap-3 sm:grid-cols-2 xl:grid-cols-3">
                {Array.from({ length: 6 }).map((_, index) => <div className="h-24 animate-pulse rounded-2xl bg-slate-200" key={index} />)}
              </div>
            ) : slotsQuery.isError ? (
              <div className="mt-5">
                <ApiErrorAlert message={getApiErrorMessage(slotsQuery.error)} />
              </div>
            ) : slots.length === 0 ? (
              <EmptyPanel description="Prueba otra fecha, otro service o un staff member distinto." title="No hay slots para esta fecha" />
            ) : (
              <div className="mt-5 grid gap-3 sm:grid-cols-2 xl:grid-cols-3">
                {slots.map((slot) => {
                  const key = slotKey(slot);
                  const staffMember = staffMembers.find((item) => item.id === slot.staffMemberId);
                  const isSelected = key === selectedSlotKey;

                  return (
                    <button
                      className={isSelected ? 'rounded-2xl border-2 border-indigo-600 bg-indigo-50 px-4 py-4 text-left shadow-sm' : 'rounded-2xl border border-slate-200 bg-slate-50 px-4 py-4 text-left shadow-sm transition hover:border-indigo-200 hover:bg-indigo-50'}
                      key={key}
                      onClick={() => setSelectedSlotKey(key)}
                      type="button"
                    >
                      <span className="text-xl font-black text-slate-950">{formatTime(slot.startTime)} - {formatTime(slot.endTime)}</span>
                      <span className="mt-2 block text-sm font-bold text-slate-500">{staffMember?.displayName ?? 'Staff member'}</span>
                      <span className="mt-1 block text-xs font-bold text-slate-400">UTC {formatUtc(slot.startAtUtc)}</span>
                    </button>
                  );
                })}
              </div>
            )}
          </section>
        </div>
      )}
    </SlotFlowShell>
  );
}

function SlotFlowShell({ children }: { children: ReactNode }) {
  return <main className="min-h-screen bg-[radial-gradient(circle_at_top_left,#e0e7ff,transparent_32%),linear-gradient(135deg,#ffffff_0%,#f8fafc_55%,#eef2ff_100%)] px-6 py-8 text-slate-950"><div className="mx-auto max-w-6xl">{children}</div></main>;
}

function SlotFlowSkeleton() {
  return (
    <SlotFlowShell>
      <div className="h-80 animate-pulse rounded-[2rem] bg-slate-200" />
      <div className="mt-8 grid gap-8 lg:grid-cols-[390px_1fr]">
        <div className="h-96 animate-pulse rounded-[2rem] bg-slate-200" />
        <div className="h-96 animate-pulse rounded-[2rem] bg-slate-200" />
      </div>
    </SlotFlowShell>
  );
}

function SlotFlowNotFound() {
  return (
    <SlotFlowShell>
      <div className="rounded-[2rem] border border-slate-200 bg-white p-10 text-center shadow-sm">
        <p className="text-sm font-black uppercase tracking-[0.25em] text-indigo-600">Business no encontrado</p>
        <h1 className="mt-4 text-4xl font-black tracking-tight text-slate-950">No podemos cargar este flujo</h1>
        <p className="mx-auto mt-4 max-w-xl text-slate-600">Puede que el slug no exista o que el business no este activo publicamente.</p>
        <Link className="mt-7 inline-flex rounded-2xl bg-slate-950 px-6 py-3.5 text-base font-black text-white hover:bg-indigo-700" to={routes.home}>
          Volver al inicio
        </Link>
      </div>
    </SlotFlowShell>
  );
}

function SelectionSummary({ date, service, slot, staffMember }: { date: string; service: BusinessServiceResponse | null; slot: AvailableSlotResponse | null; staffMember: BusinessStaffMemberResponse | null }) {
  return (
    <div className="mt-5 rounded-[1.5rem] border border-indigo-100 bg-indigo-50 p-4 text-sm font-bold text-indigo-950">
      <p>Service: {service?.name ?? 'Sin service'}</p>
      <p className="mt-1">Staff: {staffMember?.displayName ?? 'Cualquier staff member'}</p>
      <p className="mt-1">Fecha: {date}</p>
      {slot ? <p className="mt-3 rounded-2xl bg-white px-4 py-3 text-indigo-700">Slot seleccionado: {formatTime(slot.startTime)} - {formatTime(slot.endTime)}. No se crea appointment todavia.</p> : null}
    </div>
  );
}

function SectionHeader({ description, eyebrow, title }: { description: string; eyebrow: string; title: string }) {
  return (
    <div className="border-b border-slate-100 pb-5">
      <p className="text-sm font-black uppercase tracking-[0.2em] text-indigo-600">{eyebrow}</p>
      <h2 className="mt-2 text-2xl font-black tracking-tight">{title}</h2>
      <p className="mt-2 text-sm text-slate-500">{description}</p>
    </div>
  );
}

function EmptyPanel({ description, title }: { description: string; title: string }) {
  return (
    <div className="mt-5 rounded-[1.5rem] border border-dashed border-slate-300 bg-slate-50 p-6 text-center">
      <h3 className="text-lg font-black">{title}</h3>
      <p className="mt-2 text-sm text-slate-500">{description}</p>
    </div>
  );
}

function getStaffForService(profile: BusinessProfileResponse, serviceId: string) {
  return profile.staffMembers.filter((staffMember) => profile.assignments.some((assignment) => assignment.serviceId === serviceId && assignment.staffMemberId === staffMember.id));
}

function getTodayInTimeZone(timeZoneId: string) {
  try {
    const parts = new Intl.DateTimeFormat('en-US', { day: '2-digit', month: '2-digit', timeZone: timeZoneId, year: 'numeric' }).formatToParts(new Date());
    const year = parts.find((part) => part.type === 'year')?.value;
    const month = parts.find((part) => part.type === 'month')?.value;
    const day = parts.find((part) => part.type === 'day')?.value;

    if (year && month && day) {
      return `${year}-${month}-${day}`;
    }
  } catch {
    // Fall back to UTC if the browser cannot resolve the business timezone.
  }

  return new Date().toISOString().slice(0, 10);
}

function addDays(dateString: string, days: number) {
  const [year, month, day] = dateString.split('-').map(Number);
  const date = new Date(Date.UTC(year, month - 1, day + days));
  return date.toISOString().slice(0, 10);
}

function slotKey(slot: AvailableSlotResponse) {
  return `${slot.staffMemberId}-${slot.localDate}-${slot.startTime}-${slot.endTime}`;
}

function formatTime(value: string) {
  return value.slice(0, 5);
}

function formatUtc(value: string) {
  return new Date(value).toISOString().slice(11, 16);
}
