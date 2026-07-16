# Customer Appointment List And Cancellation

## Incremento

Appointments MVP.

## IDs De Capacidad

- `6.4`
- `6.8`

## Matriz De Estado Actual

| ID | Backend | Frontend | Diseno | Postpuesto |
| --- | --- | --- | --- | --- |
| `6.4` | [ ] | [ ] | [ ] | [ ] |
| `6.8` | [ ] | [ ] | [ ] | [ ] |

## Objetivo

Permitir que un customer autenticado vea sus appointments y cancele appointments futuros.

## Alcance

- Listar appointments del customer autenticado.
- Filtrar por rango/estado de forma basica.
- Cancelar appointment como customer.
- Mostrar appointments proximos, pasados y cancelados.

## Fuera De Alcance

- Reprogramar.
- Cambiar estado a completed/no-show.
- Cancelaciones con politicas avanzadas o fees.
- Notificaciones.

## Contrato Backend

| Endpoint / Use Case | Auth | Request | Success Response | Error Cases |
| --- | --- | --- | --- | --- |
| `GET /api/customers/current/appointments` | Customer | Query `from?`, `to?`, `status?` | `200 AppointmentSummaryResponse[]` | `401`, `403` |
| `POST /api/customers/current/appointments/{appointmentId}/cancel` | Customer | `CancelAppointmentRequest` | `200 AppointmentDetailsResponse` | `400`, `401`, `403`, `404`, `409` |

### `CancelAppointmentRequest`

- `cancellationReason?: string | null`

### Reglas Backend

- Customer id sale del token.
- Customer solo lista/cancela sus appointments.
- No se puede cancelar appointment ya cancelado.
- Para MVP, `Scheduled` es cancelable por customer.
- Setear `Status = CancelledByCustomer`.
- Setear `CancelledAtUtc`.
- Guardar `CancellationReason` si se envia.

## Referencias De Diseno

- TBD. No existe diseno especifico todavia.

## Rutas Y Pantallas Frontend

- `/customer/appointments`

## Estados UX

- Loading.
- Empty upcoming appointments.
- Empty filtered results.
- Error.
- Unauthorized.
- Cancel confirmation.
- Cancelling.
- Cancel success.
- Cancel conflict/already cancelled.

## Reglas De Datos Y Validacion

- Mostrar fecha/hora en timezone del business.
- Separar visualmente upcoming, past y cancelled donde sea practico.
- `cancellationReason` opcional, maximo recomendado 500 caracteres.

## Plan De Implementacion

1. Crear query backend para appointments del customer actual.
2. Crear command backend para cancelacion customer.
3. Anadir endpoints customer current.
4. Anadir tests de auth, ownership, list filters y cancellation states.
5. Anadir tipos/API frontend.
6. Anadir ruta protegida customer.
7. Construir pagina de lista y accion cancelar.
8. Invalidar/refrescar queries tras cancelar.
9. Actualizar roadmap al completar.

## Plan De Tests

- Backend: list by customer, no cross-customer access, cancel success, cancel invalid state.
- Frontend: empty, list, cancel modal, cancel success/error.
- Comandos: `dotnet build`, `dotnet test`, `npm run typecheck`, `npm run lint`, `npm test -- --run`, `npm run build`.

## Criterios De Aceptacion

- [ ] Customer ve sus appointments.
- [ ] Customer no ve appointments de otros customers.
- [ ] Customer puede cancelar appointment permitido.
- [ ] Cancelacion actualiza estado y timestamp.
- [ ] UI refleja estado cancelado tras la accion.
- [ ] Roadmap actualizado.

## Preguntas Abiertas

- Definir si se bloquea cancelacion customer para appointments ya pasados en este MVP o si basta con estado `Scheduled`.
