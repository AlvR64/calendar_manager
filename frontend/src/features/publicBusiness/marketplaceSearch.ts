import { routes } from '@/lib/routes';

export type MarketplaceSearchFilters = {
  query: string;
  city: string;
  category: string;
  service: string;
};

export const emptyMarketplaceSearchFilters: MarketplaceSearchFilters = {
  category: '',
  city: '',
  query: '',
  service: '',
};

export const marketplaceCategoryOptions = [
  { label: 'Todas', value: '' },
  { label: 'Barberia', value: 'Barberia' },
  { label: 'Estetica', value: 'Estetica' },
  { label: 'Fisioterapia', value: 'Fisioterapia' },
  { label: 'Clases', value: 'Clases' },
  { label: 'Consultas', value: 'Consultas' },
];

export function buildMarketplaceSearchPath(filters: MarketplaceSearchFilters, page = 1) {
  const searchParams = new URLSearchParams();
  const normalizedFilters = normalizeSearchFilters(filters);

  appendOptionalParam(searchParams, 'query', normalizedFilters.query);
  appendOptionalParam(searchParams, 'city', normalizedFilters.city);
  appendOptionalParam(searchParams, 'category', normalizedFilters.category);
  appendOptionalParam(searchParams, 'service', normalizedFilters.service);

  if (page > 1) {
    searchParams.set('page', String(page));
  }

  const queryString = searchParams.toString();
  return `${routes.search}${queryString ? `?${queryString}` : ''}`;
}

export function normalizeSearchFilters(filters: MarketplaceSearchFilters): MarketplaceSearchFilters {
  return {
    category: filters.category.trim(),
    city: filters.city.trim(),
    query: filters.query.trim(),
    service: filters.service.trim(),
  };
}

function appendOptionalParam(searchParams: URLSearchParams, key: string, value: string) {
  if (value) {
    searchParams.set(key, value);
  }
}
