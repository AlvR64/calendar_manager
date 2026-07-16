# Admin Appointment Calendar And List

## Incremento

Appointments MVP.

## IDs De Capacidad

- `6.5`
- `6.6`
- `6.7`

## Matriz De Estado Actual

| ID | Backend | Frontend | Diseno | Postpuesto |
| --- | --- | --- | --- | --- |
| `6.5` | [ ] | [ ] | [ ] | [ ] |
| `6.6` | [ ] | [ ] | [ ] | [ ] |
| `6.7` | [ ] | [ ] | [ ] | [ ] |

## Objetivo

Permitir que el admin vea appointments del business en una vista operativa por rango de fechas, con filtros basicos.

## Alcance

- Listar appointments del business actual del admin.
- Filtrar por rango de fechas.
- Filtrar por staff member.
- Filtrar por service y status si el coste es bajo.
- Mostrar una lista agrupada por dia.
- Preparar la base para una vista calendario simple.

## Fuera De Alcance

- Drag/drop.
- Reprogramacion.
- Crear appointment como admin.
- Reporting avanzado.
- Exportaciones.

## Contrato Backend

| Endpoint / Use Case | Auth | Request | Success Response | Error Cases |
| --- | --- | --- | --- | --- |
| `GET /api/appointments` | Admin | Query `from`, `to`, `staffMemberId?`, `serviceId?`, `status?` | `200 AppointmentSummaryResponse[]` | `400`, `401`, `403` |

### Reglas Backend

- Business id sale del claim `business_id` del admin.
- `from` y `to` son UTC o fechas locales documentadas; recomendacion MVP: aceptar `date` local/rango local y resolver con timezone del business.
- El rango debe estar limitado para evitar listados grandes; sugerido maximo 90 dias.
- Ordenar por `StartAtUtc` ascendente.
- Incluir datos suficientes para lista/calendario: service snapshot, staff display name, customer name/email, status y horarios.

## Referencias De Diseno

- TBD. No existe diseno especifico todavia.

## Rutas Y Pantallas Frontend

- `/admin/appointments`

## Estados UX

- Loading.
- Empty range.
- Empty filtered results.
- Error.
- Unauthorized/forbidden.
- Success grouped by day.

## Reglas De Datos Y Validacion

- Mostrar tiempos en timezone del business.
- Default recomendado: rango de hoy a 14 dias.
- Filtro staff debe usar staff members admin existentes.
- Filtro service debe usar services admin existentes.

## Plan De Implementacion

1. Crear query backend para listar appointments admin.
2. Anadir metodos repositorio con filtros por business/rango/staff/service/status.
3. Crear endpoint `GET /api/appointments` para admin.
4. Anadir tests de filtros, business isolation y rango invalido.
5. Anadir ruta admin `/admin/appointments` y link en layout.
6. Anadir API/types frontend.
7. Crear filtros y lista agrupada por dia.
8. Cubrir estados UX.
9. Actualizar roadmap al completar.

## Plan De Tests

- Backend: filters, date range validation, business isolation.
- Frontend: list rendering, filters, empty/error states.
- Comandos: `dotnet build`, `dotnet test`, `npm run typecheck`, `npm run lint`, `npm test -- --run`, `npm run build`.

## Criterios De Aceptacion

- [ ] Admin ve appointments de su business.
- [ ] Admin no ve appointments de otro business.
- [ ] Filtros por rango y staff funcionan.
- [ ] UI muestra appointments agrupados por dia.
- [ ] Estados empty/error estan cubiertos.
- [ ] Roadmap actualizado.

## Preguntas Abiertas

- Decidir si el primer UI debe ser lista agrupada solamente o lista + calendario semanal simple.
