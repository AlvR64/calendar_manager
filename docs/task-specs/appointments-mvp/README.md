# Specs De Entrega Del Appointments MVP

Estado: planificado.

Incremento: Appointments MVP.

Estas specs convierten el flujo publico actual de seleccion de slot en un flujo real de appointments, y anaden las vistas basicas para customer y admin.

`../../../task-groups-roadmap.md` sigue siendo la fuente de verdad del estado. Estas specs documentan alcance, contratos esperados, plan de implementacion y criterios de aceptacion.

## Decisiones Del Incremento

- La confirmacion de appointment requiere customer autenticado.
- La seleccion publica de slot sigue sin requerir login.
- No se permite guest checkout en este incremento.
- No se anade `SlotHold`; la proteccion contra doble reserva se hace al crear el appointment.
- No se anaden payments.
- No se anaden notificaciones email.
- El estado inicial del appointment es `Scheduled`.
- No se anade reprogramacion en este incremento.
- No se anade creacion de appointment por admin en este incremento.

## Alcance Incluido

- Crear appointment real como customer desde un slot seleccionado.
- Proteger creacion contra doble reserva concurrente.
- Mostrar confirmacion/detalle de appointment.
- Listar appointments del customer autenticado.
- Cancelar appointment como customer.
- Listar appointments del business para admin con filtros basicos.
- Permitir acciones admin basicas sobre appointments existentes.
- Alimentar dashboard admin basico con datos reales de appointments.

## Alcance Excluido

- Reserva invitada.
- Payments.
- Notificaciones.
- Reprogramacion.
- Slot holds temporales.
- Integracion con calendarios externos.
- Reporting avanzado.

## Specs

| Spec | Estado | IDs de capacidad | Notas |
| --- | --- | --- | --- |
| `01-customer-appointment-creation.md` | Hecha | `6.1`, `6.14`, `5.12`, `6.13` | Appointment real creado desde slot seleccionado; diseno especifico pendiente. |
| `02-appointment-detail.md` | Hecha | `6.3` | Detalle recargable protegido para customer propietario o admin del business. |
| `03-customer-appointment-list-and-cancellation.md` | Hecha | `6.4`, `6.8` | Customer ve sus appointments y cancela appointments scheduled futuros. |
| `04-admin-appointment-calendar-and-list.md` | Planificada | `6.5`, `6.6`, `6.7` | Admin lista/calendario con filtros por rango y staff. |
| `05-admin-appointment-actions.md` | Planificada | `6.9`, `6.11`, `6.12` | Admin cancela, cambia estado y guarda notas internas. |
| `06-dashboard-basics.md` | Planificada | `10.1` | Dashboard admin basico basado en appointments reales. |

## Orden Recomendado

1. `01-customer-appointment-creation.md`
2. `02-appointment-detail.md`
3. `03-customer-appointment-list-and-cancellation.md`
4. `04-admin-appointment-calendar-and-list.md`
5. `05-admin-appointment-actions.md`
6. `06-dashboard-basics.md`

## Regla De Finalizacion

Una spec se considera completa solo cuando:

- Backend, frontend y diseno aplicables estan implementados o marcados explicitamente como no aplicables/postpuestos.
- Los contratos backend documentados existen y tienen tests relevantes.
- La UI usa APIs reales, no placeholders.
- Loading, empty, error, validation, unauthorized/forbidden y success states estan cubiertos donde aplica.
- Los checks relevantes pasan.
- `../../../task-groups-roadmap.md` queda actualizado.
