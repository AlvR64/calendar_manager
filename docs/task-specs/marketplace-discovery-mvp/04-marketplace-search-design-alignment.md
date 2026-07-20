# Marketplace Search Design Alignment

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
| `11.13` | [ ] | [x] | [x] | [ ] |
| `11.14` | [ ] | [x] | [x] | [ ] |
| `11.15` | [ ] | [x] | [x] | [ ] |

## Objetivo

Dar a `/search` un diseno OpenPencil propio y pulido, alineado con el nivel visual del home, perfil publico y flujo de appointment.

## Alcance

- Crear `designs/marketplace-search.op`.
- Cubrir desktop y mobile.
- Disenar estados de resultados, loading, empty y error.
- Disenar filtros activos y paginacion `Anterior` / `Siguiente`.
- Mantener cards con categoria, ciudad, services destacados, precio desde y navegacion.
- Actualizar referencias de diseno de la spec frontend y roadmap.

## Fuera De Alcance

- Aplicar el diseno al frontend actual.
- Cambiar contratos backend o frontend.
- Anadir mapas, imagenes, reviews, favoritos o ranking inteligente.

## Contrato Backend

No cambia. El diseno sigue dependiendo del contrato existente.

| Endpoint / Use Case | Auth | Request | Success Response | Error Cases |
| --- | --- | --- | --- | --- |
| `GET /api/public/businesses` | Ninguna | Query filtros/paginacion | `PublicBusinessSearchResponse` | `400`, errores API genericos |

## Referencias De Diseno

- `designs/marketplace-search.op`
- `designs/homepage.op`
- `designs/business-public-profile.op`

## Rutas Y Pantallas Frontend

- `/search`
- `/`
- Navegacion hacia `/b/:slug`
- Navegacion hacia `/b/:slug/appointment`

## Estados UX

- Resultados.
- Loading con skeleton cards.
- Empty con CTA para ver todos.
- Error API.
- Filtros activos.
- Paginacion por botones.
- Version mobile stacked.

## Reglas De Datos Y Validacion

- Los filtros del diseno deben seguir los query params actuales: `query`, `city`, `category`, `service`, `page`.
- El diseno no recomputa disponibilidad; las cards solo muestran datos de search.
- Las acciones primarias siguen siendo perfil publico y flujo de appointment.

## Plan De Implementacion

1. Crear `designs/marketplace-search.op` con desktop, mobile y estados.
2. Actualizar `03-marketplace-search-frontend.md` para referenciar el diseno oficial.
3. Actualizar `README.md` del incremento.
4. Actualizar `../../../task-groups-roadmap.md` con `Diseno [x]` para `11.13`, `11.14`, `11.15`.
5. No modificar frontend hasta una futura slice de alineacion visual.

## Plan De Tests

- Revision manual del `.op` en OpenPencil.
- Validar que el `.op` es JSON valido.
- No aplica ejecutar checks de frontend/backend porque no hay cambios de codigo.

## Criterios De Aceptacion

- [x] Existe `designs/marketplace-search.op`.
- [x] El diseno cubre desktop y mobile.
- [x] El diseno cubre resultados, loading, empty, error, filtros activos y paginacion.
- [x] La spec frontend referencia el diseno oficial.
- [x] `../../../task-groups-roadmap.md` esta actualizado.

## Preguntas Abiertas

- Ninguna. La aplicacion del diseno al frontend queda para una futura slice.
