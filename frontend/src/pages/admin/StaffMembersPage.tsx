import { zodResolver } from '@hookform/resolvers/zod';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { useEffect, useMemo, useState, type ReactNode } from 'react';
import { useForm } from 'react-hook-form';

import type { StaffMemberResponse } from '@/api/contracts';
import { getApiErrorMessage } from '@/api/apiErrors';
import { getAuthSession } from '@/auth/authStorage';
import { ApiErrorAlert, FieldError, inputClassName, labelClassName, primaryButtonClassName } from '@/features/auth/authUi';
import {
  createStaffMember,
  deleteStaffMember,
  listStaffMembers,
  updateStaffMember,
  updateStaffMemberActiveState,
} from '@/features/staffMembers/staffMemberApi';
import { type StaffMemberFormValues, emptyToNull, staffMemberFormSchema } from '@/features/staffMembers/staffMemberValidation';

const staffMembersQueryKey = ['admin', 'staff-members'] as const;

export function StaffMembersPage() {
  const session = getAuthSession('Admin');
  const queryClient = useQueryClient();
  const [editingStaffMember, setEditingStaffMember] = useState<StaffMemberResponse | null>(null);
  const [successMessage, setSuccessMessage] = useState<string>();

  const staffMembersQuery = useQuery({
    queryFn: () => listStaffMembers(session!.token),
    queryKey: staffMembersQueryKey,
  });

  const staffMembers = useMemo(
    () => [...(staffMembersQuery.data ?? [])].sort((a, b) => a.sortOrder - b.sortOrder || a.displayName.localeCompare(b.displayName)),
    [staffMembersQuery.data],
  );
  const activeStaffMembersCount = staffMembers.filter((staffMember) => staffMember.isActive).length;

  const form = useForm<StaffMemberFormValues>({
    defaultValues: toFormValues(),
    resolver: zodResolver(staffMemberFormSchema),
  });

  useEffect(() => {
    form.reset(toFormValues(editingStaffMember ?? undefined));
  }, [editingStaffMember, form]);

  const createMutation = useMutation({
    mutationFn: (values: StaffMemberFormValues) => createStaffMember(toRequest(values), session!.token),
    onSuccess: () => {
      setSuccessMessage('Staff member creado.');
      setEditingStaffMember(null);
      form.reset(toFormValues());
      queryClient.invalidateQueries({ queryKey: staffMembersQueryKey });
    },
  });

  const updateMutation = useMutation({
    mutationFn: (values: StaffMemberFormValues) => updateStaffMember(editingStaffMember!.id, toRequest(values), session!.token),
    onSuccess: () => {
      setSuccessMessage('Staff member actualizado.');
      setEditingStaffMember(null);
      form.reset(toFormValues());
      queryClient.invalidateQueries({ queryKey: staffMembersQueryKey });
    },
  });

  const activeStateMutation = useMutation({
    mutationFn: ({ isActive, staffMemberId }: { isActive: boolean; staffMemberId: string }) =>
      updateStaffMemberActiveState(staffMemberId, { isActive }, session!.token),
    onSuccess: () => {
      setSuccessMessage('Estado del staff member actualizado.');
      queryClient.invalidateQueries({ queryKey: staffMembersQueryKey });
    },
  });

  const deleteMutation = useMutation({
    mutationFn: (staffMemberId: string) => deleteStaffMember(staffMemberId, session!.token),
    onSuccess: () => {
      setSuccessMessage('Staff member eliminado.');
      if (editingStaffMember) {
        setEditingStaffMember(null);
        form.reset(toFormValues());
      }
      queryClient.invalidateQueries({ queryKey: staffMembersQueryKey });
    },
  });

  function handleSubmit(values: StaffMemberFormValues) {
    setSuccessMessage(undefined);
    if (editingStaffMember) {
      updateMutation.mutate(values);
      return;
    }

    createMutation.mutate(values);
  }

  function handleDelete(staffMember: StaffMemberResponse) {
    setSuccessMessage(undefined);
    if (!window.confirm(`Eliminar ${staffMember.displayName}?`)) {
      return;
    }

    deleteMutation.mutate(staffMember.id);
  }

  const mutationError = createMutation.error ?? updateMutation.error ?? activeStateMutation.error ?? deleteMutation.error;
  const isSaving = createMutation.isPending || updateMutation.isPending;

  if (staffMembersQuery.isPending) {
    return <StaffMembersSkeleton />;
  }

  if (staffMembersQuery.isError) {
    return (
      <StaffMembersFrame activeStaffMembersCount={0} totalStaffMembersCount={0}>
        <ApiErrorAlert message={getApiErrorMessage(staffMembersQuery.error)} />
      </StaffMembersFrame>
    );
  }

  return (
    <StaffMembersFrame activeStaffMembersCount={activeStaffMembersCount} totalStaffMembersCount={staffMembers.length}>
      <section className="grid gap-6 xl:grid-cols-[1.1fr_0.9fr]">
        <div className="rounded-[2rem] border border-slate-200 bg-white p-5 shadow-sm md:p-6">
          <div className="flex flex-col gap-3 border-b border-slate-100 pb-5 md:flex-row md:items-end md:justify-between">
            <div>
              <p className="text-sm font-black uppercase tracking-[0.2em] text-indigo-600">Staff members</p>
              <h2 className="mt-2 text-2xl font-black tracking-tight">Equipo del business</h2>
              <p className="mt-2 text-sm text-slate-500">Incluye staff members activos e inactivos para admin.</p>
            </div>
            <button
              className="rounded-2xl border border-slate-200 px-4 py-3 text-sm font-black text-slate-700 transition hover:bg-slate-50"
              onClick={() => {
                setEditingStaffMember(null);
                form.reset(toFormValues());
              }}
              type="button"
            >
              Nuevo staff member
            </button>
          </div>

          <div className="mt-5 grid gap-4">
            {staffMembers.length === 0 ? (
              <div className="rounded-[2rem] border border-dashed border-slate-300 bg-slate-50 p-8 text-center">
                <h3 className="text-xl font-black">Aun no hay staff members</h3>
                <p className="mt-2 text-sm text-slate-500">Crea el primer staff member para asignarle services y configurar disponibilidad.</p>
              </div>
            ) : (
              staffMembers.map((staffMember) => (
                <article className="rounded-[1.5rem] border border-slate-200 p-4 transition hover:border-indigo-200 hover:shadow-sm" key={staffMember.id}>
                  <div className="flex flex-col gap-4 md:flex-row md:items-start md:justify-between">
                    <div>
                      <div className="flex flex-wrap items-center gap-2">
                        <h3 className="text-lg font-black">{staffMember.displayName}</h3>
                        <span className={staffMember.isActive ? 'rounded-full bg-emerald-100 px-3 py-1 text-xs font-black text-emerald-700' : 'rounded-full bg-slate-100 px-3 py-1 text-xs font-black text-slate-500'}>
                          {staffMember.isActive ? 'Active' : 'Inactive'}
                        </span>
                      </div>
                      <p className="mt-2 text-sm text-slate-500">{staffMember.bio || 'Sin bio publica.'}</p>
                      <div className="mt-4 flex flex-wrap gap-2 text-xs font-black text-slate-600">
                        <span className="rounded-full bg-slate-100 px-3 py-1">{staffMember.email || 'Sin email'}</span>
                        <span className="rounded-full bg-slate-100 px-3 py-1">{staffMember.phoneNumber || 'Sin telefono'}</span>
                        <span className="rounded-full bg-slate-100 px-3 py-1">Orden {staffMember.sortOrder}</span>
                      </div>
                    </div>
                    <div className="flex flex-wrap gap-2 md:justify-end">
                      <button className="rounded-xl border border-slate-200 px-3 py-2 text-sm font-black text-slate-700 hover:bg-slate-50" onClick={() => setEditingStaffMember(staffMember)} type="button">
                        Editar
                      </button>
                      <button
                        className="rounded-xl border border-slate-200 px-3 py-2 text-sm font-black text-slate-700 hover:bg-slate-50 disabled:opacity-50"
                        disabled={activeStateMutation.isPending}
                        onClick={() => activeStateMutation.mutate({ isActive: !staffMember.isActive, staffMemberId: staffMember.id })}
                        type="button"
                      >
                        {staffMember.isActive ? 'Desactivar' : 'Activar'}
                      </button>
                      <button className="rounded-xl border border-red-200 px-3 py-2 text-sm font-black text-red-700 hover:bg-red-50 disabled:opacity-50" disabled={deleteMutation.isPending} onClick={() => handleDelete(staffMember)} type="button">
                        Eliminar
                      </button>
                    </div>
                  </div>
                </article>
              ))
            )}
          </div>
        </div>

        <form className="rounded-[2rem] border border-slate-200 bg-white p-5 shadow-sm md:p-6" onSubmit={form.handleSubmit(handleSubmit)}>
          <div className="border-b border-slate-100 pb-5">
            <p className="text-sm font-black uppercase tracking-[0.2em] text-indigo-600">{editingStaffMember ? 'Edit staff member' : 'Create staff member'}</p>
            <h2 className="mt-2 text-2xl font-black tracking-tight">{editingStaffMember ? editingStaffMember.displayName : 'Nuevo staff member'}</h2>
            <p className="mt-2 text-sm text-slate-500">Define datos de contacto, bio y orden de visualizacion.</p>
          </div>

          <div className="mt-5 grid gap-4">
            <ApiErrorAlert message={mutationError ? getApiErrorMessage(mutationError) : undefined} />
            {successMessage ? <SuccessAlert message={successMessage} /> : null}
            <label className={labelClassName}>
              Display name
              <input className={inputClassName} {...form.register('displayName')} />
              <FieldError message={form.formState.errors.displayName?.message} />
            </label>
            <div className="grid gap-4 md:grid-cols-2">
              <label className={labelClassName}>
                Email
                <input className={inputClassName} type="email" {...form.register('email')} />
                <FieldError message={form.formState.errors.email?.message} />
              </label>
              <label className={labelClassName}>
                Phone number
                <input className={inputClassName} {...form.register('phoneNumber')} />
                <FieldError message={form.formState.errors.phoneNumber?.message} />
              </label>
            </div>
            <label className={labelClassName}>
              Bio
              <textarea className={`${inputClassName} min-h-28 resize-y`} {...form.register('bio')} />
              <FieldError message={form.formState.errors.bio?.message} />
            </label>
            <label className={labelClassName}>
              Sort order
              <input className={inputClassName} type="number" {...form.register('sortOrder', { valueAsNumber: true })} />
              <FieldError message={form.formState.errors.sortOrder?.message} />
            </label>
          </div>

          <div className="mt-8 grid gap-3 sm:grid-cols-2">
            {editingStaffMember ? (
              <button className="rounded-2xl border border-slate-200 px-5 py-3.5 text-base font-black text-slate-700 transition hover:bg-slate-50" onClick={() => setEditingStaffMember(null)} type="button">
                Cancelar edicion
              </button>
            ) : null}
            <button className={editingStaffMember ? primaryButtonClassName : `${primaryButtonClassName} sm:col-span-2`} disabled={isSaving} type="submit">
              {isSaving ? 'Guardando...' : editingStaffMember ? 'Guardar cambios' : 'Crear staff member'}
            </button>
          </div>
        </form>
      </section>
    </StaffMembersFrame>
  );
}

function StaffMembersFrame({ activeStaffMembersCount, children, totalStaffMembersCount }: { activeStaffMembersCount: number; children: ReactNode; totalStaffMembersCount: number }) {
  return (
    <div>
      <div className="mb-8 flex flex-col gap-4 md:flex-row md:items-end md:justify-between">
        <div>
          <p className="text-sm font-black uppercase tracking-[0.22em] text-indigo-600">Admin staff</p>
          <h1 className="mt-3 text-4xl font-black tracking-tight text-slate-950">Gestiona tu staff</h1>
          <p className="mt-3 max-w-3xl text-slate-600">Crea las personas o recursos que atenderan services y tendran disponibilidad propia.</p>
        </div>
        <div className="grid grid-cols-2 gap-3 text-center">
          <div className="rounded-2xl border border-slate-200 bg-white px-5 py-3 shadow-sm">
            <div className="text-2xl font-black">{totalStaffMembersCount}</div>
            <div className="text-xs font-bold text-slate-500">Total</div>
          </div>
          <div className="rounded-2xl border border-emerald-100 bg-emerald-50 px-5 py-3 shadow-sm">
            <div className="text-2xl font-black text-emerald-700">{activeStaffMembersCount}</div>
            <div className="text-xs font-bold text-emerald-700">Active</div>
          </div>
        </div>
      </div>
      {children}
    </div>
  );
}

function StaffMembersSkeleton() {
  return (
    <StaffMembersFrame activeStaffMembersCount={0} totalStaffMembersCount={0}>
      <div className="grid gap-6 xl:grid-cols-[1.1fr_0.9fr]">
        <div className="h-[560px] animate-pulse rounded-[2rem] bg-slate-200" />
        <div className="h-[480px] animate-pulse rounded-[2rem] bg-slate-200" />
      </div>
    </StaffMembersFrame>
  );
}

function SuccessAlert({ message }: { message: string }) {
  return <div className="rounded-2xl border border-emerald-200 bg-emerald-50 px-4 py-3 text-sm font-bold text-emerald-700">{message}</div>;
}

function toFormValues(staffMember?: StaffMemberResponse): StaffMemberFormValues {
  return {
    bio: staffMember?.bio ?? '',
    displayName: staffMember?.displayName ?? '',
    email: staffMember?.email ?? '',
    phoneNumber: staffMember?.phoneNumber ?? '',
    sortOrder: staffMember?.sortOrder ?? 0,
  };
}

function toRequest(values: StaffMemberFormValues) {
  return {
    bio: emptyToNull(values.bio),
    displayName: values.displayName,
    email: emptyToNull(values.email),
    phoneNumber: emptyToNull(values.phoneNumber),
    sortOrder: values.sortOrder,
  };
}
