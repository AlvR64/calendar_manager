# Admin Appointment Actions

## Incremento

Appointments MVP.

## IDs De Capacidad

- `6.9`
- `6.11`
- `6.12`

## Matriz De Estado Actual

| ID | Backend | Frontend | Diseno | Postpuesto |
| --- | --- | --- | --- | --- |
| `6.9` | [ ] | [ ] | [ ] | [ ] |
| `6.11` | [ ] | [ ] | [ ] | [ ] |
| `6.12` | [ ] | [ ] | [ ] | [ ] |

## Objetivo

Permitir que el admin gestione appointments existentes: cancelar, cambiar estado operativo y guardar notas internas.

## Alcance

- Cancelar appointment como admin.
- Cambiar estado a `Scheduled`, `Completed` o `NoShow`.
- Guardar notas internas.
- Integrar acciones en `/admin/appointments`.

## Fuera De Alcance

- Estado `Confirmed`; no existe en el enum actual.
- Reprogramacion.
- Creacion por admin.
- Auditoria completa.
- Notificaciones.

## Contrato Backend

| Endpoint / Use Case | Auth | Request | Success Response | Error Cases |
| --- | --- | --- | --- | --- |
| `POST /api/appointments/{appointmentId}/cancel` | Admin | `CancelAppointmentRequest` | `200 AppointmentDetailsResponse` | `400`, `401`, `403`, `404`, `409` |
| `PUT /api/appointments/{appointmentId}/status` | Admin | `UpdateAppointmentStatusRequest` | `200 AppointmentDetailsResponse` | `400`, `401`, `403`, `404`, `409` |
| `PUT /api/appointments/{appointmentId}/internal-notes` | Admin | `UpdateAppointmentInternalNotesRequest` | `200 AppointmentDetailsResponse` | `400`, `401`, `403`, `404` |

### Requests

- `CancelAppointmentRequest.cancellationReason?: string | null`
- `UpdateAppointmentStatusRequest.status: Scheduled | Completed | NoShow`
- `UpdateAppointmentInternalNotesRequest.internalNotes?: string | null`

### Reglas Backend

- Admin solo puede actuar sobre appointments de su business.
- No se puede cancelar appointment ya cancelado.
- Cancelacion admin setea `Status = CancelledByAdmin`, `CancelledAtUtc` y `CancellationReason`.
- Cambio de estado no debe permitir estados cancelados; cancelacion usa endpoint dedicado.
- `internalNotes` maximo 1000 caracteres.
- `UpdatedAtUtc` se actualiza en cambios.

## Referencias De Diseno

- TBD. No existe diseno especifico todavia.

## Rutas Y Pantallas Frontend

- `/admin/appointments`

## Estados UX

- Action loading.
- Confirm cancel.
- Status update success/error.
- Notes save success/error.
- Conflict/already cancelled.
- Forbidden/not found.

## Reglas De Datos Y Validacion

- Mostrar acciones permitidas segun estado actual.
- Ocultar o deshabilitar acciones sobre appointments cancelados cuando no apliquen.
- Notas internas no son visibles para customer.

## Plan De Implementacion

1. Crear commands backend para cancel admin, update status y update internal notes.
2. Anadir metodos repositorio para obtener appointment for update por business.
3. Anadir endpoints admin en `AppointmentsController`.
4. Anadir tests de business isolation, invalid states y success.
5. Anadir API/types frontend.
6. Integrar acciones en `/admin/appointments`.
7. Refrescar lista/detalle tras acciones.
8. Actualizar roadmap al completar.

## Plan De Tests

- Backend: cancel admin, status transitions permitidas, forbidden cross-business, notes validation.
- Frontend: action buttons, confirm cancel, save notes, error states.
- Comandos: `dotnet build`, `dotnet test`, `npm run typecheck`, `npm run lint`, `npm test -- --run`, `npm run build`.

## Criterios De Aceptacion

- [ ] Admin cancela appointment de su business.
- [ ] Admin no actua sobre appointments de otro business.
- [ ] Admin cambia estado operativo permitido.
- [ ] Admin guarda notas internas.
- [ ] Customer no ve notas internas.
- [ ] Roadmap actualizado.

## Preguntas Abiertas

- Decidir si `Completed`/`NoShow` solo se permiten para appointments pasados o tambien manualmente en cualquier momento para MVP.
