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
  { label: 'Barberia', value: 'barber' },
  { label: 'Estetica', value: 'beauty' },
  { label: 'Fisioterapia', value: 'physiotherapy' },
  { label: 'Clases', value: 'classes' },
  { label: 'Consultas', value: 'consulting' },
];

export const businessCategoryOptions = [
  { label: 'Sin categoria', value: '' },
  ...marketplaceCategoryOptions.filter((option) => option.value),
];

export const spanishCityOptions = [
  'A Coruna',
  'Albacete',
  'Alcala de Henares',
  'Alcobendas',
  'Alicante',
  'Almeria',
  'Avila',
  'Badajoz',
  'Barcelona',
  'Bilbao',
  'Burgos',
  'Caceres',
  'Cadiz',
  'Cartagena',
  'Castellon de la Plana',
  'Ceuta',
  'Cordoba',
  'Cuenca',
  'Donostia-San Sebastian',
  'Elche',
  'Getafe',
  'Girona',
  'Gijon',
  'Granada',
  'Guadalajara',
  'Huelva',
  'Huesca',
  'Jaen',
  'Las Palmas de Gran Canaria',
  'Leganes',
  'Leon',
  'Lleida',
  'Logrono',
  'Lugo',
  'Madrid',
  'Malaga',
  'Marbella',
  'Melilla',
  'Murcia',
  'Ourense',
  'Oviedo',
  'Palencia',
  'Palma',
  'Pamplona',
  'Pontevedra',
  'Sabadell',
  'Salamanca',
  'Santa Cruz de Tenerife',
  'Santander',
  'Santiago de Compostela',
  'Segovia',
  'Sevilla',
  'Tarragona',
  'Terrassa',
  'Toledo',
  'Valencia',
  'Valladolid',
  'Vigo',
  'Vitoria-Gasteiz',
  'Zamora',
  'Zaragoza',
];

const legacyCategoryValues: Record<string, string> = {
  Barberia: 'barber',
  Clases: 'classes',
  Consultas: 'consulting',
  Estetica: 'beauty',
  Fisioterapia: 'physiotherapy',
};

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
    category: normalizeMarketplaceCategoryValue(filters.category),
    city: filters.city.trim(),
    query: filters.query.trim(),
    service: filters.service.trim(),
  };
}

export function getMarketplaceCategoryLabel(value?: string | null) {
  const normalizedValue = normalizeMarketplaceCategoryValue(value ?? '');
  return marketplaceCategoryOptions.find((option) => option.value === normalizedValue)?.label ?? value ?? '';
}

export function normalizeMarketplaceCategoryValue(value: string) {
  const trimmedValue = value.trim();
  return legacyCategoryValues[trimmedValue] ?? trimmedValue;
}

function appendOptionalParam(searchParams: URLSearchParams, key: string, value: string) {
  if (value) {
    searchParams.set(key, value);
  }
}
