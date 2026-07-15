import { zodResolver } from '@hookform/resolvers/zod';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { useEffect, useMemo, useState, type ReactNode } from 'react';
import { useForm } from 'react-hook-form';

import type { ServiceResponse } from '@/api/contracts';
import { getApiErrorMessage } from '@/api/apiErrors';
import { getAuthSession } from '@/auth/authStorage';
import { ApiErrorAlert, FieldError, inputClassName, labelClassName, primaryButtonClassName } from '@/features/auth/authUi';
import { createService, deleteService, listServices, updateService, updateServiceActiveState } from '@/features/services/serviceApi';
import { type ServiceFormValues, emptyToNull, serviceFormSchema } from '@/features/services/serviceValidation';

const servicesQueryKey = ['admin', 'services'] as const;

export function ServicesPage() {
  const session = getAuthSession('Admin');
  const queryClient = useQueryClient();
  const [editingService, setEditingService] = useState<ServiceResponse | null>(null);
  const [successMessage, setSuccessMessage] = useState<string>();

  const servicesQuery = useQuery({
    queryFn: () => listServices(session!.token),
    queryKey: servicesQueryKey,
  });

  const services = useMemo(
    () => [...(servicesQuery.data ?? [])].sort((a, b) => a.sortOrder - b.sortOrder || a.name.localeCompare(b.name)),
    [servicesQuery.data],
  );
  const activeServicesCount = services.filter((service) => service.isActive).length;

  const form = useForm<ServiceFormValues>({
    defaultValues: toFormValues(),
    resolver: zodResolver(serviceFormSchema),
  });

  useEffect(() => {
    form.reset(toFormValues(editingService ?? undefined));
  }, [editingService, form]);

  const createMutation = useMutation({
    mutationFn: (values: ServiceFormValues) => createService(toRequest(values), session!.token),
    onSuccess: () => {
      setSuccessMessage('Service creado.');
      setEditingService(null);
      form.reset(toFormValues());
      queryClient.invalidateQueries({ queryKey: servicesQueryKey });
    },
  });

  const updateMutation = useMutation({
    mutationFn: (values: ServiceFormValues) => updateService(editingService!.id, toRequest(values), session!.token),
    onSuccess: () => {
      setSuccessMessage('Service actualizado.');
      setEditingService(null);
      form.reset(toFormValues());
      queryClient.invalidateQueries({ queryKey: servicesQueryKey });
    },
  });

  const activeStateMutation = useMutation({
    mutationFn: ({ isActive, serviceId }: { isActive: boolean; serviceId: string }) =>
      updateServiceActiveState(serviceId, { isActive }, session!.token),
    onSuccess: () => {
      setSuccessMessage('Estado del service actualizado.');
      queryClient.invalidateQueries({ queryKey: servicesQueryKey });
    },
  });

  const deleteMutation = useMutation({
    mutationFn: (serviceId: string) => deleteService(serviceId, session!.token),
    onSuccess: () => {
      setSuccessMessage('Service eliminado.');
      if (editingService) {
        setEditingService(null);
        form.reset(toFormValues());
      }
      queryClient.invalidateQueries({ queryKey: servicesQueryKey });
    },
  });

  function handleSubmit(values: ServiceFormValues) {
    setSuccessMessage(undefined);
    if (editingService) {
      updateMutation.mutate(values);
      return;
    }

    createMutation.mutate(values);
  }

  function handleDelete(service: ServiceResponse) {
    setSuccessMessage(undefined);
    if (!window.confirm(`Eliminar ${service.name}?`)) {
      return;
    }

    deleteMutation.mutate(service.id);
  }

  const mutationError = createMutation.error ?? updateMutation.error ?? activeStateMutation.error ?? deleteMutation.error;
  const isSaving = createMutation.isPending || updateMutation.isPending;

  if (servicesQuery.isPending) {
    return <ServicesSkeleton />;
  }

  if (servicesQuery.isError) {
    return (
      <ServicesFrame activeServicesCount={0} totalServicesCount={0}>
        <ApiErrorAlert message={getApiErrorMessage(servicesQuery.error)} />
      </ServicesFrame>
    );
  }

  return (
    <ServicesFrame activeServicesCount={activeServicesCount} totalServicesCount={services.length}>
      <section className="grid gap-6 xl:grid-cols-[1.1fr_0.9fr]">
        <div className="rounded-[2rem] border border-slate-200 bg-white p-5 shadow-sm md:p-6">
          <div className="flex flex-col gap-3 border-b border-slate-100 pb-5 md:flex-row md:items-end md:justify-between">
            <div>
              <p className="text-sm font-black uppercase tracking-[0.2em] text-indigo-600">Services</p>
              <h2 className="mt-2 text-2xl font-black tracking-tight">Oferta del business</h2>
              <p className="mt-2 text-sm text-slate-500">Incluye services activos e inactivos para admin.</p>
            </div>
            <button
              className="rounded-2xl border border-slate-200 px-4 py-3 text-sm font-black text-slate-700 transition hover:bg-slate-50"
              onClick={() => {
                setEditingService(null);
                form.reset(toFormValues());
              }}
              type="button"
            >
              Nuevo service
            </button>
          </div>

          <div className="mt-5 grid gap-4">
            {services.length === 0 ? (
              <div className="rounded-[2rem] border border-dashed border-slate-300 bg-slate-50 p-8 text-center">
                <h3 className="text-xl font-black">Aun no hay services</h3>
                <p className="mt-2 text-sm text-slate-500">Crea el primer service para que pueda aparecer en el perfil publico y en slots.</p>
              </div>
            ) : (
              services.map((service) => (
                <article className="rounded-[1.5rem] border border-slate-200 p-4 transition hover:border-indigo-200 hover:shadow-sm" key={service.id}>
                  <div className="flex flex-col gap-4 md:flex-row md:items-start md:justify-between">
                    <div>
                      <div className="flex flex-wrap items-center gap-2">
                        <h3 className="text-lg font-black">{service.name}</h3>
                        <span className={service.isActive ? 'rounded-full bg-emerald-100 px-3 py-1 text-xs font-black text-emerald-700' : 'rounded-full bg-slate-100 px-3 py-1 text-xs font-black text-slate-500'}>
                          {service.isActive ? 'Active' : 'Inactive'}
                        </span>
                      </div>
                      <p className="mt-2 text-sm text-slate-500">{service.description || 'Sin descripcion publica.'}</p>
                      <div className="mt-4 flex flex-wrap gap-2 text-xs font-black text-slate-600">
                        <span className="rounded-full bg-slate-100 px-3 py-1">{service.durationMinutes} min</span>
                        <span className="rounded-full bg-slate-100 px-3 py-1">{formatPrice(service.priceAmount)}</span>
                        <span className="rounded-full bg-slate-100 px-3 py-1">Orden {service.sortOrder}</span>
                      </div>
                    </div>
                    <div className="flex flex-wrap gap-2 md:justify-end">
                      <button className="rounded-xl border border-slate-200 px-3 py-2 text-sm font-black text-slate-700 hover:bg-slate-50" onClick={() => setEditingService(service)} type="button">
                        Editar
                      </button>
                      <button
                        className="rounded-xl border border-slate-200 px-3 py-2 text-sm font-black text-slate-700 hover:bg-slate-50 disabled:opacity-50"
                        disabled={activeStateMutation.isPending}
                        onClick={() => activeStateMutation.mutate({ isActive: !service.isActive, serviceId: service.id })}
                        type="button"
                      >
                        {service.isActive ? 'Desactivar' : 'Activar'}
                      </button>
                      <button className="rounded-xl border border-red-200 px-3 py-2 text-sm font-black text-red-700 hover:bg-red-50 disabled:opacity-50" disabled={deleteMutation.isPending} onClick={() => handleDelete(service)} type="button">
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
            <p className="text-sm font-black uppercase tracking-[0.2em] text-indigo-600">{editingService ? 'Edit service' : 'Create service'}</p>
            <h2 className="mt-2 text-2xl font-black tracking-tight">{editingService ? editingService.name : 'Nuevo service'}</h2>
            <p className="mt-2 text-sm text-slate-500">Define duracion, precio y orden de visualizacion.</p>
          </div>

          <div className="mt-5 grid gap-4">
            <ApiErrorAlert message={mutationError ? getApiErrorMessage(mutationError) : undefined} />
            {successMessage ? <SuccessAlert message={successMessage} /> : null}
            <label className={labelClassName}>
              Service name
              <input className={inputClassName} {...form.register('name')} />
              <FieldError message={form.formState.errors.name?.message} />
            </label>
            <label className={labelClassName}>
              Description
              <textarea className={`${inputClassName} min-h-28 resize-y`} {...form.register('description')} />
              <FieldError message={form.formState.errors.description?.message} />
            </label>
            <div className="grid gap-4 md:grid-cols-3">
              <label className={labelClassName}>
                Duration
                <input className={inputClassName} min={1} type="number" {...form.register('durationMinutes', { valueAsNumber: true })} />
                <FieldError message={form.formState.errors.durationMinutes?.message} />
              </label>
              <label className={labelClassName}>
                Price
                <input className={inputClassName} min={0} step="0.01" type="number" {...form.register('priceAmount', { valueAsNumber: true })} />
                <FieldError message={form.formState.errors.priceAmount?.message} />
              </label>
              <label className={labelClassName}>
                Sort order
                <input className={inputClassName} type="number" {...form.register('sortOrder', { valueAsNumber: true })} />
                <FieldError message={form.formState.errors.sortOrder?.message} />
              </label>
            </div>
          </div>

          <div className="mt-8 grid gap-3 sm:grid-cols-2">
            {editingService ? (
              <button className="rounded-2xl border border-slate-200 px-5 py-3.5 text-base font-black text-slate-700 transition hover:bg-slate-50" onClick={() => setEditingService(null)} type="button">
                Cancelar edicion
              </button>
            ) : null}
            <button className={editingService ? primaryButtonClassName : `${primaryButtonClassName} sm:col-span-2`} disabled={isSaving} type="submit">
              {isSaving ? 'Guardando...' : editingService ? 'Guardar cambios' : 'Crear service'}
            </button>
          </div>
        </form>
      </section>
    </ServicesFrame>
  );
}

function ServicesFrame({ activeServicesCount, children, totalServicesCount }: { activeServicesCount: number; children: ReactNode; totalServicesCount: number }) {
  return (
    <div>
      <div className="mb-8 flex flex-col gap-4 md:flex-row md:items-end md:justify-between">
        <div>
          <p className="text-sm font-black uppercase tracking-[0.22em] text-indigo-600">Admin services</p>
          <h1 className="mt-3 text-4xl font-black tracking-tight text-slate-950">Gestiona tus services</h1>
          <p className="mt-3 max-w-3xl text-slate-600">Crea la oferta que customers podran elegir antes de seleccionar staff member, fecha y slot.</p>
        </div>
        <div className="grid grid-cols-2 gap-3 text-center">
          <div className="rounded-2xl border border-slate-200 bg-white px-5 py-3 shadow-sm">
            <div className="text-2xl font-black">{totalServicesCount}</div>
            <div className="text-xs font-bold text-slate-500">Total</div>
          </div>
          <div className="rounded-2xl border border-emerald-100 bg-emerald-50 px-5 py-3 shadow-sm">
            <div className="text-2xl font-black text-emerald-700">{activeServicesCount}</div>
            <div className="text-xs font-bold text-emerald-700">Active</div>
          </div>
        </div>
      </div>
      {children}
    </div>
  );
}

function ServicesSkeleton() {
  return (
    <ServicesFrame activeServicesCount={0} totalServicesCount={0}>
      <div className="grid gap-6 xl:grid-cols-[1.1fr_0.9fr]">
        <div className="h-[560px] animate-pulse rounded-[2rem] bg-slate-200" />
        <div className="h-[480px] animate-pulse rounded-[2rem] bg-slate-200" />
      </div>
    </ServicesFrame>
  );
}

function SuccessAlert({ message }: { message: string }) {
  return <div className="rounded-2xl border border-emerald-200 bg-emerald-50 px-4 py-3 text-sm font-bold text-emerald-700">{message}</div>;
}

function toFormValues(service?: ServiceResponse): ServiceFormValues {
  return {
    description: service?.description ?? '',
    durationMinutes: service?.durationMinutes ?? 60,
    name: service?.name ?? '',
    priceAmount: service?.priceAmount ?? 0,
    sortOrder: service?.sortOrder ?? 0,
  };
}

function toRequest(values: ServiceFormValues) {
  return {
    description: emptyToNull(values.description),
    durationMinutes: values.durationMinutes,
    name: values.name,
    priceAmount: values.priceAmount,
    sortOrder: values.sortOrder,
  };
}

function formatPrice(value: number) {
  return new Intl.NumberFormat('es-ES', { currency: 'EUR', style: 'currency' }).format(value);
}
