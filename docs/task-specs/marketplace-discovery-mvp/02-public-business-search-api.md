# Public Business Search API

## Incremento

Marketplace Discovery MVP.

## IDs De Capacidad

- `11.9`
- `11.10`

## Matriz De Estado Actual

Mirror del estado actual de `../../../task-groups-roadmap.md` para contexto rapido. El roadmap sigue siendo autoritativo.

| ID | Backend | Frontend | Diseno | Postpuesto |
| --- | --- | --- | --- | --- |
| `11.9` | [ ] | [ ] | [ ] | [ ] |
| `11.10` | [ ] | [ ] | [ ] | [ ] |

## Objetivo

Ofrecer un endpoint publico para que customers descubran businesses activos por texto, ciudad, categoria o service, con datos suficientes para pintar cards marketplace.

## Alcance

- Crear endpoint publico de busqueda/listado de businesses activos.
- Soportar filtros `query`, `city`, `category`, `service`, `page`, `pageSize`.
- Buscar por business name, description, city, category, service name y service description.
- Devolver resultados paginados con datos ligeros de card.
- Soportar modo destacados/recientes cuando no hay filtros.

## Fuera De Alcance

- Ranking inteligente.
- Busqueda fuzzy avanzada.
- Motor externo de busqueda.
- Geolocalizacion/distancia.
- Reviews/ratings.

## Contrato Backend

| Endpoint / Use Case | Auth | Request | Success Response | Error Cases |
| --- | --- | --- | --- | --- |
| `GET /api/public/businesses` | Ninguna | Query: `query?`, `city?`, `category?`, `service?`, `page?`, `pageSize?` | `PublicBusinessSearchResponse` | `400` para paginacion/filtros invalidos |

### Response Esperada

- `items`: lista de cards publicas.
- `page`, `pageSize`, `totalCount`, `hasNextPage`.
- Card: `id`, `slug`, `name`, `description`, `city`, `countryCode`, `category`, `timeZoneId`, `currencyCode`, `featuredServices`, `startingPriceAmount`.

## Referencias De Diseno

- Pendiente. Diseno nuevo recomendado para `/search` antes o durante spec frontend.

## Rutas Y Pantallas Frontend

- Consumido por `/` y `/search` en `03-marketplace-search-frontend.md`.

## Estados UX

- No aplica directamente; endpoint debe soportar loading/empty/error desde frontend.

## Reglas De Datos Y Validacion

- Solo retornar businesses activos.
- Solo services activos en `featuredServices`.
- `pageSize` limitado para evitar consultas grandes.
- Orden inicial recomendado: destacados/recientes o nombre estable si no hay ranking.

## Plan De Implementacion

1. Crear contratos `PublicBusinessSearchRequest/Response` o equivalentes.
2. Extender repositorio/query publica de businesses.
3. Implementar filtros y paginacion basica.
4. Mapear services destacados y precio desde.
5. Crear endpoint controller publico.
6. Anadir tests backend de filtros/paginacion.
7. Actualizar `../../../task-groups-roadmap.md` al completar.

## Plan De Tests

- Backend tests: sin filtros, query por business, city, category, service, solo activos, paginacion.
- Manual: requests con combinaciones simples de filtros.
- Comandos: `dotnet test`, `dotnet build`.

## Criterios De Aceptacion

- [ ] El endpoint devuelve businesses activos paginados.
- [ ] Los filtros principales funcionan de forma combinable.
- [ ] La response contiene datos suficientes para cards marketplace.
- [ ] Los tests/checks relevantes pasan.
- [ ] `../../../task-groups-roadmap.md` esta actualizado tras completar.

## Preguntas Abiertas

- Orden exacto de resultados hasta que exista ranking real.
