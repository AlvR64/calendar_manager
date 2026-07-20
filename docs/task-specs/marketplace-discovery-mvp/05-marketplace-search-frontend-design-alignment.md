# Marketplace Search Frontend Design Alignment

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

Alinear la implementacion real de `/search` con el diseno OpenPencil refinado para que el marketplace publico tenga una experiencia visual consistente en desktop y mobile.

## Alcance

- Aplicar `designs/marketplace-search.op` a la pagina `/search`.
- Ajustar el header, hero, panel de filtros, resumen, chips activos, cards, paginacion y estados.
- Mantener los filtros compartibles por query params.
- Mantener la navegacion desde cards a perfil publico y flujo de appointment.
- Preservar el uso compartido del formulario con Home sin degradar la landing.
- Actualizar tests frontend afectados por copy o estructura accesible.

## Fuera De Alcance

- Cambios backend.
- Cambios en `GET /api/public/businesses`.
- Cambios de ranking, mapas, reviews, imagenes, favoritos o filtros nuevos.
- Rehacer el diseno OpenPencil salvo correcciones menores detectadas durante la implementacion.

## Contrato Backend

No cambia. Depende del endpoint publico existente.

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
- Loading con skeleton cards/paneles.
- Empty con CTA `Ver todos` cuando hay filtros.
- Error API con `role="alert"`.
- Filtros activos y accion de limpiar.
- Paginacion `Anterior` / `Siguiente`.
- Version mobile stacked.

## Reglas De Datos Y Validacion

- Mantener `query`, `city`, `category`, `service` y `page` en query params.
- El backend sigue siendo autoritativo para resultados y paginacion.
- No recomputar disponibilidad en cards.
- Usar `Appointment`, no `Booking`, en copy nuevo.

## Plan De Implementacion

1. Ajustar `MarketplaceSearchPage` a la estructura visual del diseno refinado.
2. Ajustar `BusinessSearchForm` preservando accesibilidad y variantes de Home/search.
3. Ajustar `PublicBusinessCard` a las cards del diseno.
4. Integrar loading, empty, error, filtros activos y paginacion con el nuevo estilo.
5. Actualizar tests frontend afectados.
6. Ejecutar checks frontend.
7. Actualizar README del incremento y roadmap al completar.

## Plan De Tests

- Component tests de `/search` para resultados, filtros, cards, paginacion, empty y error.
- Component test de Home para confirmar que el formulario compartido sigue navegando a `/search`.
- Manual: revisar `/search` en desktop y mobile contra `designs/marketplace-search.op`.
- Comandos: `npm run typecheck`, `npm run lint`, `npm test -- --run`, `npm run build`.

## Criterios De Aceptacion

- [x] `/search` sigue el diseno OpenPencil refinado.
- [x] Desktop y mobile conservan una jerarquia clara y usable.
- [x] El contrato backend se respeta sin cambios.
- [x] Loading, empty, error, filtros activos, resultados y paginacion estan manejados.
- [x] Home no queda degradado por cambios al formulario compartido.
- [x] Los tests/checks relevantes pasan.
- [x] `../../../task-groups-roadmap.md` esta actualizado tras completar.

## Preguntas Abiertas

- Ninguna.
