import { zodResolver } from '@hookform/resolvers/zod';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { useEffect, useState, type ReactNode } from 'react';
import { useForm } from 'react-hook-form';

import type { BusinessResponse } from '@/api/contracts';
import { getApiErrorMessage } from '@/api/apiErrors';
import { getAuthSession } from '@/auth/authStorage';
import { getBusinessById, updateCurrentBusinessBookingWindow, updateCurrentBusinessDetails } from '@/features/businesses/businessApi';
import {
  type BookingWindowFormValues,
  type BusinessDetailsFormValues,
  bookingWindowSchema,
  businessDetailsSchema,
  emptyToNull,
} from '@/features/businesses/businessValidation';
import { ApiErrorAlert, FieldError, inputClassName, labelClassName, primaryButtonClassName } from '@/features/auth/authUi';
import {
  businessCategoryOptions,
  getMarketplaceCategoryLabel,
  normalizeMarketplaceCategoryValue,
} from '@/features/publicBusiness/marketplaceSearch';

const businessQueryKey = ['admin', 'business'] as const;

export function BusinessSettingsPage() {
  const session = getAuthSession('Admin');
  const queryClient = useQueryClient();
  const [detailsSuccessMessage, setDetailsSuccessMessage] = useState<string>();
  const [bookingWindowSuccessMessage, setBookingWindowSuccessMessage] = useState<string>();

  const businessQuery = useQuery({
    enabled: Boolean(session?.businessId),
    queryFn: () => getBusinessById(session!.businessId),
    queryKey: [...businessQueryKey, session?.businessId],
  });

  const detailsForm = useForm<BusinessDetailsFormValues>({
    defaultValues: toDetailsFormValues(),
    resolver: zodResolver(businessDetailsSchema),
  });
  const bookingWindowForm = useForm<BookingWindowFormValues>({
    defaultValues: { maxAdvanceBookingDays: 30 },
    resolver: zodResolver(bookingWindowSchema),
  });

  useEffect(() => {
    if (!businessQuery.data) {
      return;
    }

    detailsForm.reset(toDetailsFormValues(businessQuery.data));
    bookingWindowForm.reset({ maxAdvanceBookingDays: businessQuery.data.maxAdvanceBookingDays });
  }, [businessQuery.data, bookingWindowForm, detailsForm]);

  const detailsMutation = useMutation({
    mutationFn: (values: BusinessDetailsFormValues) =>
      updateCurrentBusinessDetails(
        {
          addressLine1: emptyToNull(values.addressLine1),
          addressLine2: emptyToNull(values.addressLine2),
          category: emptyToNull(values.category),
          city: emptyToNull(values.city),
          contactEmail: emptyToNull(values.contactEmail),
          contactPhoneNumber: emptyToNull(values.contactPhoneNumber),
          countryCode: emptyToNull(values.countryCode)?.toUpperCase() ?? null,
          currencyCode: values.currencyCode.toUpperCase(),
          description: emptyToNull(values.description),
          name: values.name,
          postalCode: emptyToNull(values.postalCode),
          timeZoneId: values.timeZoneId,
          websiteUrl: emptyToNull(values.websiteUrl),
        },
        session!.token,
      ),
    onSuccess: (business) => {
      setDetailsSuccessMessage('Business details actualizados.');
      queryClient.setQueryData([...businessQueryKey, session?.businessId], business);
      detailsForm.reset(toDetailsFormValues(business));
    },
  });

  const bookingWindowMutation = useMutation({
    mutationFn: (values: BookingWindowFormValues) => updateCurrentBusinessBookingWindow(values, session!.token),
    onSuccess: (response) => {
      setBookingWindowSuccessMessage('Ventana de reserva actualizada.');
      queryClient.setQueryData<BusinessResponse | undefined>([...businessQueryKey, session?.businessId], (business) =>
        business ? { ...business, maxAdvanceBookingDays: response.maxAdvanceBookingDays } : business,
      );
      bookingWindowForm.reset({ maxAdvanceBookingDays: response.maxAdvanceBookingDays });
    },
  });

  if (businessQuery.isPending) {
    return <BusinessSettingsSkeleton />;
  }

  if (businessQuery.isError) {
    return (
      <BusinessSettingsFrame>
        <ApiErrorAlert message={getApiErrorMessage(businessQuery.error)} />
      </BusinessSettingsFrame>
    );
  }

  return (
    <BusinessSettingsFrame>
      <section className="grid gap-6 xl:grid-cols-[1.4fr_0.8fr]">
        <form
          className="rounded-[2rem] border border-slate-200 bg-white p-6 shadow-sm"
          onSubmit={detailsForm.handleSubmit((values) => detailsMutation.mutate(values))}
        >
          <div className="flex flex-col gap-2 border-b border-slate-100 pb-5 md:flex-row md:items-end md:justify-between">
            <div>
              <p className="text-sm font-black uppercase tracking-[0.2em] text-indigo-600">Public details</p>
              <h2 className="mt-2 text-2xl font-black tracking-tight">Datos publicos del business</h2>
              <p className="mt-2 max-w-2xl text-sm text-slate-500">Estos datos se usaran en el perfil publico y en el flujo de slots.</p>
            </div>
            <div className="rounded-full bg-slate-100 px-4 py-2 text-sm font-black text-slate-600">/{businessQuery.data.slug}</div>
          </div>

          <div className="mt-5 grid gap-4">
            <ApiErrorAlert message={detailsMutation.error ? getApiErrorMessage(detailsMutation.error) : undefined} />
            {detailsSuccessMessage ? <SuccessAlert message={detailsSuccessMessage} /> : null}
          </div>

          <div className="mt-6 grid gap-5 md:grid-cols-2">
            <label className={labelClassName}>
              Business name
              <input className={inputClassName} {...detailsForm.register('name')} />
              <FieldError message={detailsForm.formState.errors.name?.message} />
            </label>
            <label className={labelClassName}>
              Categoria marketplace
              <select className={inputClassName} {...detailsForm.register('category')}>
                {businessCategoryOptions.map((option) => (
                  <option key={option.value} value={option.value}>{option.label}</option>
                ))}
              </select>
              <FieldError message={detailsForm.formState.errors.category?.message} />
            </label>
            <label className={labelClassName}>
              Contact email
              <input className={inputClassName} type="email" {...detailsForm.register('contactEmail')} />
              <FieldError message={detailsForm.formState.errors.contactEmail?.message} />
            </label>
            <label className={labelClassName}>
              Contact phone
              <input className={inputClassName} {...detailsForm.register('contactPhoneNumber')} />
              <FieldError message={detailsForm.formState.errors.contactPhoneNumber?.message} />
            </label>
            <label className={labelClassName}>
              Website
              <input className={inputClassName} placeholder="https://example.com" {...detailsForm.register('websiteUrl')} />
              <FieldError message={detailsForm.formState.errors.websiteUrl?.message} />
            </label>
            <label className={`${labelClassName} md:col-span-2`}>
              Description
              <textarea className={`${inputClassName} min-h-28 resize-y`} {...detailsForm.register('description')} />
              <FieldError message={detailsForm.formState.errors.description?.message} />
            </label>
            <label className={labelClassName}>
              Address line 1
              <input className={inputClassName} {...detailsForm.register('addressLine1')} />
              <FieldError message={detailsForm.formState.errors.addressLine1?.message} />
            </label>
            <label className={labelClassName}>
              Address line 2
              <input className={inputClassName} {...detailsForm.register('addressLine2')} />
              <FieldError message={detailsForm.formState.errors.addressLine2?.message} />
            </label>
            <label className={labelClassName}>
              City
              <input className={inputClassName} {...detailsForm.register('city')} />
              <FieldError message={detailsForm.formState.errors.city?.message} />
            </label>
            <label className={labelClassName}>
              Postal code
              <input className={inputClassName} {...detailsForm.register('postalCode')} />
              <FieldError message={detailsForm.formState.errors.postalCode?.message} />
            </label>
            <label className={labelClassName}>
              Country code
              <input className={inputClassName} maxLength={2} placeholder="ES" {...detailsForm.register('countryCode')} />
              <FieldError message={detailsForm.formState.errors.countryCode?.message} />
            </label>
            <label className={labelClassName}>
              Currency
              <input className={inputClassName} maxLength={3} placeholder="EUR" {...detailsForm.register('currencyCode')} />
              <FieldError message={detailsForm.formState.errors.currencyCode?.message} />
            </label>
            <label className={`${labelClassName} md:col-span-2`}>
              Timezone
              <input className={inputClassName} placeholder="Europe/Madrid" {...detailsForm.register('timeZoneId')} />
              <FieldError message={detailsForm.formState.errors.timeZoneId?.message} />
            </label>
          </div>

          <div className="mt-8 flex justify-end">
            <button className="rounded-2xl bg-slate-950 px-6 py-3 text-sm font-black text-white transition hover:bg-indigo-700 disabled:bg-slate-300" disabled={detailsMutation.isPending} type="submit">
              {detailsMutation.isPending ? 'Guardando...' : 'Guardar details'}
            </button>
          </div>
        </form>

        <aside className="grid gap-6 content-start">
          <form
            className="rounded-[2rem] border border-slate-200 bg-white p-6 shadow-sm"
            onSubmit={bookingWindowForm.handleSubmit((values) => bookingWindowMutation.mutate(values))}
          >
            <p className="text-sm font-black uppercase tracking-[0.2em] text-indigo-600">Booking window</p>
            <h2 className="mt-2 text-2xl font-black tracking-tight">Antelacion maxima</h2>
            <p className="mt-2 text-sm text-slate-500">Define cuantos dias hacia adelante se pueden consultar slots.</p>
            <div className="mt-5 grid gap-4">
              <ApiErrorAlert message={bookingWindowMutation.error ? getApiErrorMessage(bookingWindowMutation.error) : undefined} />
              {bookingWindowSuccessMessage ? <SuccessAlert message={bookingWindowSuccessMessage} /> : null}
              <label className={labelClassName}>
                Max advance booking days
                <input className={inputClassName} max={365} min={1} type="number" {...bookingWindowForm.register('maxAdvanceBookingDays', { valueAsNumber: true })} />
                <FieldError message={bookingWindowForm.formState.errors.maxAdvanceBookingDays?.message} />
              </label>
              <button className={primaryButtonClassName} disabled={bookingWindowMutation.isPending} type="submit">
                {bookingWindowMutation.isPending ? 'Guardando...' : 'Guardar ventana'}
              </button>
            </div>
          </form>

          <div className="rounded-[2rem] border border-indigo-100 bg-indigo-50 p-6 text-sm text-indigo-950">
            <div className="text-xs font-black uppercase tracking-[0.2em] text-indigo-600">Resumen</div>
            <div className="mt-4 grid gap-3 font-bold">
              <div>Business id: {businessQuery.data.id}</div>
              <div>Slug: {businessQuery.data.slug}</div>
              <div>Categoria: {getMarketplaceCategoryLabel(businessQuery.data.category) || 'Sin categoria'}</div>
              <div>Timezone: {businessQuery.data.timeZoneId}</div>
              <div>Currency: {businessQuery.data.currencyCode}</div>
            </div>
          </div>
        </aside>
      </section>
    </BusinessSettingsFrame>
  );
}

function BusinessSettingsFrame({ children }: { children: ReactNode }) {
  return (
    <div>
      <div className="mb-8">
        <p className="text-sm font-black uppercase tracking-[0.22em] text-indigo-600">Business settings</p>
        <h1 className="mt-3 text-4xl font-black tracking-tight text-slate-950">Configura tu business</h1>
        <p className="mt-3 max-w-3xl text-slate-600">Sincroniza la informacion publica que veran customers y la ventana usada por la disponibilidad.</p>
      </div>
      {children}
    </div>
  );
}

function BusinessSettingsSkeleton() {
  return (
    <BusinessSettingsFrame>
      <div className="grid gap-6 xl:grid-cols-[1.4fr_0.8fr]">
        <div className="h-[560px] animate-pulse rounded-[2rem] bg-slate-200" />
        <div className="h-80 animate-pulse rounded-[2rem] bg-slate-200" />
      </div>
    </BusinessSettingsFrame>
  );
}

function SuccessAlert({ message }: { message: string }) {
  return <div className="rounded-2xl border border-emerald-200 bg-emerald-50 px-4 py-3 text-sm font-bold text-emerald-700">{message}</div>;
}

function toDetailsFormValues(business?: BusinessResponse): BusinessDetailsFormValues {
  return {
    addressLine1: business?.addressLine1 ?? '',
    addressLine2: business?.addressLine2 ?? '',
    category: business?.category ? normalizeMarketplaceCategoryValue(business.category) : '',
    city: business?.city ?? '',
    contactEmail: business?.contactEmail ?? '',
    contactPhoneNumber: business?.contactPhoneNumber ?? '',
    countryCode: business?.countryCode ?? '',
    currencyCode: business?.currencyCode ?? 'EUR',
    description: business?.description ?? '',
    name: business?.name ?? '',
    postalCode: business?.postalCode ?? '',
    timeZoneId: business?.timeZoneId ?? 'Europe/Madrid',
    websiteUrl: business?.websiteUrl ?? '',
  };
}
