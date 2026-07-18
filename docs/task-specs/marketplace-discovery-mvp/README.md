# Specs De Entrega Del Marketplace Discovery MVP

Estado: planificado.

Incremento: Marketplace Discovery MVP.

Estas specs convierten la landing publica en un hub real de descubrimiento para que customers encuentren businesses por texto, ciudad, categoria o service, y lleguen al perfil publico o al flujo de appointment.

`../../../task-groups-roadmap.md` sigue siendo la fuente de verdad del estado. Estas specs documentan alcance, contratos esperados, plan de implementacion y criterios de aceptacion.

## Decisiones Del Incremento

- La categoria de business empieza como campo simple controlado, no como taxonomia compleja.
- La busqueda usa SQL Server y filtros basicos; no se introduce motor externo tipo Algolia/Elasticsearch.
- El endpoint de search sirve tanto para resultados como para destacados/recientes de la landing.
- Los resultados publicos muestran cards ligeras; el perfil publico sigue siendo la fuente de detalle.
- El flujo de appointment existente se reutiliza desde resultados de busqueda.

## Alcance Incluido

- Categoria/tipo publico de business.
- Seed demo con varios businesses para validar el marketplace.
- Endpoint publico de busqueda/listado de businesses activos.
- Home con buscador real.
- Pagina `/search` con filtros y resultados.
- Cards publicas de business con services destacados y navegacion a perfil/appointment.

## Alcance Excluido

- Mapas y geolocalizacion real.
- Distancia por ubicacion del customer.
- Reviews, ratings y favoritos.
- Imagenes/media.
- Ranking inteligente o sponsored results.
- Busqueda con motor externo.
- Multi-sede.

## Specs

| Spec | Estado | IDs de capacidad | Notas |
| --- | --- | --- | --- |
| `01-business-category-and-demo-marketplace-seed.md` | Hecha | `1.8`, `11.11` | Categoria simple y datos demo para mostrar varios businesses. |
| `02-public-business-search-api.md` | Planificada | `11.9`, `11.10` | API publica para buscar/listar businesses activos y cards ligeras. |
| `03-marketplace-search-frontend.md` | Planificada | `11.13`, `11.14`, `11.15` | Home con buscador, `/search`, cards y navegacion a perfil/appointment. |

## Orden Recomendado

1. `01-business-category-and-demo-marketplace-seed.md`
2. `02-public-business-search-api.md`
3. `03-marketplace-search-frontend.md`

## Regla De Finalizacion

Una spec se considera completa solo cuando:

- Backend, frontend y diseno aplicables estan implementados o marcados explicitamente como no aplicables/postpuestos.
- Los contratos backend documentados existen y tienen tests relevantes.
- La UI usa APIs reales, no placeholders.
- Loading, empty, error, validation, unauthorized/forbidden y success states estan cubiertos donde aplica.
- Los checks relevantes pasan.
- `../../../task-groups-roadmap.md` queda actualizado.
