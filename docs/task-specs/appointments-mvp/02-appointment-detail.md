# Appointment Detail

Estado: done.

## Incremento

Appointments MVP.

## IDs De Capacidad

- `6.3`

## Matriz De Estado Actual

| ID | Backend | Frontend | Diseno | Postpuesto |
| --- | --- | --- | --- | --- |
| `6.3` | [x] | [x] | [ ] | [ ] |

## Objetivo

Proveer una pagina recargable de confirmacion/detalle para appointments creados.

## Alcance

- Obtener appointment por id.
- Permitir acceso al customer propietario.
- Permitir acceso al admin del business.
- Mostrar datos principales del appointment con hora local del business.
- Usar esta pantalla como destino despues de crear appointment.

## Fuera De Alcance

- Cancelar appointment desde esta spec.
- Cambiar estado.
- Reprogramar.
- Mostrar historial/auditoria.

## Contrato Backend

| Endpoint / Use Case | Auth | Request | Success Response | Error Cases |
| --- | --- | --- | --- | --- |
| `GET /api/appointments/{appointmentId}` | Customer/Admin | `appointmentId` route param | `200 AppointmentDetailsResponse` | `401`, `403`, `404` |

### `AppointmentDetailsResponse`

- `id`
- `business: { id, name, slug, timeZoneId }`
- `service: { id, nameSnapshot, durationMinutesSnapshot, priceAmountSnapshot, currencyCodeSnapshot }`
- `staffMember: { id, displayName }`
- `customer: { id, firstName, lastName, email }`
- `startAtUtc`
- `endAtUtc`
- `localDate`
- `startTime`
- `endTime`
- `status`
- `customerNotes`
- `internalNotes?` solo para admin si se decide exponerlo en esta respuesta.
- `cancelledAtUtc`
- `cancellationReason`
- `createdAtUtc`

## Referencias De Diseno

- TBD. No existe diseno especifico todavia.

## Rutas Y Pantallas Frontend

- `/appointments/:appointmentId`

## Estados UX

- Loading.
- Not found.
- Unauthorized/forbidden.
- Success.
- Appointment cancelled.
- Generic error.

## Reglas De Datos Y Validacion

- Customer solo ve sus appointments.
- Admin solo ve appointments de su business.
- Mostrar fecha/hora en timezone del business.
- Mantener `startAtUtc`/`endAtUtc` disponibles para debug/API consistency si es util.

## Plan De Implementacion

1. Crear query backend `GetAppointmentById`.
2. Anadir repositorio para obtener appointment con business, service, staff member y customer.
3. Crear endpoint `GET /api/appointments/{appointmentId}`.
4. Mapear access rules por role/claim.
5. Anadir tests backend de owner, admin same business, forbidden y not found.
6. Anadir tipos/API frontend.
7. Crear ruta `/appointments/:appointmentId`.
8. Mostrar resumen de confirmacion/detalle.
9. Actualizar link de exito desde creacion.
10. Actualizar roadmap al completar.

## Plan De Tests

- Backend: controller/query auth matrix y mapping de response.
- Frontend: loading, success, not found, forbidden/error.
- Comandos: `dotnet build`, `dotnet test`, `npm run typecheck`, `npm run lint`, `npm test -- --run`, `npm run build`.

## Criterios De Aceptacion

- [x] Appointment creado puede abrirse por URL.
- [x] Customer no puede ver appointments de otros customers.
- [x] Admin no puede ver appointments de otro business.
- [x] Fecha/hora se muestran en timezone del business.
- [x] Estados de loading/error/not-found estan cubiertos.
- [x] Roadmap actualizado.

## Preguntas Abiertas

- Decidir si `internalNotes` se devuelve en detalle cuando el solicitante es admin o solo en endpoints admin.
