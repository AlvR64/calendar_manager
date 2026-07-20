import type { FormEvent } from 'react';
import { useState } from 'react';
import { Link } from 'react-router-dom';

import type { PublicBusinessCardResponse } from '@/api/contracts';
import {
  emptyMarketplaceSearchFilters,
  marketplaceCategoryOptions,
  normalizeSearchFilters,
  type MarketplaceSearchFilters,
} from '@/features/publicBusiness/marketplaceSearch';
import { routes } from '@/lib/routes';

type BusinessSearchFormProps = {
  initialFilters?: MarketplaceSearchFilters;
  onSubmit: (filters: MarketplaceSearchFilters) => void;
  submitLabel?: string;
  variant?: 'hero' | 'panel';
};

export function BusinessSearchForm({
  initialFilters = emptyMarketplaceSearchFilters,
  onSubmit,
  submitLabel = 'Buscar businesses',
  variant = 'panel',
}: BusinessSearchFormProps) {
  const [filters, setFilters] = useState(initialFilters);

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    onSubmit(normalizeSearchFilters(filters));
  }

  const formClassName = variant === 'hero'
    ? 'mt-9 rounded-[2rem] border border-white/80 bg-white/90 p-4 shadow-2xl shadow-indigo-100/70 backdrop-blur md:p-5'
    : 'rounded-[2.125rem] border border-indigo-200 bg-white p-4 shadow-2xl shadow-indigo-100/50 md:p-4';

  const inputClassName = variant === 'hero'
    ? 'mt-2 w-full rounded-2xl border border-slate-200 bg-white px-4 py-3 text-base font-semibold text-slate-950 outline-none transition placeholder:text-slate-500 focus:border-indigo-500 focus:ring-4 focus:ring-indigo-100'
    : 'mt-2 w-full rounded-[1.375rem] border border-slate-200 bg-slate-50 px-4 py-3.5 text-base font-extrabold text-slate-950 outline-none transition placeholder:text-slate-500 focus:border-indigo-300 focus:bg-white focus:ring-4 focus:ring-indigo-100';

  const labelClassName = variant === 'hero'
    ? 'block text-sm font-black text-slate-700'
    : 'block text-xs font-black uppercase tracking-[0.03em] text-slate-700';

  return (
    <form className={formClassName} onSubmit={handleSubmit}>
      <div className="grid gap-3 md:grid-cols-[1.35fr_0.9fr_0.95fr_0.95fr_auto] md:items-end">
        <label className={labelClassName}>
          Que necesitas
          <input
            className={inputClassName}
            onChange={(event) => setFilters((current) => ({ ...current, query: event.target.value }))}
            placeholder="Barberia, fisio, yoga..."
            value={filters.query}
          />
        </label>
        <label className={labelClassName}>
          Ciudad
          <input
            className={inputClassName}
            onChange={(event) => setFilters((current) => ({ ...current, city: event.target.value }))}
            placeholder="Madrid"
            value={filters.city}
          />
        </label>
        <label className={labelClassName}>
          Categoria
          <select
            className={inputClassName}
            onChange={(event) => setFilters((current) => ({ ...current, category: event.target.value }))}
            value={filters.category}
          >
            {marketplaceCategoryOptions.map((option) => (
              <option key={option.value || 'all'} value={option.value}>{option.label}</option>
            ))}
          </select>
        </label>
        <label className={labelClassName}>
          Service
          <input
            className={inputClassName}
            onChange={(event) => setFilters((current) => ({ ...current, service: event.target.value }))}
            placeholder="Corte, masaje..."
            value={filters.service}
          />
        </label>
        <button className="rounded-[1.375rem] bg-indigo-600 px-6 py-4 text-base font-black text-white shadow-xl shadow-indigo-200 transition hover:bg-indigo-700 focus:outline-none focus:ring-4 focus:ring-indigo-200 md:min-h-[3.625rem]" type="submit">
          {submitLabel}
        </button>
      </div>
    </form>
  );
}

export function PublicBusinessCard({ business }: { business: PublicBusinessCardResponse }) {
  const location = [business.city, business.countryCode].filter(Boolean).join(', ');

  return (
    <article className="group rounded-[2rem] border border-slate-200 bg-white p-5 shadow-sm transition hover:-translate-y-1 hover:border-indigo-200 hover:shadow-2xl hover:shadow-indigo-100/70 md:p-6">
      <div className="flex h-full flex-col gap-5">
        <div>
          <div className="flex flex-wrap items-center gap-2">
            {business.category ? <span className="rounded-full border border-indigo-100 bg-indigo-50 px-3 py-1.5 text-xs font-black text-indigo-800">{business.category}</span> : null}
            {location ? <span className="rounded-full border border-slate-200 bg-slate-100 px-3 py-1.5 text-xs font-black text-slate-600">{location}</span> : null}
          </div>
          <h2 className="mt-5 text-3xl font-black leading-[1.05] tracking-tight text-slate-950">{business.name}</h2>
          <p className="mt-4 line-clamp-3 text-sm leading-6 text-slate-600">
            {business.description || 'Business activo con services disponibles para explorar y reservar appointments.'}
          </p>
        </div>

        <div className="rounded-[1.5rem] border border-slate-200 bg-slate-50 p-4">
          <p className="text-xs font-black uppercase tracking-[0.16em] text-slate-600">Services destacados</p>
          {business.featuredServices.length > 0 ? (
            <div className="mt-3 grid gap-2">
              {business.featuredServices.map((service) => (
                <div className="flex items-center justify-between gap-3 text-sm" key={service.id}>
                  <span className="font-black text-slate-800">{service.name}</span>
                  <span className="shrink-0 font-extrabold text-slate-500">{service.durationMinutes} min</span>
                </div>
              ))}
            </div>
          ) : (
            <p className="mt-3 text-sm font-semibold text-slate-500">Services por publicar.</p>
          )}
        </div>

        <div className="mt-auto flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
          <p className="text-sm font-black text-slate-700">
            {business.startingPriceAmount == null ? 'Precio a consultar' : `Desde ${formatCurrency(business.startingPriceAmount, business.currencyCode)}`}
          </p>
          <div className="grid grid-cols-2 gap-2 sm:flex">
            <Link className="rounded-2xl border border-slate-200 bg-white px-4 py-3 text-center text-sm font-black text-slate-900 transition hover:bg-slate-50 focus:outline-none focus:ring-4 focus:ring-slate-100" to={routes.businessProfile(business.slug)}>
              Ver perfil
            </Link>
            <Link className="rounded-2xl bg-slate-950 px-4 py-3 text-center text-sm font-black text-white transition hover:bg-indigo-700 focus:outline-none focus:ring-4 focus:ring-indigo-200" to={routes.appointmentSlotFlow(business.slug)}>
              Reservar appointment
            </Link>
          </div>
        </div>
      </div>
    </article>
  );
}

function formatCurrency(amount: number, currencyCode: string) {
  return new Intl.NumberFormat(undefined, { currency: currencyCode, style: 'currency' }).format(amount);
}
