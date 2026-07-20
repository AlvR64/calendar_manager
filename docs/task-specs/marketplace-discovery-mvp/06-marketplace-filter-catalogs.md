# Marketplace Filter Catalogs

## Incremento

Marketplace Discovery MVP.

## IDs De Capacidad

- `1.2`
- `11.9`
- `11.11`
- `11.13`
- `11.14`

## Matriz De Estado Actual

Mirror del estado actual de `../../../task-groups-roadmap.md` para contexto rapido. El roadmap sigue siendo autoritativo.

| ID | Backend | Frontend | Diseno | Postpuesto |
| --- | --- | --- | --- | --- |
| `1.2` | [x] | [x] | [x] | [ ] |
| `11.9` | [x] | [x] | [ ] | [ ] |
| `11.11` | [x] | [x] | [ ] | [ ] |
| `11.13` | [ ] | [x] | [x] | [ ] |
| `11.14` | [ ] | [x] | [x] | [ ] |

## Objetivo

Normalizar los filtros publicos de marketplace para que las categorias usen codigos canonicos en ingles con labels en espanol, y para que la ciudad se elija desde un selector buscable de ciudades espanolas sin introducir tabla maestra en base de datos.

## Alcance

- Definir un catalogo frontend unico de categorias con value canonico y label visible.
- Usar valores canonicos en URLs y payloads, por ejemplo `/search?category=barber`.
- Mostrar labels en espanol en search, cards, perfil publico y admin settings.
- Actualizar admin settings para guardar valores canonicos.
- Anadir selector buscable de ciudades espanolas en filtros marketplace.
- Mantener ciudad como texto enviado al backend, por ejemplo `Madrid`.
- Mantener `query` y `service` como texto libre.
- Normalizar o migrar categorias persistidas antiguas si aplica.
- Actualizar seed/demo para usar categorias canonicas cuando aplique.
- Actualizar tests backend/frontend afectados.

## Fuera De Alcance

- Tabla maestra de ciudades.
- Endpoint backend de facetas o autocompletado dinamico.
- Geolocalizacion o distancia al customer.
- Ranking avanzado.
- Multi-sede.
- Redisenar visualmente `/search` mas alla de cambios necesarios para los filtros.

## Contrato Backend

El endpoint publico se mantiene, pero el valor semantico de `category` pasa a ser canonico en ingles.

| Endpoint / Use Case | Auth | Request | Success Response | Error Cases |
| --- | --- | --- | --- | --- |
| `GET /api/public/businesses` | Ninguna | Query `category=barber`, `city=Madrid`, resto de filtros actuales | `PublicBusinessSearchResponse` | `400`, errores API genericos |
| Business settings actuales | Admin | `category` con value canonico | Business response con `category` canonica | Validation/auth/not-found |

## Referencias De Diseno

- `designs/marketplace-search.op`
- `designs/business-public-profile.op`
- `designs/business-settings.op`

## Rutas Y Pantallas Frontend

- `/`
- `/search`
- `/b/:slug`
- `/admin/business-settings`

## Estados UX

- Categoria seleccionada desde catalogo canonico.
- Ciudad seleccionable/buscable desde lista frontend.
- Filtros activos con labels legibles.
- Resultados, loading, empty y error se mantienen como en search actual.

## Reglas De Datos Y Validacion

- Mapping inicial de categorias:
  - `barber` -> `Barberia`
  - `beauty` -> `Estetica`
  - `physiotherapy` -> `Fisioterapia`
  - `classes` -> `Clases`
  - `consulting` -> `Consultas`
- Backend mantiene `Business.Category` como string salvo decision posterior.
- URLs y valores guardados usan el codigo canonico.
- UI muestra siempre label en espanol.
- Para compatibilidad, valores antiguos como `Barberia` deben migrarse o normalizarse.
- Ciudad no requiere tabla maestra; el catalogo inicial vive en frontend.

## Plan De Implementacion

1. Crear catalogos frontend compartidos de categorias y ciudades.
2. Actualizar search/home para usar categorias canonicas y selector buscable de ciudad.
3. Actualizar cards, chips y perfil publico para mostrar labels.
4. Actualizar admin settings para guardar categorias canonicas.
5. Anadir migracion o normalizacion de datos existentes si aplica.
6. Actualizar seed/demo y tests backend/frontend.
7. Ejecutar checks.
8. Actualizar README del incremento y roadmap al completar.

## Plan De Tests

- Backend: busqueda por `category=barber` devuelve businesses con categoria canonica.
- Backend: datos existentes con categorias antiguas quedan normalizados si hay migracion.
- Frontend: Home category chip navega a `/search?category=barber`.
- Frontend: `/search?category=barber` muestra chip `Categoria: Barberia`.
- Frontend: cards/perfil/admin settings muestran labels en espanol.
- Frontend: ciudad permite buscar/seleccionar `Madrid` y mantiene `city=Madrid` en URL.
- Comandos: `dotnet build`, `dotnet test`, `npm run typecheck`, `npm run lint`, `npm test -- --run`, `npm run build`.

## Criterios De Aceptacion

- [x] Categorias se guardan y filtran con codigos canonicos en ingles.
- [x] La UI muestra labels de categorias en espanol.
- [x] URLs de search usan valores canonicos.
- [x] El selector de ciudades permite buscar ciudades espanolas.
- [x] No se introduce tabla maestra de ciudades.
- [x] Search, Home, perfil publico y admin settings quedan sincronizados.
- [x] Los tests/checks relevantes pasan.
- [x] `../../../task-groups-roadmap.md` esta actualizado tras completar.

## Preguntas Abiertas

- Ninguna.
