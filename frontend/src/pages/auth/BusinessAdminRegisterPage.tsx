import { zodResolver } from '@hookform/resolvers/zod';
import { useMutation } from '@tanstack/react-query';
import { Link, useNavigate } from 'react-router-dom';
import { useForm } from 'react-hook-form';

import { registerBusiness } from '@/features/auth/authApi';
import { getAuthErrorMessage } from '@/features/auth/authErrors';
import { ApiErrorAlert, AuthShell, FieldError, inputClassName, labelClassName, primaryButtonClassName } from '@/features/auth/authUi';
import { type BusinessRegisterFormValues, businessRegisterSchema } from '@/features/auth/authValidation';
import { routes } from '@/lib/routes';

export function BusinessAdminRegisterPage() {
  const navigate = useNavigate();
  const {
    formState: { errors },
    handleSubmit,
    register,
  } = useForm<BusinessRegisterFormValues>({
    defaultValues: {
      adminDisplayName: '',
      adminEmail: '',
      adminPassword: '',
      businessName: '',
      businessSlug: '',
      currencyCode: 'EUR',
      timeZoneId: 'Europe/Madrid',
    },
    resolver: zodResolver(businessRegisterSchema),
  });

  const registerMutation = useMutation({
    mutationFn: registerBusiness,
    onSuccess: () => {
      navigate(routes.adminLogin, {
        replace: true,
        state: { message: 'Business registrado. Entra con el admin que acabas de crear.' },
      });
    },
  });

  return (
    <AuthShell
      description="Crea el business, define su identidad publica y genera el primer admin para configurar services, staff members y availability."
      eyebrow="Business onboarding"
      highlights={["Timezone IANA desde el primer dia", "Moneda preparada para services", "Admin inicial separado de customers"]}
      title="Publica tu business en minutos"
    >
      <div className="mb-8">
        <p className="text-sm font-black uppercase tracking-[0.2em] text-indigo-600">Register business</p>
        <h2 className="mt-3 text-3xl font-black tracking-tight">Crea tu business</h2>
        <p className="mt-2 text-sm text-slate-500">Despues entraras con el admin para completar la configuracion.</p>
      </div>

      <div className="mb-4">
        <ApiErrorAlert message={registerMutation.error ? getAuthErrorMessage(registerMutation.error) : undefined} />
      </div>

      <form className="grid gap-5" onSubmit={handleSubmit((values) => registerMutation.mutate({ ...values, currencyCode: values.currencyCode.toUpperCase() }))}>
        <div className="grid gap-5 md:grid-cols-2">
          <label className={labelClassName}>
            Business name
            <input autoComplete="organization" className={inputClassName} {...register('businessName')} />
            <FieldError message={errors.businessName?.message} />
          </label>
          <label className={labelClassName}>
            Public slug
            <input className={inputClassName} placeholder="studio-centro" {...register('businessSlug')} />
            <FieldError message={errors.businessSlug?.message} />
          </label>
        </div>
        <div className="grid gap-5 md:grid-cols-2">
          <label className={labelClassName}>
            Timezone
            <input className={inputClassName} placeholder="Europe/Madrid" {...register('timeZoneId')} />
            <FieldError message={errors.timeZoneId?.message} />
          </label>
          <label className={labelClassName}>
            Currency
            <input className={inputClassName} maxLength={3} placeholder="EUR" {...register('currencyCode')} />
            <FieldError message={errors.currencyCode?.message} />
          </label>
        </div>
        <label className={labelClassName}>
          Admin display name
          <input autoComplete="name" className={inputClassName} {...register('adminDisplayName')} />
          <FieldError message={errors.adminDisplayName?.message} />
        </label>
        <label className={labelClassName}>
          Admin email
          <input autoComplete="email" className={inputClassName} type="email" {...register('adminEmail')} />
          <FieldError message={errors.adminEmail?.message} />
        </label>
        <label className={labelClassName}>
          Admin password
          <input autoComplete="new-password" className={inputClassName} type="password" {...register('adminPassword')} />
          <FieldError message={errors.adminPassword?.message} />
        </label>
        <button className={primaryButtonClassName} disabled={registerMutation.isPending} type="submit">
          {registerMutation.isPending ? 'Creando business...' : 'Crear business'}
        </button>
      </form>

      <p className="mt-6 text-center text-sm font-semibold text-slate-500">
        Ya tienes admin?{' '}
        <Link className="font-black text-indigo-600" to={routes.adminLogin}>
          Entra aqui
        </Link>
      </p>
    </AuthShell>
  );
}
