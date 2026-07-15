import type { ReactNode } from 'react';
import { Link } from 'react-router-dom';

type AuthShellProps = {
  children: ReactNode;
  description: string;
  eyebrow: string;
  highlights: string[];
  title: string;
};

type FieldErrorProps = {
  message?: string;
};

type ApiErrorAlertProps = {
  message?: string;
};

export function AuthShell({ children, description, eyebrow, highlights, title }: AuthShellProps) {
  return (
    <main className="min-h-screen bg-[radial-gradient(circle_at_top_left,#eef2ff,transparent_34%),linear-gradient(135deg,#ffffff_0%,#f8fafc_48%,#eef2ff_100%)] px-6 py-8 text-slate-950">
      <div className="mx-auto grid min-h-[calc(100vh-4rem)] max-w-6xl gap-8 lg:grid-cols-[0.95fr_1.05fr] lg:items-center">
        <section className="max-w-xl">
          <Link className="text-2xl font-black tracking-tight" to="/">
            Calendar Manager
          </Link>
          <p className="mt-16 text-sm font-black uppercase tracking-[0.28em] text-indigo-600">{eyebrow}</p>
          <h1 className="mt-5 text-5xl font-black leading-[0.95] tracking-tight md:text-6xl">{title}</h1>
          <p className="mt-6 text-lg leading-8 text-slate-600">{description}</p>
          <div className="mt-8 grid gap-3">
            {highlights.map((highlight) => (
              <div className="rounded-2xl border border-white/80 bg-white/70 px-4 py-3 text-sm font-bold text-slate-700 shadow-sm" key={highlight}>
                {highlight}
              </div>
            ))}
          </div>
        </section>
        <section className="rounded-[2rem] border border-white/80 bg-white/90 p-5 shadow-2xl shadow-indigo-100/80 backdrop-blur md:p-8">
          {children}
        </section>
      </div>
    </main>
  );
}

export function FieldError({ message }: FieldErrorProps) {
  if (!message) {
    return null;
  }

  return <p className="mt-2 text-sm font-semibold text-red-600">{message}</p>;
}

export function ApiErrorAlert({ message }: ApiErrorAlertProps) {
  if (!message) {
    return null;
  }

  return (
    <div className="rounded-2xl border border-red-200 bg-red-50 px-4 py-3 text-sm font-bold text-red-700" role="alert">
      {message}
    </div>
  );
}

export const inputClassName =
  'mt-2 w-full rounded-2xl border border-slate-200 bg-white px-4 py-3 text-base font-semibold text-slate-950 outline-none transition placeholder:text-slate-400 focus:border-indigo-500 focus:ring-4 focus:ring-indigo-100';

export const labelClassName = 'block text-sm font-black text-slate-700';

export const primaryButtonClassName =
  'w-full rounded-2xl bg-slate-950 px-5 py-3.5 text-base font-black text-white transition hover:bg-indigo-700 disabled:cursor-not-allowed disabled:bg-slate-300';
