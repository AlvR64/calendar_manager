# <Spec Name>

## Incremento

<Nombre del incremento>.

## IDs De Capacidad

- `<id>`

## Matriz De Estado Actual

Mirror del estado actual de `task-groups-roadmap.md` para contexto rapido. El roadmap sigue siendo autoritativo.

| ID | Backend | Frontend | Diseno | Postpuesto |
| --- | --- | --- | --- | --- |
| `<id>` | [ ] | [ ] | [ ] | [ ] |

## Objetivo

Resultado de usuario/business que esta slice debe entregar.

## Alcance

- Que incluye esta spec.

## Fuera De Alcance

- Que queda deliberadamente fuera, aunque este relacionado.

## Contrato Backend

Documentar el comportamiento backend del que depende la feature/frontend.

| Endpoint / Use Case | Auth | Request | Success Response | Error Cases |
| --- | --- | --- | --- | --- |
| `METHOD /path` | Requerida/Ninguna | DTO/body/query | DTO/status | Validation/auth/not-found/conflict |

## Referencias De Diseno

- `designs/<file>.op`

## Rutas Y Pantallas Frontend

- `/route`

## Estados UX

- Loading
- Empty
- Error
- Validation errors
- Success
- Unauthorized/forbidden cuando aplique

## Reglas De Datos Y Validacion

- La validacion backend sigue siendo autoritativa.
- Replicar validaciones simples en cliente cuando sea util.
- Documentar manejo de timezone, currency y fechas cuando aplique.

## Plan De Implementacion

1. Anadir o actualizar tipos/API client.
2. Anadir o actualizar query/mutation hooks.
3. Construir forms/components/screens.
4. Conectar rutas/comportamiento de pagina.
5. Cubrir estados loading, empty, error, validation y success.
6. Anadir o actualizar tests.
7. Actualizar `task-groups-roadmap.md` al completar.

## Plan De Tests

- Unit/component tests:
- Integration/manual checks:
- Comandos a ejecutar:

## Criterios De Aceptacion

- [ ] La feature satisface los IDs de capacidad listados arriba.
- [ ] La UI sigue el diseno OpenPencil referenciado donde exista.
- [ ] El contrato backend se respeta.
- [ ] Loading, empty, error, validation y success states estan manejados donde aplique.
- [ ] Los tests/checks relevantes pasan.
- [ ] `task-groups-roadmap.md` esta actualizado tras completar.

## Preguntas Abiertas

- Ninguna.
