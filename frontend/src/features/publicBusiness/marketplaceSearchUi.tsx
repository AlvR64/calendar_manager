import type { FormEvent } from 'react';
import { useState } from 'react';
import { Link } from 'react-router-dom';

import type { PublicBusinessCardResponse } from '@/api/contracts';
import { inputClassName, labelClassName } from '@/features/auth/authUi';
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
    : 'rounded-[2rem] border border-slate-200 bg-white p-5 shadow-sm md:p-6';

  return (
    <form className={formClassName} onSubmit={handleSubmit}>
      <div className="grid gap-4 md:grid-cols-[1.25fr_0.9fr_0.9fr_0.9fr_auto] md:items-end">
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
        <button className="rounded-2xl bg-indigo-600 px-6 py-4 text-base font-black text-white shadow-xl shadow-indigo-200 transition hover:bg-indigo-700" type="submit">
          {submitLabel}
        </button>
      </div>
    </form>
  );
}

export function PublicBusinessCard({ business }: { business: PublicBusinessCardResponse }) {
  const location = [business.city, business.countryCode].filter(Boolean).join(', ');

  return (
    <article className="rounded-[2rem] border border-slate-200 bg-white p-5 shadow-sm transition hover:-translate-y-0.5 hover:shadow-xl hover:shadow-indigo-100 md:p-6">
      <div className="flex h-full flex-col gap-5">
        <div>
          <div className="flex flex-wrap items-center gap-2">
            {business.category ? <span className="rounded-full bg-indigo-50 px-3 py-1 text-xs font-black text-indigo-700">{business.category}</span> : null}
            {location ? <span className="rounded-full bg-slate-100 px-3 py-1 text-xs font-black text-slate-600">{location}</span> : null}
          </div>
          <h2 className="mt-4 text-2xl font-black tracking-tight text-slate-950">{business.name}</h2>
          <p className="mt-3 line-clamp-3 text-sm leading-6 text-slate-600">
            {business.description || 'Business activo con services disponibles para explorar y reservar appointments.'}
          </p>
        </div>

        <div className="rounded-[1.5rem] bg-slate-50 p-4">
          <p className="text-xs font-black uppercase tracking-[0.18em] text-slate-500">Services destacados</p>
          {business.featuredServices.length > 0 ? (
            <div className="mt-3 grid gap-2">
              {business.featuredServices.map((service) => (
                <div className="flex items-center justify-between gap-3 text-sm" key={service.id}>
                  <span className="font-black text-slate-800">{service.name}</span>
                  <span className="shrink-0 font-bold text-slate-500">{service.durationMinutes} min</span>
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
          <div className="flex gap-2">
            <Link className="rounded-2xl border border-slate-200 bg-white px-4 py-3 text-center text-sm font-black text-slate-800 hover:bg-slate-50" to={routes.businessProfile(business.slug)}>
              Ver perfil
            </Link>
            <Link className="rounded-2xl bg-slate-950 px-4 py-3 text-center text-sm font-black text-white hover:bg-indigo-700" to={routes.appointmentSlotFlow(business.slug)}>
              Elegir appointment
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
