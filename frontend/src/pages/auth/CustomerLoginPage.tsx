import { zodResolver } from '@hookform/resolvers/zod';
import { useMutation } from '@tanstack/react-query';
import { Link, useLocation, useNavigate } from 'react-router-dom';
import { useForm } from 'react-hook-form';

import { setAuthSession } from '@/auth/authStorage';
import { loginCustomer } from '@/features/auth/authApi';
import { getAuthErrorMessage } from '@/features/auth/authErrors';
import { ApiErrorAlert, AuthShell, FieldError, inputClassName, labelClassName, primaryButtonClassName } from '@/features/auth/authUi';
import { type LoginFormValues, loginSchema } from '@/features/auth/authValidation';
import { routes } from '@/lib/routes';

export function CustomerLoginPage() {
  const navigate = useNavigate();
  const location = useLocation();
  const searchParams = new URLSearchParams(location.search);
  const returnTo = getSafeReturnTo(searchParams.get('returnTo'));
  const registrationMessage = (location.state as { message?: string } | null)?.message;
  const {
    formState: { errors },
    handleSubmit,
    register,
  } = useForm<LoginFormValues>({
    defaultValues: { email: '', password: '' },
    resolver: zodResolver(loginSchema),
  });

  const loginMutation = useMutation({
    mutationFn: loginCustomer,
    onSuccess: (response) => {
      setAuthSession({
        accountType: 'Customer',
        email: response.user.email,
        expiresAtUtc: response.expiresAtUtc,
        firstName: response.user.firstName,
        id: response.user.id,
        lastName: response.user.lastName,
        token: response.accessToken,
        tokenType: response.tokenType,
      });
      navigate(returnTo ?? routes.home, { replace: true });
    },
  });

  return (
    <AuthShell
      description="Accede como customer para reutilizar tus datos en futuros appointments cuando el flujo de confirmacion este disponible."
      eyebrow="Customer auth"
      highlights={["Cuenta customer separada de admin", "Preparado para appointments futuros", "Tus datos quedan listos para confirmar reservas"]}
      title="Entra para reservar mas rapido"
    >
      <div className="mb-8">
        <p className="text-sm font-black uppercase tracking-[0.2em] text-indigo-600">Customer login</p>
        <h2 className="mt-3 text-3xl font-black tracking-tight">Accede a tu cuenta</h2>
        <p className="mt-2 text-sm text-slate-500">El area privada de customer llegara despues del primer MVP.</p>
      </div>

      {registrationMessage ? <div className="mb-4 rounded-2xl border border-emerald-200 bg-emerald-50 px-4 py-3 text-sm font-bold text-emerald-700">{registrationMessage}</div> : null}
      <div className="mb-4">
        <ApiErrorAlert message={loginMutation.error ? getAuthErrorMessage(loginMutation.error) : undefined} />
      </div>

      <form className="grid gap-5" onSubmit={handleSubmit((values) => loginMutation.mutate(values))}>
        <label className={labelClassName}>
          Email
          <input autoComplete="email" className={inputClassName} type="email" {...register('email')} />
          <FieldError message={errors.email?.message} />
        </label>
        <label className={labelClassName}>
          Password
          <input autoComplete="current-password" className={inputClassName} type="password" {...register('password')} />
          <FieldError message={errors.password?.message} />
        </label>
        <button className={primaryButtonClassName} disabled={loginMutation.isPending} type="submit">
          {loginMutation.isPending ? 'Entrando...' : 'Entrar como customer'}
        </button>
      </form>

      <p className="mt-6 text-center text-sm font-semibold text-slate-500">
        No tienes cuenta?{' '}
        <Link className="font-black text-indigo-600" to={withReturnTo(routes.customerRegister, returnTo)}>
          Registrate
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
