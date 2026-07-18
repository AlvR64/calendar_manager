# Business Category And Demo Marketplace Seed

## Incremento

Marketplace Discovery MVP.

## IDs De Capacidad

- `1.8`
- `11.11`

## Matriz De Estado Actual

Mirror del estado actual de `../../../task-groups-roadmap.md` para contexto rapido. El roadmap sigue siendo autoritativo.

| ID | Backend | Frontend | Diseno | Postpuesto |
| --- | --- | --- | --- | --- |
| `1.8` | [x] | [x] | [ ] | [ ] |
| `11.11` | [x] | [x] | [ ] | [ ] |

## Objetivo

Permitir clasificar businesses por sector para que la busqueda marketplace pueda filtrar y mostrar resultados utiles desde datos demo realistas.

## Alcance

- Anadir categoria/tipo publico al business.
- Exponer categoria en contracts de business settings y profile publico.
- Permitir al admin configurar categoria desde business settings.
- Actualizar seed/dev data con varios businesses demo, cada uno con categoria, ciudad, services y staff suficientes para navegar.
- Crear migration EF si cambia el modelo persistido.

## Fuera De Alcance

- Taxonomia jerarquica de categorias.
- Categorias multiples por business.
- Busqueda publica; se cubre en `02-public-business-search-api.md`.
- Imagenes/logo de businesses.

## Contrato Backend

| Endpoint / Use Case | Auth | Request | Success Response | Error Cases |
| --- | --- | --- | --- | --- |
| Business settings existentes | Admin | Campo `category` en create/update/settings cuando aplique | Responses incluyen `category` | Validacion existente + longitud/campo requerido si se decide |
| Public profile existente | Ninguna | Slug/business id | Profile incluye `category` | `404` |

## Referencias De Diseno

- Pendiente. Reusar visual actual de business settings y perfil publico.

## Rutas Y Pantallas Frontend

- `/admin/business-settings`
- `/b/:slug`

## Estados UX

- Loading de settings/profile existente.
- Validation error si categoria es invalida.
- Success al guardar settings.

## Reglas De Datos Y Validacion

- Categoria como string controlado inicialmente, por ejemplo `Barberia`, `Estetica`, `Fisioterapia`, `Clases`, `Consultas`.
- Guardar valor normalizado de forma consistente para filtros.
- Mostrar una etiqueta legible en UI.

## Plan De Implementacion

1. Actualizar entidad/configuracion EF de business y migration.
2. Actualizar request/response DTOs backend y mappers.
3. Actualizar business settings frontend y tipos TS.
4. Actualizar profile publico para mostrar categoria.
5. Actualizar seed/dev data con varios businesses demo.
6. Anadir/actualizar tests backend y frontend relevantes.
7. Actualizar `../../../task-groups-roadmap.md` al completar.

## Plan De Tests

- Backend tests de validacion/mapping de categoria.
- Frontend tests de business settings y profile publico.
- Manual: seed muestra varios businesses con categorias distintas.
- Comandos: `dotnet test`, `npm run typecheck`, `npm run lint`, `npm test -- --run`, `npm run build`.

## Criterios De Aceptacion

- [x] Admin puede ver/editar categoria del business.
- [x] Profile publico expone y muestra categoria.
- [x] Seed demo contiene varios businesses categorizados.
- [x] Los tests/checks relevantes pasan.
- [x] `../../../task-groups-roadmap.md` esta actualizado tras completar.

## Preguntas Abiertas

- Lista inicial usada en UI: `Barberia`, `Estetica`, `Fisioterapia`, `Clases`, `Consultas`.
