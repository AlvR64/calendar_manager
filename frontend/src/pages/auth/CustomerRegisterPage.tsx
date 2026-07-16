import { zodResolver } from '@hookform/resolvers/zod';
import { useMutation } from '@tanstack/react-query';
import { Link, useLocation, useNavigate } from 'react-router-dom';
import { useForm } from 'react-hook-form';

import { registerCustomer } from '@/features/auth/authApi';
import { getAuthErrorMessage } from '@/features/auth/authErrors';
import { ApiErrorAlert, AuthShell, FieldError, inputClassName, labelClassName, primaryButtonClassName } from '@/features/auth/authUi';
import { type CustomerRegisterFormValues, customerRegisterSchema, emptyToNull } from '@/features/auth/authValidation';
import { routes } from '@/lib/routes';

export function CustomerRegisterPage() {
  const navigate = useNavigate();
  const location = useLocation();
  const searchParams = new URLSearchParams(location.search);
  const returnTo = getSafeReturnTo(searchParams.get('returnTo'));
  const {
    formState: { errors },
    handleSubmit,
    register,
  } = useForm<CustomerRegisterFormValues>({
    defaultValues: { email: '', firstName: '', lastName: '', password: '', phoneNumber: '' },
    resolver: zodResolver(customerRegisterSchema),
  });

  const registerMutation = useMutation({
    mutationFn: registerCustomer,
    onSuccess: () => {
      navigate(withReturnTo(routes.customerLogin, returnTo), {
        replace: true,
        state: { message: 'Cuenta customer creada. Entra para continuar.' },
      });
    },
  });

  return (
    <AuthShell
      description="Crea una cuenta customer para tener tus datos preparados cuando el flujo de appointment permita confirmar slots."
      eyebrow="Customer auth"
      highlights={["Registro rapido", "Datos listos para futuros appointments", "Cuenta separada del panel admin"]}
      title="Crea tu cuenta customer"
    >
      <div className="mb-8">
        <p className="text-sm font-black uppercase tracking-[0.2em] text-indigo-600">Customer register</p>
        <h2 className="mt-3 text-3xl font-black tracking-tight">Tus datos basicos</h2>
        <p className="mt-2 text-sm text-slate-500">El primer MVP llega hasta seleccionar slot, pero la cuenta ya queda preparada.</p>
      </div>

      <div className="mb-4">
        <ApiErrorAlert message={registerMutation.error ? getAuthErrorMessage(registerMutation.error) : undefined} />
      </div>

      <form
        className="grid gap-5"
        onSubmit={handleSubmit((values) =>
          registerMutation.mutate({
            email: values.email,
            firstName: values.firstName,
            lastName: emptyToNull(values.lastName),
            password: values.password,
            phoneNumber: emptyToNull(values.phoneNumber),
          }),
        )}
      >
        <div className="grid gap-5 md:grid-cols-2">
          <label className={labelClassName}>
            First name
            <input autoComplete="given-name" className={inputClassName} {...register('firstName')} />
            <FieldError message={errors.firstName?.message} />
          </label>
          <label className={labelClassName}>
            Last name
            <input autoComplete="family-name" className={inputClassName} {...register('lastName')} />
            <FieldError message={errors.lastName?.message} />
          </label>
        </div>
        <label className={labelClassName}>
          Email
          <input autoComplete="email" className={inputClassName} type="email" {...register('email')} />
          <FieldError message={errors.email?.message} />
        </label>
        <label className={labelClassName}>
          Phone
          <input autoComplete="tel" className={inputClassName} {...register('phoneNumber')} />
          <FieldError message={errors.phoneNumber?.message} />
        </label>
        <label className={labelClassName}>
          Password
          <input autoComplete="new-password" className={inputClassName} type="password" {...register('password')} />
          <FieldError message={errors.password?.message} />
        </label>
        <button className={primaryButtonClassName} disabled={registerMutation.isPending} type="submit">
          {registerMutation.isPending ? 'Creando cuenta...' : 'Crear cuenta customer'}
        </button>
      </form>

      <p className="mt-6 text-center text-sm font-semibold text-slate-500">
        Ya tienes cuenta?{' '}
        <Link className="font-black text-indigo-600" to={withReturnTo(routes.customerLogin, returnTo)}>
          Entra aqui
        </Link>
      </p>
    </AuthShell>
  );
}

function getSafeReturnTo(value: string | null) {
  return value?.startsWith('/') && !value.startsWith('//') ? value : null;
}

function withReturnTo(path: string, returnTo: string | null) {
  return returnTo ? `${path}?returnTo=${encodeURIComponent(returnTo)}` : path;
}
