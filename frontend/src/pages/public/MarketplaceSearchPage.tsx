import { useQuery } from '@tanstack/react-query';
import { Link, useNavigate, useSearchParams } from 'react-router-dom';

import { getApiErrorMessage } from '@/api/apiErrors';
import { searchPublicBusinesses } from '@/features/publicBusiness/publicBusinessApi';
import {
  buildMarketplaceSearchPath,
  emptyMarketplaceSearchFilters,
  getMarketplaceCategoryLabel,
  normalizeMarketplaceCategoryValue,
  type MarketplaceSearchFilters,
} from '@/features/publicBusiness/marketplaceSearch';
import {
  BusinessSearchForm,
  PublicBusinessCard,
} from '@/features/publicBusiness/marketplaceSearchUi';
import { PublicHeader } from '@/layouts/PublicHeader';
import { routes } from '@/lib/routes';

const pageSize = 9;

export function MarketplaceSearchPage() {
  const [searchParams] = useSearchParams();
  const navigate = useNavigate();
  const filters = filtersFromSearchParams(searchParams);
  const page = parsePage(searchParams.get('page'));
  const hasFilters = Object.values(filters).some(Boolean);

  const businessSearchQuery = useQuery({
    queryFn: () => searchPublicBusinesses({ ...filters, page, pageSize }),
    queryKey: ['public', 'business-search', filters, page, pageSize],
  });

  function handleSubmit(nextFilters: MarketplaceSearchFilters) {
    navigate(buildMarketplaceSearchPath(nextFilters));
  }

  return (
    <main className="min-h-screen bg-slate-50 text-slate-950">
      <PublicHeader className="border-b border-slate-200 bg-white" maxWidthClassName="max-w-7xl" />

      <div className="mx-auto max-w-7xl px-6 py-10 md:px-8 lg:px-10">
        <section className="grid gap-8 lg:grid-cols-[minmax(0,1fr)_24rem] lg:items-start">
          <div className="max-w-3xl">
            <p className="text-xs font-black uppercase tracking-[0.28em] text-indigo-600">Marketplace</p>
            <h1 className="mt-4 text-5xl font-black leading-[0.95] tracking-tight md:text-6xl">
              Encuentra el business adecuado para tu proximo appointment.
            </h1>
            <p className="mt-6 max-w-2xl text-lg leading-8 text-slate-600">
              Filtra por ciudad, categoria o service y entra directo al perfil publico o al flujo de reserva.
            </p>
          </div>
          <aside className="rounded-[1.875rem] border border-indigo-200 bg-white p-6 shadow-xl shadow-indigo-100/60">
            <p className="text-xs font-black uppercase tracking-[0.18em] text-indigo-600">Busqueda publica</p>
            <h2 className="mt-3 text-2xl font-black leading-tight tracking-tight">Resultados activos, ligeros y listos para reservar.</h2>
            <p className="mt-4 text-sm font-medium leading-6 text-slate-600">
              Cada card muestra categoria, ciudad, services destacados, precio desde y acciones claras.
            </p>
            <div className="mt-5 flex flex-wrap gap-2">
              <span className="rounded-full border border-indigo-100 bg-indigo-50 px-3 py-1.5 text-xs font-black text-indigo-800">4 filtros</span>
              <span className="rounded-full border border-slate-200 bg-slate-100 px-3 py-1.5 text-xs font-black text-slate-600">sin login</span>
            </div>
          </aside>
        </section>

        <section className="mt-8" aria-label="Filtros de marketplace">
          <BusinessSearchForm initialFilters={filters} key={buildMarketplaceSearchPath(filters)} onSubmit={handleSubmit} submitLabel="Aplicar filtros" />
        </section>

        <section className="mt-10 rounded-[1.875rem] border border-slate-200 bg-white p-5 shadow-sm md:p-6">
          <div className="flex flex-col gap-5 md:flex-row md:items-center md:justify-between">
            <div>
              <p className="text-xs font-black uppercase tracking-[0.18em] text-indigo-600">
                {hasFilters ? 'Resultados de busqueda' : 'Businesses recientes'}
              </p>
              <h2 className="mt-2 text-2xl font-black tracking-tight">
                {businessSearchQuery.data ? `${businessSearchQuery.data.totalCount} businesses encontrados` : 'Buscando businesses'}
              </h2>
            </div>
            {hasFilters ? <ActiveFilters filters={filters} /> : <p className="rounded-full bg-slate-100 px-4 py-2 text-sm font-black text-slate-600">Sin filtros activos</p>}
          </div>
        </section>

        <section className="mt-8">
          {businessSearchQuery.isPending ? <SearchSkeleton /> : null}
          {businessSearchQuery.isError ? <SearchErrorState message={getApiErrorMessage(businessSearchQuery.error)} /> : null}
          {businessSearchQuery.isSuccess && businessSearchQuery.data.items.length === 0 ? <EmptySearchState hasFilters={hasFilters} /> : null}
          {businessSearchQuery.isSuccess && businessSearchQuery.data.items.length > 0 ? (
            <>
              <div className="grid gap-5 lg:grid-cols-3">
                {businessSearchQuery.data.items.map((business) => (
                  <PublicBusinessCard business={business} key={business.id} />
                ))}
              </div>
              <SearchPagination filters={filters} hasNextPage={businessSearchQuery.data.hasNextPage} page={businessSearchQuery.data.page} />
            </>
          ) : null}
        </section>
      </div>
    </main>
  );
}

function ActiveFilters({ filters }: { filters: MarketplaceSearchFilters }) {
  const activeFilters = [
    ['Texto', filters.query],
    ['Ciudad', filters.city],
    ['Categoria', getMarketplaceCategoryLabel(filters.category)],
    ['Service', filters.service],
  ].filter(([, value]) => value);

  return (
    <div className="flex flex-wrap gap-2 md:justify-end">
      {activeFilters.map(([label, value]) => (
        <span className="rounded-full border border-indigo-100 bg-indigo-50 px-3 py-1.5 text-xs font-black text-indigo-800" key={label}>{label}: {value}</span>
      ))}
      <Link className="rounded-2xl bg-indigo-600 px-4 py-2 text-xs font-black text-white shadow-sm transition hover:bg-indigo-700 focus:outline-none focus:ring-4 focus:ring-indigo-200" to={routes.search}>Limpiar filtros</Link>
    </div>
  );
}

function SearchPagination({ filters, hasNextPage, page }: { filters: MarketplaceSearchFilters; hasNextPage: boolean; page: number }) {
  return (
    <nav aria-label="Paginacion de resultados" className="mt-8 flex items-center justify-between gap-4 rounded-[1.75rem] border border-slate-200 bg-white p-3 shadow-sm sm:p-4">
      {page > 1 ? (
        <Link className="rounded-2xl border border-slate-200 px-4 py-3 text-sm font-black text-slate-900 transition hover:bg-slate-50 sm:px-5" to={buildMarketplaceSearchPath(filters, page - 1)}>
          Anterior
        </Link>
      ) : (
        <span className="rounded-2xl border border-slate-100 px-4 py-3 text-sm font-black text-slate-500 sm:px-5">Anterior</span>
      )}
      <span className="text-sm font-black text-slate-600">Pagina {page}</span>
      {hasNextPage ? (
        <Link className="rounded-2xl bg-slate-950 px-4 py-3 text-sm font-black text-white transition hover:bg-indigo-700 sm:px-5" to={buildMarketplaceSearchPath(filters, page + 1)}>
          Siguiente
        </Link>
      ) : (
        <span className="rounded-2xl bg-slate-100 px-4 py-3 text-sm font-black text-slate-500 sm:px-5">Siguiente</span>
      )}
    </nav>
  );
}

function SearchSkeleton() {
  return (
    <div className="grid gap-5 lg:grid-cols-3">
      {[0, 1, 2].map((item) => (
        <div className="rounded-[2rem] border border-slate-200 bg-white p-6 shadow-sm" key={item}>
          <div className="h-8 w-44 animate-pulse rounded-full bg-slate-200" />
          <div className="mt-6 h-8 w-3/4 animate-pulse rounded-full bg-slate-200" />
          <div className="mt-4 h-4 w-full animate-pulse rounded-full bg-slate-100" />
          <div className="mt-2 h-4 w-5/6 animate-pulse rounded-full bg-slate-100" />
          <div className="mt-6 h-32 animate-pulse rounded-[1.5rem] bg-slate-100" />
          <div className="mt-6 flex items-center justify-between">
            <div className="h-4 w-24 animate-pulse rounded-full bg-slate-200" />
            <div className="h-11 w-32 animate-pulse rounded-2xl bg-slate-200" />
          </div>
        </div>
      ))}
    </div>
  );
}

function EmptySearchState({ hasFilters }: { hasFilters: boolean }) {
  return (
    <div className="rounded-[2rem] border border-dashed border-slate-300 bg-white p-10 text-center shadow-sm">
      <p className="text-xs font-black uppercase tracking-[0.25em] text-indigo-600">Sin resultados</p>
      <h2 className="mt-4 text-3xl font-black tracking-tight text-slate-950">No encontramos businesses para esta busqueda</h2>
      <p className="mx-auto mt-4 max-w-xl text-slate-600">
        {hasFilters ? 'Prueba con menos filtros, otra ciudad o una categoria distinta.' : 'Todavia no hay businesses activos publicados para marketplace.'}
      </p>
      {hasFilters ? (
        <Link className="mt-7 inline-flex rounded-2xl bg-slate-950 px-6 py-3.5 text-base font-black text-white transition hover:bg-indigo-700" to={routes.search}>
          Ver todos
        </Link>
      ) : null}
    </div>
  );
}

function SearchErrorState({ message }: { message: string }) {
  return (
    <div className="rounded-[2rem] border border-red-200 bg-red-50 p-8 shadow-sm" role="alert">
      <p className="text-xs font-black uppercase tracking-[0.22em] text-red-700">Error API</p>
      <h2 className="mt-3 text-2xl font-black tracking-tight text-red-950">No pudimos cargar la busqueda</h2>
      <p className="mt-3 max-w-2xl text-sm font-bold leading-6 text-red-700">{message}</p>
    </div>
  );
}

function filtersFromSearchParams(searchParams: URLSearchParams): MarketplaceSearchFilters {
  return {
    category: normalizeMarketplaceCategoryValue(searchParams.get('category') ?? emptyMarketplaceSearchFilters.category),
    city: searchParams.get('city') ?? emptyMarketplaceSearchFilters.city,
    query: searchParams.get('query') ?? emptyMarketplaceSearchFilters.query,
    service: searchParams.get('service') ?? emptyMarketplaceSearchFilters.service,
  };
}

function parsePage(value: string | null) {
  const parsed = Number(value);
  return Number.isInteger(parsed) && parsed > 0 ? parsed : 1;
}
