# Customer Appointment Creation

## Incremento

Appointments MVP.

## IDs De Capacidad

- `6.1`
- `6.14`
- `5.12`
- `6.13`

## Matriz De Estado Actual

Mirror de `../../../task-groups-roadmap.md` al crear la spec. El roadmap sigue siendo autoritativo.

| ID | Backend | Frontend | Diseno | Postpuesto |
| --- | --- | --- | --- | --- |
| `6.1` | [ ] | [ ] | [ ] | [ ] |
| `6.14` | [ ] | [ ] | [ ] | [ ] |
| `5.12` | [x] | [ ] | [ ] | [ ] |
| `6.13` | [ ] | [ ] | [ ] | [ ] |

## Objetivo

Permitir que un customer autenticado confirme un appointment real desde el slot seleccionado en `/b/:slug/appointment`.

## Alcance

- Reutilizar el flujo publico existente de seleccion de service, staff member opcional, fecha y slot.
- Exigir customer login solo al confirmar appointment.
- Preservar la seleccion al ir a login/register y volver.
- Crear appointment con customer autenticado.
- Guardar notas visibles del customer al crear el appointment.
- Validar disponibilidad, excepciones, ventana de reserva y solapes.
- Proteger contra doble reserva concurrente.
- Mostrar estado de exito y link al detalle de appointment.

## Fuera De Alcance

- Reserva invitada.
- Payments.
- Notificaciones email.
- Slot holds.
- Reprogramacion.
- Creacion de appointment por admin.

## Contrato Backend

| Endpoint / Use Case | Auth | Request | Success Response | Error Cases |
| --- | --- | --- | --- | --- |
| `POST /api/appointments` | Customer | `CreateAppointmentRequest` | `201 AppointmentResponse` | `400`, `401`, `403`, `404`, `409` |

### `CreateAppointmentRequest`

- `businessId: Guid`
- `serviceId: Guid`
- `staffMemberId: Guid`
- `startAtUtc: DateTimeOffset`
- `customerNotes?: string | null`

El backend calcula `endAtUtc` desde la duracion actual del service activo. No se acepta `endAtUtc` del cliente para evitar manipulacion de duracion.

### `AppointmentResponse`

- `id`
- `businessId`
- `staffMemberId`
- `serviceId`
- `customerId`
- `startAtUtc`
- `endAtUtc`
- `status`
- `customerNotes`
- `serviceNameSnapshot`
- `serviceDurationMinutesSnapshot`
- `priceAmountSnapshot`
- `currencyCodeSnapshot`
- `createdAtUtc`

### Reglas Backend

- Customer id sale del token `Customer`, no del request.
- Business debe existir y estar activo.
- Service debe estar activo y pertenecer al business.
- Staff member debe estar activo y pertenecer al business.
- Staff member debe estar asignado activamente al service.
- Business timezone debe ser un IANA timezone id valido.
- Fecha local debe estar dentro de `MaxAdvanceBookingDays`.
- Appointment debe caer dentro de disponibilidad y fuera de excepciones.
- Appointment no puede solapar con appointments activos del staff member.
- Appointments cancelados por customer/admin no bloquean solape.
- Estado inicial: `Scheduled`.
- Snapshots se toman en el momento de creacion.

### Concurrencia

- Ejecutar en transaccion.
- Tomar lock transaccional SQL Server con `sp_getapplock`.
- Resource sugerido: `appointment:{businessId}:{staffMemberId}:{localDate}`.
- Dentro del lock, revalidar disponibilidad, excepciones, ventana y solape.
- Insertar appointment atomicamente.
- Si el lock no se obtiene o aparece solape, devolver `409 Conflict`.

## Referencias De Diseno

- Reutiliza parcialmente `designs/public-appointment-slot-flow.op`.
- Falta diseno especifico para bloque de confirmacion autenticada y exito.

## Rutas Y Pantallas Frontend

- `/b/:slug/appointment`
- `/auth/customer/login`
- `/auth/customer/register`
- `/appointments/:appointmentId` despues de confirmar, cubierto por `02-appointment-detail.md`.

## Estados UX

- Loading profile.
- Loading slots.
- Slot selected.
- Customer no autenticado.
- Redirect a login/register manteniendo seleccion.
- Creating appointment.
- Success.
- Validation error.
- Slot already taken / conflict.
- Unauthorized/forbidden.
- Generic API error.

## Reglas De Datos Y Validacion

- Appointment instants son UTC-first.
- Mostrar tiempos al customer en timezone del business.
- Preferir datos de slot devueltos por backend.
- Preservar seleccion con search params o sessionStorage.
- Search params recomendados: `serviceId`, `staffMemberId`, `date`, `startAtUtc`.
- `customerNotes` maximo recomendado: 1000 caracteres, alineado con entidad.

## Plan De Implementacion

1. Anadir contratos backend `CreateAppointmentRequest` y `AppointmentResponse`.
2. Extender `IAppointmentRepository` con add/get y persistencia necesaria.
3. Crear command/handler `CreateAppointment`.
4. Reutilizar `IAppointmentScheduleValidator` dentro de transaccion y lock.
5. Crear `AppointmentsController` con `POST /api/appointments`.
6. Anadir tests backend de happy path, auth, validation, overlap y concurrency conflict.
7. Anadir tipos/API frontend para crear appointment.
8. Actualizar `/b/:slug/appointment` con confirmacion autenticada.
9. Preservar seleccion al pasar por login/register.
10. Anadir tests frontend para login requerido, confirmacion y conflict.
11. Actualizar `task-groups-roadmap.md` al completar.

## Plan De Tests

- Backend: command handler, validator integration, controller auth/error mapping, repository overlap.
- Frontend: seleccion persistida, no-auth CTA, submit success, conflict, validation error.
- Comandos: `dotnet build`, `dotnet test`, `npm run typecheck`, `npm run lint`, `npm test -- --run`, `npm run build`.

## Criterios De Aceptacion

- [ ] Customer autenticado puede crear appointment real desde un slot.
- [ ] Customer no autenticado no puede confirmar y recibe CTA a login/register.
- [ ] La seleccion sobrevive login/register.
- [ ] Backend calcula `endAtUtc` y snapshots.
- [ ] Doble reserva concurrente devuelve `409` y no crea dos appointments.
- [ ] Solapes con appointments activos se bloquean.
- [ ] Appointments cancelados no bloquean slots.
- [ ] UI muestra success y link al detalle.
- [ ] Roadmap actualizado.

## Preguntas Abiertas

- Definir copy final para el bloque de confirmacion cuando se cree el diseno especifico.
