# Marketplace Search Frontend

## Incremento

Marketplace Discovery MVP.

## IDs De Capacidad

- `11.13`
- `11.14`
- `11.15`

## Matriz De Estado Actual

Mirror del estado actual de `../../../task-groups-roadmap.md` para contexto rapido. El roadmap sigue siendo autoritativo.

| ID | Backend | Frontend | Diseno | Postpuesto |
| --- | --- | --- | --- | --- |
| `11.13` | [ ] | [ ] | [ ] | [ ] |
| `11.14` | [ ] | [ ] | [ ] | [ ] |
| `11.15` | [ ] | [ ] | [ ] | [ ] |

## Objetivo

Dar al customer una entrada centralizada para buscar businesses y reservar servicios desde resultados marketplace.

## Alcance

- Convertir el home en entrada a busqueda real.
- Crear ruta `/search` con query params compartibles.
- Mostrar resultados como cards de business.
- Permitir filtrar por texto, ciudad y categoria.
- Navegar desde card a perfil publico y flujo de appointment.
- Mostrar destacados/recientes si no hay filtros.

## Fuera De Alcance

- Mapas.
- Reviews/ratings.
- Favoritos.
- Imagenes/media.
- Ranking inteligente.
- Filtros por disponibilidad inmediata.

## Contrato Backend

Depende de `GET /api/public/businesses` definido en `02-public-business-search-api.md`.

| Endpoint / Use Case | Auth | Request | Success Response | Error Cases |
| --- | --- | --- | --- | --- |
| `GET /api/public/businesses` | Ninguna | Query filtros/paginacion | `PublicBusinessSearchResponse` | `400`, errores API genericos |

## Referencias De Diseno

- `designs/homepage.op` como base visual.
- Nuevo diseno recomendado: `designs/marketplace-search.op`.

## Rutas Y Pantallas Frontend

- `/`
- `/search`
- Navegacion hacia `/b/:slug`
- Navegacion hacia `/b/:slug/appointment`

## Estados UX

- Loading de resultados.
- Empty con sugerencias para ampliar busqueda.
- Error API.
- Filtros activos.
- Paginacion o cargar mas si hay `hasNextPage`.

## Reglas De Datos Y Validacion

- Mantener filtros en query params para compartir URLs.
- El backend sigue siendo autoritativo para resultados.
- No recomputar disponibilidad en cards; usar solo datos de busqueda hasta entrar al flujo de appointment.

## Plan De Implementacion

1. Anadir tipos TS y API client de busqueda publica.
2. Actualizar home con formulario de busqueda real.
3. Crear pagina `/search` y registrar ruta.
4. Crear cards de business reutilizables.
5. Conectar filtros con query params y TanStack Query.
6. Cubrir loading, empty, error y resultados.
7. Anadir tests frontend.
8. Actualizar `../../../task-groups-roadmap.md` al completar.

## Plan De Tests

- Component tests de home search y pagina `/search`.
- Tests de cards con links a perfil y appointment.
- Manual: buscar por nombre, ciudad, categoria y service.
- Comandos: `npm run typecheck`, `npm run lint`, `npm test -- --run`, `npm run build`.

## Criterios De Aceptacion

- [ ] Home permite iniciar una busqueda real.
- [ ] `/search` muestra resultados desde API real.
- [ ] Cards navegan a perfil y appointment del business.
- [ ] Loading, empty y error estan manejados.
- [ ] Los tests/checks relevantes pasan.
- [ ] `../../../task-groups-roadmap.md` esta actualizado tras completar.

## Preguntas Abiertas

- Si la paginacion UI sera por botones de pagina o `Cargar mas`.
