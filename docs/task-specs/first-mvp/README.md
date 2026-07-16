# Specs De Entrega Del First MVP

Estado: baseline completada.

Incremento: First MVP.

Estas specs de entrega documentan como backend, frontend y diseno se sincronizaron para la baseline inicial usable del producto.

`../../../task-groups-roadmap.md` sigue siendo la fuente de verdad del estado. Las specs de esta carpeta son guias de implementacion y registro historico.

Las specs completadas se conservan como registro historico y referencia de regresion. No se borran salvo que su contexto historico se migre a otro lugar.

## Alcance

Incluido:

- Shell frontend, landing publica y comportamiento del layout admin.
- Pantallas de auth de customer y admin respaldadas por el backend existente.
- Registro y settings de business.
- Services admin.
- Staff members admin.
- Asignaciones staff-service.
- Disponibilidad y excepciones.
- Perfil publico de business.
- Flujo publico hasta seleccionar un slot de appointment.

Excluido:

- Crear appointments.
- Listar appointments.
- Cancelar o reprogramar appointments.
- Metricas de dashboard/reporting.
- Notificaciones.
- Media uploads.
- Integraciones con calendarios externos.

## Specs

Esta tabla es un indice rapido de ejecucion. `../../../task-groups-roadmap.md` sigue siendo autoritativo para el estado de backend, frontend y diseno.

| Spec | Estado | IDs de capacidad | Notas |
| --- | --- | --- | --- |
| `01-auth-and-registration.md` | Hecha | `1.1`, `7.1`, `7.2`, `8.1` | Frontend implementado y roadmap actualizado. |
| `02-business-settings.md` | Hecha | `0.11`, `1.2`, `1.6` | Frontend implementado y roadmap actualizado. |
| `03-services.md` | Hecha | `2.1`-`2.5` | Frontend implementado y roadmap actualizado. |
| `04-staff-members.md` | Hecha | `3.1`-`3.5` | Frontend implementado y roadmap actualizado. |
| `05-staff-service-assignments.md` | Hecha | `4.1`-`4.6` | Frontend implementado; endpoints admin de listado de asignaciones anadidos y roadmap actualizado. |
| `06-availability.md` | Hecha | `5.1`-`5.8` | Frontend implementado y roadmap actualizado; `5.12` queda para appointment creation. |
| `07-public-business-profile.md` | Hecha | `0.10`, `11.1`-`11.7` | Frontend implementado y roadmap actualizado. |
| `08-public-slot-flow.md` | Hecha | `0.12`, `5.9`, `5.10`, `5.11`, `11.3`, `11.4`, `11.6` | Frontend implementado; se detiene antes de crear appointments. |

Estas specs ya no son la cola activa de trabajo. Crea nuevas specs de entrega desde `../TEMPLATE.md` para nuevas slices, preferiblemente en una carpeta por incremento como `../appointments-mvp/` cuando empiece el trabajo de appointments.

## Regla Historica De Finalizacion

Una slice del First MVP se consideraba completa solo cuando:

- La UI frontend relevante estaba implementada mas alla de placeholders.
- Usaba el contrato backend o un estado mock/dev documentado intencionalmente.
- Los estados loading, empty, error, validation y success estaban cubiertos donde aplicaba.
- Los checks relevantes pasaban.
- `task-groups-roadmap.md` estaba actualizado.
