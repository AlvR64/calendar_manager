import { zodResolver } from '@hookform/resolvers/zod';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { useEffect, useMemo, useState, type ReactNode } from 'react';
import { useForm, useWatch } from 'react-hook-form';

import type { StaffMemberAvailabilityExceptionResponse, StaffMemberAvailabilityResponse, StaffMemberResponse } from '@/api/contracts';
import { getApiErrorMessage } from '@/api/apiErrors';
import { getAuthSession } from '@/auth/authStorage';
import { ApiErrorAlert, FieldError, inputClassName, labelClassName, primaryButtonClassName } from '@/features/auth/authUi';
import { getBusinessById } from '@/features/businesses/businessApi';
import {
  createStaffMemberAvailability,
  createStaffMemberAvailabilityException,
  deleteStaffMemberAvailability,
  deleteStaffMemberAvailabilityException,
  listStaffMemberAvailabilities,
  listStaffMemberAvailabilityExceptions,
  updateStaffMemberAvailability,
  updateStaffMemberAvailabilityException,
} from '@/features/availability/availabilityApi';
import {
  type AvailabilityExceptionFormValues,
  type WeeklyAvailabilityFormValues,
  availabilityExceptionSchema,
  emptyToNull,
  timeToApi,
  timeToInput,
  weeklyAvailabilitySchema,
} from '@/features/availability/availabilityValidation';
import { listStaffMembers } from '@/features/staffMembers/staffMemberApi';

const staffMembersQueryKey = ['admin', 'staff-members'] as const;
const businessQueryKey = (businessId: string) => ['admin', 'businesses', businessId] as const;
const availabilityQueryKey = (staffMemberId: string) => ['admin', 'staff-members', staffMemberId, 'availability'] as const;
const exceptionsQueryKey = (staffMemberId: string) => ['admin', 'staff-members', staffMemberId, 'availability-exceptions'] as const;

const dayOptions = [
  { label: 'Domingo', value: 0 },
  { label: 'Lunes', value: 1 },
  { label: 'Martes', value: 2 },
  { label: 'Miercoles', value: 3 },
  { label: 'Jueves', value: 4 },
  { label: 'Viernes', value: 5 },
  { label: 'Sabado', value: 6 },
];

export function AvailabilityPage() {
  const session = getAuthSession('Admin');
  const queryClient = useQueryClient();
  const [selectedStaffMemberId, setSelectedStaffMemberId] = useState('');
  const [editingAvailability, setEditingAvailability] = useState<StaffMemberAvailabilityResponse | null>(null);
  const [editingException, setEditingException] = useState<StaffMemberAvailabilityExceptionResponse | null>(null);
  const [successMessage, setSuccessMessage] = useState<string>();

  const staffMembersQuery = useQuery({
    queryFn: () => listStaffMembers(session!.token),
    queryKey: staffMembersQueryKey,
  });

  const businessQuery = useQuery({
    enabled: Boolean(session?.businessId),
    queryFn: () => getBusinessById(session!.businessId!),
    queryKey: businessQueryKey(session?.businessId ?? 'unknown'),
  });

  const staffMembers = staffMembersQuery.data ?? [];
  const activeStaffMemberId = staffMembers.some((staffMember) => staffMember.id === selectedStaffMemberId)
    ? selectedStaffMemberId
    : staffMembers[0]?.id ?? '';
  const selectedStaffMember = staffMembers.find((staffMember) => staffMember.id === activeStaffMemberId) ?? null;

  const availabilityQuery = useQuery({
    enabled: Boolean(activeStaffMemberId),
    queryFn: () => listStaffMemberAvailabilities(activeStaffMemberId, session!.token),
    queryKey: availabilityQueryKey(activeStaffMemberId || 'none'),
  });

  const exceptionsQuery = useQuery({
    enabled: Boolean(activeStaffMemberId),
    queryFn: () => listStaffMemberAvailabilityExceptions(activeStaffMemberId, session!.token),
    queryKey: exceptionsQueryKey(activeStaffMemberId || 'none'),
  });

  const availabilities = useMemo(
    () => [...(availabilityQuery.data ?? [])].sort((a, b) => a.dayOfWeek - b.dayOfWeek || a.startTime.localeCompare(b.startTime)),
    [availabilityQuery.data],
  );
  const exceptions = useMemo(
    () => [...(exceptionsQuery.data ?? [])].sort((a, b) => a.localDate.localeCompare(b.localDate) || (a.startTime ?? '').localeCompare(b.startTime ?? '')),
    [exceptionsQuery.data],
  );

  const weeklyForm = useForm<WeeklyAvailabilityFormValues>({
    defaultValues: toWeeklyFormValues(),
    resolver: zodResolver(weeklyAvailabilitySchema),
  });

  const exceptionForm = useForm<AvailabilityExceptionFormValues>({
    defaultValues: toExceptionFormValues(),
    resolver: zodResolver(availabilityExceptionSchema),
  });
  const exceptionIsClosed = useWatch({ control: exceptionForm.control, name: 'isClosed' });

  useEffect(() => {
    weeklyForm.reset(toWeeklyFormValues(editingAvailability ?? undefined));
  }, [editingAvailability, weeklyForm]);

  useEffect(() => {
    exceptionForm.reset(toExceptionFormValues(editingException ?? undefined));
  }, [editingException, exceptionForm]);

  const createAvailabilityMutation = useMutation({
    mutationFn: (values: WeeklyAvailabilityFormValues) => createStaffMemberAvailability(activeStaffMemberId, toWeeklyRequest(values), session!.token),
    onSuccess: () => {
      setSuccessMessage('Disponibilidad creada.');
      weeklyForm.reset(toWeeklyFormValues());
      queryClient.invalidateQueries({ queryKey: availabilityQueryKey(activeStaffMemberId) });
    },
  });

  const updateAvailabilityMutation = useMutation({
    mutationFn: (values: WeeklyAvailabilityFormValues) => updateStaffMemberAvailability(activeStaffMemberId, editingAvailability!.id, toWeeklyRequest(values), session!.token),
    onSuccess: () => {
      setSuccessMessage('Disponibilidad actualizada.');
      setEditingAvailability(null);
      weeklyForm.reset(toWeeklyFormValues());
      queryClient.invalidateQueries({ queryKey: availabilityQueryKey(activeStaffMemberId) });
    },
  });

  const deleteAvailabilityMutation = useMutation({
    mutationFn: (availabilityId: string) => deleteStaffMemberAvailability(activeStaffMemberId, availabilityId, session!.token),
    onSuccess: () => {
      setSuccessMessage('Disponibilidad eliminada.');
      setEditingAvailability(null);
      queryClient.invalidateQueries({ queryKey: availabilityQueryKey(activeStaffMemberId) });
    },
  });

  const createExceptionMutation = useMutation({
    mutationFn: (values: AvailabilityExceptionFormValues) => createStaffMemberAvailabilityException(activeStaffMemberId, toExceptionRequest(values), session!.token),
    onSuccess: () => {
      setSuccessMessage('Excepcion creada.');
      exceptionForm.reset(toExceptionFormValues());
      queryClient.invalidateQueries({ queryKey: exceptionsQueryKey(activeStaffMemberId) });
    },
  });

  const updateExceptionMutation = useMutation({
    mutationFn: (values: AvailabilityExceptionFormValues) => updateStaffMemberAvailabilityException(activeStaffMemberId, editingException!.id, toExceptionRequest(values), session!.token),
    onSuccess: () => {
      setSuccessMessage('Excepcion actualizada.');
      setEditingException(null);
      exceptionForm.reset(toExceptionFormValues());
      queryClient.invalidateQueries({ queryKey: exceptionsQueryKey(activeStaffMemberId) });
    },
  });

  const deleteExceptionMutation = useMutation({
    mutationFn: (exceptionId: string) => deleteStaffMemberAvailabilityException(activeStaffMemberId, exceptionId, session!.token),
    onSuccess: () => {
      setSuccessMessage('Excepcion eliminada.');
      setEditingException(null);
      queryClient.invalidateQueries({ queryKey: exceptionsQueryKey(activeStaffMemberId) });
    },
  });

  function handleStaffMemberChange(staffMemberId: string) {
    setSelectedStaffMemberId(staffMemberId);
    setEditingAvailability(null);
    setEditingException(null);
    setSuccessMessage(undefined);
  }

  function handleWeeklySubmit(values: WeeklyAvailabilityFormValues) {
    setSuccessMessage(undefined);
    if (!activeStaffMemberId) {
      return;
    }

    if (editingAvailability) {
      updateAvailabilityMutation.mutate(values);
      return;
    }

    createAvailabilityMutation.mutate(values);
  }

  function handleExceptionSubmit(values: AvailabilityExceptionFormValues) {
    setSuccessMessage(undefined);
    if (!activeStaffMemberId) {
      return;
    }

    if (editingException) {
      updateExceptionMutation.mutate(values);
      return;
    }

    createExceptionMutation.mutate(values);
  }

  function handleDeleteAvailability(availability: StaffMemberAvailabilityResponse) {
    setSuccessMessage(undefined);
    if (!window.confirm(`Eliminar disponibilidad de ${dayLabel(availability.dayOfWeek)} ${formatTime(availability.startTime)}-${formatTime(availability.endTime)}?`)) {
      return;
    }

    deleteAvailabilityMutation.mutate(availability.id);
  }

  function handleDeleteException(exception: StaffMemberAvailabilityExceptionResponse) {
    setSuccessMessage(undefined);
    if (!window.confirm(`Eliminar excepcion del ${exception.localDate}?`)) {
      return;
    }

    deleteExceptionMutation.mutate(exception.id);
  }

  const mutationError =
    createAvailabilityMutation.error ??
    updateAvailabilityMutation.error ??
    deleteAvailabilityMutation.error ??
    createExceptionMutation.error ??
    updateExceptionMutation.error ??
    deleteExceptionMutation.error;
  const isSavingWeekly = createAvailabilityMutation.isPending || updateAvailabilityMutation.isPending;
  const isSavingException = createExceptionMutation.isPending || updateExceptionMutation.isPending;

  if (staffMembersQuery.isPending || businessQuery.isPending) {
    return <AvailabilitySkeleton />;
  }

  if (staffMembersQuery.isError || businessQuery.isError) {
    return (
      <AvailabilityFrame activeStaffMember={null} timeZoneId="">
        <ApiErrorAlert message={getApiErrorMessage(staffMembersQuery.error ?? businessQuery.error)} />
      </AvailabilityFrame>
    );
  }

  if (staffMembers.length === 0) {
    return (
      <AvailabilityFrame activeStaffMember={null} timeZoneId={businessQuery.data.timeZoneId}>
        <div className="rounded-[2rem] border border-dashed border-slate-300 bg-white p-10 text-center shadow-sm">
          <h2 className="text-2xl font-black">Aun no hay staff members</h2>
          <p className="mt-2 text-slate-500">Crea un staff member antes de configurar disponibilidad.</p>
        </div>
      </AvailabilityFrame>
    );
  }

  const isLoadingSelectedData = availabilityQuery.isPending || exceptionsQuery.isPending;

  return (
    <AvailabilityFrame activeStaffMember={selectedStaffMember} timeZoneId={businessQuery.data.timeZoneId}>
      <div className="mb-6 rounded-[2rem] border border-slate-200 bg-white p-5 shadow-sm md:p-6">
        <label className={labelClassName}>
          Staff member
          <select className={inputClassName} onChange={(event) => handleStaffMemberChange(event.target.value)} value={activeStaffMemberId}>
            {staffMembers.map((staffMember) => (
              <option key={staffMember.id} value={staffMember.id}>
                {staffMember.displayName}{staffMember.isActive ? '' : ' (inactive)'}
              </option>
            ))}
          </select>
        </label>
      </div>

      <ApiErrorAlert message={mutationError ? getApiErrorMessage(mutationError) : undefined} />
      {successMessage ? <SuccessAlert message={successMessage} /> : null}

      {availabilityQuery.isError || exceptionsQuery.isError ? (
        <div className="mt-4">
          <ApiErrorAlert message={getApiErrorMessage(availabilityQuery.error ?? exceptionsQuery.error)} />
        </div>
      ) : isLoadingSelectedData ? (
        <div className="mt-6 grid gap-6 xl:grid-cols-2">
          <div className="h-[520px] animate-pulse rounded-[2rem] bg-slate-200" />
          <div className="h-[520px] animate-pulse rounded-[2rem] bg-slate-200" />
        </div>
      ) : (
        <div className="mt-6 grid gap-6 xl:grid-cols-2">
          <section className="rounded-[2rem] border border-slate-200 bg-white p-5 shadow-sm md:p-6">
            <SectionHeader eyebrow="Weekly" title="Disponibilidad semanal" description="Bloques recurrentes en horario local del business." />
            <div className="mt-5 grid gap-3">
              {availabilities.length === 0 ? (
                <EmptyPanel title="Sin disponibilidad semanal" description="Crea al menos un bloque para que el staff member pueda recibir slots." />
              ) : (
                availabilities.map((availability) => (
                  <article className="rounded-2xl border border-slate-200 p-4" key={availability.id}>
                    <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
                      <div>
                        <h3 className="text-lg font-black">{dayLabel(availability.dayOfWeek)}</h3>
                        <p className="text-sm font-bold text-slate-500">{formatTime(availability.startTime)} - {formatTime(availability.endTime)}</p>
                      </div>
                      <div className="flex flex-wrap gap-2">
                        <button className="rounded-xl border border-slate-200 px-3 py-2 text-sm font-black text-slate-700 hover:bg-slate-50" onClick={() => setEditingAvailability(availability)} type="button">
                          Editar
                        </button>
                        <button className="rounded-xl border border-red-200 px-3 py-2 text-sm font-black text-red-700 hover:bg-red-50 disabled:opacity-50" disabled={deleteAvailabilityMutation.isPending} onClick={() => handleDeleteAvailability(availability)} type="button">
                          Eliminar
                        </button>
                      </div>
                    </div>
                  </article>
                ))
              )}
            </div>

            <form className="mt-6 rounded-[1.5rem] border border-slate-100 bg-slate-50 p-4" onSubmit={weeklyForm.handleSubmit(handleWeeklySubmit)}>
              <h3 className="text-lg font-black">{editingAvailability ? 'Editar bloque semanal' : 'Nuevo bloque semanal'}</h3>
              <div className="mt-4 grid gap-4 md:grid-cols-3">
                <label className={labelClassName}>
                  Dia
                  <select className={inputClassName} {...weeklyForm.register('dayOfWeek', { valueAsNumber: true })}>
                    {dayOptions.map((day) => (
                      <option key={day.value} value={day.value}>{day.label}</option>
                    ))}
                  </select>
                  <FieldError message={weeklyForm.formState.errors.dayOfWeek?.message} />
                </label>
                <label className={labelClassName}>
                  Start time
                  <input className={inputClassName} type="time" {...weeklyForm.register('startTime')} />
                  <FieldError message={weeklyForm.formState.errors.startTime?.message} />
                </label>
                <label className={labelClassName}>
                  End time
                  <input className={inputClassName} type="time" {...weeklyForm.register('endTime')} />
                  <FieldError message={weeklyForm.formState.errors.endTime?.message} />
                </label>
              </div>
              <div className="mt-5 grid gap-3 sm:grid-cols-2">
                {editingAvailability ? (
                  <button className="rounded-2xl border border-slate-200 px-5 py-3.5 text-base font-black text-slate-700 transition hover:bg-white" onClick={() => setEditingAvailability(null)} type="button">
                    Cancelar edicion
                  </button>
                ) : null}
                <button className={editingAvailability ? primaryButtonClassName : `${primaryButtonClassName} sm:col-span-2`} disabled={isSavingWeekly} type="submit">
                  {isSavingWeekly ? 'Guardando...' : editingAvailability ? 'Guardar bloque' : 'Crear bloque'}
                </button>
              </div>
            </form>
          </section>

          <section className="rounded-[2rem] border border-slate-200 bg-white p-5 shadow-sm md:p-6">
            <SectionHeader eyebrow="Exceptions" title="Excepciones por fecha" description="Cierres completos o horarios especiales para un dia concreto." />
            <div className="mt-5 grid gap-3">
              {exceptions.length === 0 ? (
                <EmptyPanel title="Sin excepciones" description="Anade vacaciones, ausencias o horarios especiales cuando haga falta." />
              ) : (
                exceptions.map((exception) => (
                  <article className="rounded-2xl border border-slate-200 p-4" key={exception.id}>
                    <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
                      <div>
                        <div className="flex flex-wrap items-center gap-2">
                          <h3 className="text-lg font-black">{exception.localDate}</h3>
                          <span className={exception.isClosed ? 'rounded-full bg-red-100 px-3 py-1 text-xs font-black text-red-700' : 'rounded-full bg-indigo-100 px-3 py-1 text-xs font-black text-indigo-700'}>
                            {exception.isClosed ? 'Closed day' : 'Special hours'}
                          </span>
                        </div>
                        <p className="mt-1 text-sm font-bold text-slate-500">
                          {exception.isClosed ? 'Cerrado todo el dia' : `${formatTime(exception.startTime)} - ${formatTime(exception.endTime)}`}
                        </p>
                        {exception.reason ? <p className="mt-1 text-sm text-slate-500">{exception.reason}</p> : null}
                      </div>
                      <div className="flex flex-wrap gap-2">
                        <button className="rounded-xl border border-slate-200 px-3 py-2 text-sm font-black text-slate-700 hover:bg-slate-50" onClick={() => setEditingException(exception)} type="button">
                          Editar
                        </button>
                        <button className="rounded-xl border border-red-200 px-3 py-2 text-sm font-black text-red-700 hover:bg-red-50 disabled:opacity-50" disabled={deleteExceptionMutation.isPending} onClick={() => handleDeleteException(exception)} type="button">
                          Eliminar
                        </button>
                      </div>
                    </div>
                  </article>
                ))
              )}
            </div>

            <form className="mt-6 rounded-[1.5rem] border border-slate-100 bg-slate-50 p-4" onSubmit={exceptionForm.handleSubmit(handleExceptionSubmit)}>
              <h3 className="text-lg font-black">{editingException ? 'Editar excepcion' : 'Nueva excepcion'}</h3>
              <div className="mt-4 grid gap-4 md:grid-cols-2">
                <label className={labelClassName}>
                  Local date
                  <input className={inputClassName} type="date" {...exceptionForm.register('localDate')} />
                  <FieldError message={exceptionForm.formState.errors.localDate?.message} />
                </label>
                <label className={`${labelClassName} flex items-center gap-3 rounded-2xl border border-slate-200 bg-white px-4 py-3`}>
                  <input className="h-5 w-5 rounded border-slate-300" type="checkbox" {...exceptionForm.register('isClosed')} />
                  Closed all day
                </label>
                <label className={labelClassName}>
                  Start time
                  <input className={inputClassName} disabled={exceptionIsClosed} type="time" {...exceptionForm.register('startTime')} />
                  <FieldError message={exceptionForm.formState.errors.startTime?.message} />
                </label>
                <label className={labelClassName}>
                  End time
                  <input className={inputClassName} disabled={exceptionIsClosed} type="time" {...exceptionForm.register('endTime')} />
                  <FieldError message={exceptionForm.formState.errors.endTime?.message} />
                </label>
              </div>
              <label className={`${labelClassName} mt-4`}>
                Reason
                <input className={inputClassName} {...exceptionForm.register('reason')} />
                <FieldError message={exceptionForm.formState.errors.reason?.message} />
              </label>
              <div className="mt-5 grid gap-3 sm:grid-cols-2">
                {editingException ? (
                  <button className="rounded-2xl border border-slate-200 px-5 py-3.5 text-base font-black text-slate-700 transition hover:bg-white" onClick={() => setEditingException(null)} type="button">
                    Cancelar edicion
                  </button>
                ) : null}
                <button className={editingException ? primaryButtonClassName : `${primaryButtonClassName} sm:col-span-2`} disabled={isSavingException} type="submit">
                  {isSavingException ? 'Guardando...' : editingException ? 'Guardar excepcion' : 'Crear excepcion'}
                </button>
              </div>
            </form>
          </section>
        </div>
      )}
    </AvailabilityFrame>
  );
}

function AvailabilityFrame({ activeStaffMember, children, timeZoneId }: { activeStaffMember: StaffMemberResponse | null; children: ReactNode; timeZoneId: string }) {
  return (
    <div>
      <div className="mb-8 flex flex-col gap-4 md:flex-row md:items-end md:justify-between">
        <div>
          <p className="text-sm font-black uppercase tracking-[0.22em] text-indigo-600">Admin availability</p>
          <h1 className="mt-3 text-4xl font-black tracking-tight text-slate-950">Configura disponibilidad</h1>
          <p className="mt-3 max-w-3xl text-slate-600">Define horarios semanales y excepciones por staff member. Los horarios se interpretan en timezone del business.</p>
        </div>
        <div className="rounded-2xl border border-indigo-100 bg-indigo-50 px-5 py-3 text-sm font-black text-indigo-700 shadow-sm">
          {timeZoneId || 'Timezone'}{activeStaffMember ? ` · ${activeStaffMember.displayName}` : ''}
        </div>
      </div>
      {children}
    </div>
  );
}

function AvailabilitySkeleton() {
  return (
    <AvailabilityFrame activeStaffMember={null} timeZoneId="">
      <div className="h-28 animate-pulse rounded-[2rem] bg-slate-200" />
      <div className="mt-6 grid gap-6 xl:grid-cols-2">
        <div className="h-[520px] animate-pulse rounded-[2rem] bg-slate-200" />
        <div className="h-[520px] animate-pulse rounded-[2rem] bg-slate-200" />
      </div>
    </AvailabilityFrame>
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
    <div className="rounded-[1.5rem] border border-dashed border-slate-300 bg-slate-50 p-6 text-center">
      <h3 className="text-lg font-black">{title}</h3>
      <p className="mt-2 text-sm text-slate-500">{description}</p>
    </div>
  );
}

function SuccessAlert({ message }: { message: string }) {
  return <div className="mt-4 rounded-2xl border border-emerald-200 bg-emerald-50 px-4 py-3 text-sm font-bold text-emerald-700">{message}</div>;
}

function toWeeklyFormValues(availability?: StaffMemberAvailabilityResponse): WeeklyAvailabilityFormValues {
  return {
    dayOfWeek: availability?.dayOfWeek ?? 1,
    endTime: timeToInput(availability?.endTime) || '17:00',
    startTime: timeToInput(availability?.startTime) || '09:00',
  };
}

function toWeeklyRequest(values: WeeklyAvailabilityFormValues) {
  return {
    dayOfWeek: values.dayOfWeek,
    endTime: timeToApi(values.endTime),
    startTime: timeToApi(values.startTime),
  };
}

function toExceptionFormValues(exception?: StaffMemberAvailabilityExceptionResponse): AvailabilityExceptionFormValues {
  return {
    endTime: timeToInput(exception?.endTime),
    isClosed: exception?.isClosed ?? true,
    localDate: exception?.localDate ?? '',
    reason: exception?.reason ?? '',
    startTime: timeToInput(exception?.startTime),
  };
}

function toExceptionRequest(values: AvailabilityExceptionFormValues) {
  return {
    endTime: values.isClosed ? null : timeToApi(values.endTime ?? ''),
    isClosed: values.isClosed,
    localDate: values.localDate,
    reason: emptyToNull(values.reason),
    startTime: values.isClosed ? null : timeToApi(values.startTime ?? ''),
  };
}

function dayLabel(dayOfWeek: number) {
  return dayOptions.find((day) => day.value === dayOfWeek)?.label ?? `Dia ${dayOfWeek}`;
}

function formatTime(value?: string | null) {
  return timeToInput(value) || '--:--';
}
