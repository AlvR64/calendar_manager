import { useQuery } from '@tanstack/react-query';
import { Link, useNavigate, useSearchParams } from 'react-router-dom';

import { getApiErrorMessage } from '@/api/apiErrors';
import { ApiErrorAlert } from '@/features/auth/authUi';
import { searchPublicBusinesses } from '@/features/publicBusiness/publicBusinessApi';
import {
  buildMarketplaceSearchPath,
  emptyMarketplaceSearchFilters,
  type MarketplaceSearchFilters,
} from '@/features/publicBusiness/marketplaceSearch';
import {
  BusinessSearchForm,
  PublicBusinessCard,
} from '@/features/publicBusiness/marketplaceSearchUi';
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
    <main className="min-h-screen bg-[radial-gradient(circle_at_top_left,#e0e7ff,transparent_32%),linear-gradient(135deg,#ffffff_0%,#f8fafc_55%,#eef2ff_100%)] px-6 py-8 text-slate-950">
      <div className="mx-auto max-w-6xl">
        <header className="flex flex-col gap-5 sm:flex-row sm:items-center sm:justify-between">
          <Link className="text-2xl font-black tracking-tight" to={routes.home}>Calendar Manager</Link>
          <Link className="rounded-full bg-slate-950 px-5 py-2.5 text-center text-sm font-black text-white shadow-lg shadow-slate-300/60 transition hover:bg-indigo-700" to={routes.businessRegister}>
            Publicar mi business
          </Link>
        </header>

        <section className="mt-12 grid gap-8 lg:grid-cols-[0.85fr_1.15fr] lg:items-end">
          <div>
            <p className="text-sm font-black uppercase tracking-[0.25em] text-indigo-600">Marketplace</p>
            <h1 className="mt-4 text-5xl font-black leading-[0.95] tracking-tight md:text-6xl">Encuentra el business adecuado para tu proximo appointment.</h1>
            <p className="mt-5 text-lg leading-8 text-slate-600">
              Filtra por texto, ciudad, categoria o service. Los resultados muestran solo businesses activos con datos ligeros para decidir rapido.
            </p>
          </div>
          <BusinessSearchForm initialFilters={filters} key={buildMarketplaceSearchPath(filters)} onSubmit={handleSubmit} submitLabel="Aplicar filtros" />
        </section>

        <section className="mt-8 rounded-[2rem] border border-white/80 bg-white/80 p-5 shadow-sm backdrop-blur md:p-6">
          <div className="flex flex-col gap-4 md:flex-row md:items-center md:justify-between">
            <div>
              <p className="text-sm font-black uppercase tracking-[0.2em] text-indigo-600">
                {hasFilters ? 'Resultados de busqueda' : 'Businesses recientes'}
              </p>
              <h2 className="mt-2 text-2xl font-black tracking-tight">
                {businessSearchQuery.data ? `${businessSearchQuery.data.totalCount} businesses encontrados` : 'Buscando businesses'}
              </h2>
            </div>
            {hasFilters ? <ActiveFilters filters={filters} /> : <p className="text-sm font-bold text-slate-500">Sin filtros activos</p>}
          </div>
        </section>

        <section className="mt-8">
          {businessSearchQuery.isPending ? <SearchSkeleton /> : null}
          {businessSearchQuery.isError ? <ApiErrorAlert message={getApiErrorMessage(businessSearchQuery.error)} /> : null}
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
    ['Categoria', filters.category],
    ['Service', filters.service],
  ].filter(([, value]) => value);

  return (
    <div className="flex flex-wrap gap-2">
      {activeFilters.map(([label, value]) => (
        <span className="rounded-full bg-indigo-50 px-3 py-1 text-xs font-black text-indigo-700" key={label}>{label}: {value}</span>
      ))}
      <Link className="rounded-full bg-slate-100 px-3 py-1 text-xs font-black text-slate-600 hover:bg-slate-200" to={routes.search}>Limpiar</Link>
    </div>
  );
}

function SearchPagination({ filters, hasNextPage, page }: { filters: MarketplaceSearchFilters; hasNextPage: boolean; page: number }) {
  return (
    <nav aria-label="Paginacion de resultados" className="mt-8 flex items-center justify-between gap-4 rounded-[2rem] border border-slate-200 bg-white p-4 shadow-sm">
      {page > 1 ? (
        <Link className="rounded-2xl border border-slate-200 px-5 py-3 text-sm font-black text-slate-800 hover:bg-slate-50" to={buildMarketplaceSearchPath(filters, page - 1)}>
          Anterior
        </Link>
      ) : (
        <span className="rounded-2xl border border-slate-100 px-5 py-3 text-sm font-black text-slate-300">Anterior</span>
      )}
      <span className="text-sm font-black text-slate-600">Pagina {page}</span>
      {hasNextPage ? (
        <Link className="rounded-2xl bg-slate-950 px-5 py-3 text-sm font-black text-white hover:bg-indigo-700" to={buildMarketplaceSearchPath(filters, page + 1)}>
          Siguiente
        </Link>
      ) : (
        <span className="rounded-2xl bg-slate-100 px-5 py-3 text-sm font-black text-slate-300">Siguiente</span>
      )}
    </nav>
  );
}

function SearchSkeleton() {
  return (
    <div className="grid gap-5 lg:grid-cols-3">
      {[0, 1, 2].map((item) => (
        <div className="h-80 animate-pulse rounded-[2rem] bg-slate-200" key={item} />
      ))}
    </div>
  );
}

function EmptySearchState({ hasFilters }: { hasFilters: boolean }) {
  return (
    <div className="rounded-[2rem] border border-dashed border-slate-300 bg-white/85 p-10 text-center shadow-sm">
      <p className="text-sm font-black uppercase tracking-[0.25em] text-indigo-600">Sin resultados</p>
      <h2 className="mt-4 text-3xl font-black tracking-tight text-slate-950">No encontramos businesses para esta busqueda</h2>
      <p className="mx-auto mt-4 max-w-xl text-slate-600">
        {hasFilters ? 'Prueba con menos filtros, otra ciudad o una categoria distinta.' : 'Todavia no hay businesses activos publicados para marketplace.'}
      </p>
      {hasFilters ? (
        <Link className="mt-7 inline-flex rounded-2xl bg-slate-950 px-6 py-3.5 text-base font-black text-white hover:bg-indigo-700" to={routes.search}>
          Ver todos
        </Link>
      ) : null}
    </div>
  );
}

function filtersFromSearchParams(searchParams: URLSearchParams): MarketplaceSearchFilters {
  return {
    category: searchParams.get('category') ?? emptyMarketplaceSearchFilters.category,
    city: searchParams.get('city') ?? emptyMarketplaceSearchFilters.city,
    query: searchParams.get('query') ?? emptyMarketplaceSearchFilters.query,
    service: searchParams.get('service') ?? emptyMarketplaceSearchFilters.service,
  };
}

function parsePage(value: string | null) {
  const parsed = Number(value);
  return Number.isInteger(parsed) && parsed > 0 ? parsed : 1;
}
