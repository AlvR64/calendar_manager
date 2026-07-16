# Roadmap De Calendar Manager

Este archivo es la fuente unica del roadmap. Backend, frontend, diseno y specs deben mantenerse sincronizados aqui.

## Como Leer Este Roadmap

- El roadmap se organiza primero por incrementos de producto, que son bloques de entrega legibles.
- Los IDs como `6.1`, `5.12` o `11.3` son IDs de capacidad estables.
- Una spec de entrega puede cubrir varios IDs de capacidad de distintos grupos.
- El mapa de capacidades mantiene el estado granular; no define por si solo el orden de implementacion.
- Al completar una spec, se actualiza el incremento correspondiente y el mapa de capacidades.
- Las specs completadas se conservan como registro historico y referencia de regresion.

Leyenda del mapa de capacidades:

- `Backend`: capacidad implementada actualmente en backend.
- `Frontend`: capacidad implementada actualmente en frontend con UI/API real, no solo placeholder.
- `Diseno`: diseno creado actualmente en OpenPencil.
- `Postpuesto`: capacidad que queda deliberadamente para mas adelante o no aplica aun.
- Si los checks estan vacios, la capacidad esta pendiente en esa parte.

## Estado Base Actual

Primer MVP baseline completada. Las specs de entrega estan en `docs/task-specs/first-mvp/`.

Alcance incluido:

- Shell frontend, landing publica y layout admin.
- Registro/login de admin y customer para el MVP.
- Registro y settings de business.
- CRUD admin de services y staff members.
- Asignaciones staff-service.
- Disponibilidad semanal y excepciones por staff member.
- Perfil publico de business.
- Flujo publico hasta seleccion local de slot de appointment.
- Health check backend simple.

Alcance excluido del primer MVP:

- Creacion real de appointments.
- Listado, detalle, cancelacion y reprogramacion de appointments.
- Dashboard/reporting.
- Notificaciones.
- Media uploads.
- Integraciones externas de calendario.
- Multi-sede.

Notas de sincronizacion:

- `13.1` es backend-only; no requiere frontend ni diseno.
- `5.12` queda pendiente para frontend/diseno hasta que exista el flujo real de creacion de appointments.
- Las specs completadas del primer MVP se conservan como registro historico y referencia de regresion.

## Incremento Activo: Appointments MVP

| Slice | Estado | Spec | IDs de capacidad | Notas |
| --- | --- | --- | --- | --- |
| Crear appointment como customer | Hecho | `docs/task-specs/appointments-mvp/01-customer-appointment-creation.md` | `6.1`, `6.14`, `5.12`, `6.13` | Crea appointments reales despues de seleccionar slot; diseno especifico queda pendiente. |
| Confirmacion y detalle de appointment | Hecho | `docs/task-specs/appointments-mvp/02-appointment-detail.md` | `6.3` | Detalle recargable protegido para customer propietario o admin del business; diseno especifico queda pendiente. |
| Lista de appointments del customer | Planificado | `docs/task-specs/appointments-mvp/03-customer-appointment-list-and-cancellation.md` | `6.4`, `6.8` | Customer ve y cancela sus appointments. |
| Gestion admin de appointments | Planificado | `docs/task-specs/appointments-mvp/04-admin-appointment-calendar-and-list.md`, `docs/task-specs/appointments-mvp/05-admin-appointment-actions.md` | `6.5`, `6.6`, `6.7`, `6.9`, `6.11`, `6.12` | Admin lista, calendario, cancelacion, estados y notas internas. |
| Dashboard basico | Mas adelante | `docs/task-specs/appointments-mvp/06-dashboard-basics.md` | `10.1` | Depende de tener appointments reales. |

## Incrementos De Producto

| Incremento | Estado | Specs | Notas |
| --- | --- | --- | --- |
| First MVP | Hecho | `docs/task-specs/first-mvp/` | Baseline usable completada. |
| Appointments MVP | Siguiente | `docs/task-specs/appointments-mvp/` | Creacion y gestion real de appointments. |
| Operacion MVP | Mas adelante | Pendiente | Dashboard, notificaciones y flujos operativos. |
| Growth/Search MVP | Mas adelante | Pendiente | Descubrimiento publico, categorias y busqueda. |
| Hardening De Plataforma | Mas adelante | Pendiente | Rate limiting, readiness checks, auditoria y paginacion. |

## Reglas Para Specs De Entrega

- Una spec de entrega debe representar una slice visible de producto, no necesariamente una sola fila del mapa de capacidades.
- Cada spec debe declarar su incremento de producto.
- Cada spec debe declarar los IDs de capacidad que cubre.
- Una spec puede cubrir IDs de capacidad de varios grupos.
- Al completar una spec, se actualiza el estado del incremento y los checks del mapa de capacidades.
- No se borran specs completadas salvo que su contexto historico se migre a otro lugar.

## Mapa De Capacidades

Las siguientes secciones contienen los IDs de capacidad estables usados por incrementos y specs de entrega.

### 0. Frontend Shell Y Public Landing

| ID | Caso de uso | Backend | Frontend | Diseno | Postpuesto |
| --- | --- | --- | --- | --- | --- |
| 0.1 | Inicializar Vite React SPA con TypeScript. | [ ] | [x] | [ ] | [ ] |
| 0.2 | Configurar npm scripts para dev, build, lint, typecheck, test y preview. | [ ] | [x] | [ ] | [ ] |
| 0.3 | Configurar React Router con rutas publicas, auth y admin del MVP. | [ ] | [x] | [ ] | [ ] |
| 0.4 | Configurar TanStack Query como provider de server state. | [ ] | [x] | [ ] | [ ] |
| 0.5 | Configurar Tailwind CSS y base compatible con shadcn/ui. | [ ] | [x] | [ ] | [ ] |
| 0.6 | Configurar aliases `@/*`, tsconfig, ESLint y Vitest. | [ ] | [x] | [ ] | [ ] |
| 0.7 | Crear cliente HTTP base con API URL desde `VITE_API_BASE_URL`. | [ ] | [x] | [ ] | [ ] |
| 0.8 | Crear storage de auth MVP y guard de rutas admin. | [ ] | [x] | [ ] | [ ] |
| 0.9 | Anadir test smoke del shell frontend. | [ ] | [x] | [ ] | [ ] |
| 0.10 | Implementar pagina principal publica tipo marketplace. | [ ] | [x] | [x] | [ ] |
| 0.11 | Implementar business admin shell/layout base. | [ ] | [x] | [x] | [ ] |
| 0.12 | Implementar flujo publico de seleccion de appointment hasta elegir slot. | [ ] | [x] | [x] | [ ] |

### 1. Gestion Del Business Por Admin

| ID | Caso de uso | Backend | Frontend | Diseno | Postpuesto |
| --- | --- | --- | --- | --- | --- |
| 1.1 | Registrar business con su admin inicial. | [x] | [x] | [x] | [ ] |
| 1.2 | Actualizar datos publicos del business: nombre, descripcion, contacto, web, direccion, timezone, moneda. | [x] | [x] | [x] | [ ] |
| 1.3 | Activar/desactivar business desde admin. | [ ] | [ ] | [ ] | [x] |
| 1.4 | Configurar politica de cancelacion del business. | [ ] | [ ] | [ ] | [x] |
| 1.5 | Configurar antelacion minima para reservar. | [ ] | [ ] | [ ] | [x] |
| 1.6 | Configurar ventana maxima de reserva: por ejemplo hasta 30/60/90 dias. | [x] | [x] | [x] | [ ] |
| 1.7 | Configurar buffer antes/despues de appointments. | [ ] | [ ] | [ ] | [x] |

### 2. Services

| ID | Caso de uso | Backend | Frontend | Diseno | Postpuesto |
| --- | --- | --- | --- | --- | --- |
| 2.1 | Crear service para el business del admin autenticado. | [x] | [x] | [x] | [ ] |
| 2.2 | Listar services del business para admin, incluyendo inactivos. | [x] | [x] | [x] | [ ] |
| 2.3 | Obtener service por id para admin, incluyendo inactivos. | [x] | [x] | [x] | [ ] |
| 2.4 | Actualizar service: nombre, descripcion, duracion, precio, orden. | [x] | [x] | [x] | [ ] |
| 2.5 | Desactivar/eliminar service. | [x] | [x] | [x] | [ ] |
| 2.6 | Anadir imagenes de services. | [ ] | [ ] | [ ] | [x] |

### 3. Staff Members

| ID | Caso de uso | Backend | Frontend | Diseno | Postpuesto |
| --- | --- | --- | --- | --- | --- |
| 3.1 | Crear staff member para el business del admin autenticado. | [x] | [x] | [x] | [ ] |
| 3.2 | Listar staff members del business para admin, incluyendo inactivos. | [x] | [x] | [x] | [ ] |
| 3.3 | Obtener staff member por id para admin, incluyendo inactivos. | [x] | [x] | [x] | [ ] |
| 3.4 | Actualizar staff member: nombre, email, telefono, bio, orden. | [x] | [x] | [x] | [ ] |
| 3.5 | Desactivar/eliminar staff member. | [x] | [x] | [x] | [ ] |
| 3.6 | Anadir avatar/foto de staff member. | [ ] | [ ] | [ ] | [x] |

### 4. Asignaciones Staff-Service

| ID | Caso de uso | Backend | Frontend | Diseno | Postpuesto |
| --- | --- | --- | --- | --- | --- |
| 4.1 | Asignar service a staff member desde la ruta de staff. | [x] | [x] | [x] | [ ] |
| 4.2 | Asignar staff member a service desde la ruta de service. | [x] | [x] | [x] | [ ] |
| 4.3 | Desasignar service de staff member. | [x] | [x] | [x] | [ ] |
| 4.4 | Activar/desactivar una asignacion staff-service sin borrarla. | [x] | [x] | [x] | [ ] |
| 4.5 | Listar services asignados a un staff member. | [x] | [x] | [x] | [ ] |
| 4.6 | Listar staff members asignados a un service. | [x] | [x] | [x] | [ ] |
| 4.7 | Configurar duracion custom por staff-service si un staff tarda distinto en el mismo service. | [ ] | [ ] | [ ] | [x] |
| 4.8 | Configurar precio custom por staff-service si aplica. | [ ] | [ ] | [ ] | [x] |

### 5. Disponibilidad Y Horarios

| ID | Caso de uso | Backend | Frontend | Diseno | Postpuesto |
| --- | --- | --- | --- | --- | --- |
| 5.1 | Crear disponibilidad semanal de un staff member. | [x] | [x] | [x] | [ ] |
| 5.2 | Listar disponibilidad semanal de un staff member. | [x] | [x] | [x] | [ ] |
| 5.3 | Actualizar disponibilidad semanal de un staff member. | [x] | [x] | [x] | [ ] |
| 5.4 | Eliminar bloque de disponibilidad semanal. | [x] | [x] | [x] | [ ] |
| 5.5 | Crear excepcion de disponibilidad por fecha concreta: vacaciones, ausencia, horario especial. | [x] | [x] | [x] | [ ] |
| 5.6 | Listar excepciones de disponibilidad de un staff member. | [x] | [x] | [x] | [ ] |
| 5.7 | Actualizar excepcion de disponibilidad. | [x] | [x] | [x] | [ ] |
| 5.8 | Eliminar excepcion de disponibilidad. | [x] | [x] | [x] | [ ] |
| 5.9 | Calcular slots disponibles para business + service + fecha. | [x] | [x] | [x] | [ ] |
| 5.10 | Calcular slots disponibles para business + service + staff member + fecha. | [x] | [x] | [x] | [ ] |
| 5.11 | Validar que un appointment caiga dentro de disponibilidad y fuera de excepciones. | [x] | [x] | [x] | [ ] |
| 5.12 | Validar que un appointment no solape con otro appointment activo. | [x] | [x] | [ ] | [ ] |

### 6. Appointments / Reservas

| ID | Caso de uso | Backend | Frontend | Diseno | Postpuesto |
| --- | --- | --- | --- | --- | --- |
| 6.1 | Crear appointment como customer. | [x] | [x] | [ ] | [ ] |
| 6.2 | Crear appointment como admin. | [ ] | [ ] | [ ] | [ ] |
| 6.3 | Obtener appointment por id. | [x] | [x] | [ ] | [ ] |
| 6.4 | Listar appointments del customer autenticado. | [ ] | [ ] | [ ] | [ ] |
| 6.5 | Listar appointments del business para admin. | [ ] | [ ] | [ ] | [ ] |
| 6.6 | Listar appointments por staff member y rango de fechas. | [ ] | [ ] | [ ] | [ ] |
| 6.7 | Listar appointments por fecha/rango para calendario admin. | [ ] | [ ] | [ ] | [ ] |
| 6.8 | Cancelar appointment como customer. | [ ] | [ ] | [ ] | [ ] |
| 6.9 | Cancelar appointment como admin. | [ ] | [ ] | [ ] | [ ] |
| 6.10 | Reprogramar appointment. | [ ] | [ ] | [ ] | [ ] |
| 6.11 | Cambiar estado operativo de appointment: scheduled, completed, no-show. Las cancelaciones se gestionan con 6.8 y 6.9. | [ ] | [ ] | [ ] | [ ] |
| 6.12 | Guardar notas internas del appointment para admin/staff. | [ ] | [ ] | [ ] | [ ] |
| 6.13 | Guardar notas visibles del customer en el appointment. | [x] | [x] | [ ] | [ ] |
| 6.14 | Proteger creacion de appointment contra doble reserva concurrente: tomar lock transaccional, validar disponibilidad/solapes dentro de la transaccion e insertar appointment atomicamente. | [x] | [x] | [ ] | [ ] |

Nota para 6.14: para MVP se prefiere lock transaccional tipo SQL Server `sp_getapplock` por `businessId` + `staffMemberId` + `localDate`. `SlotHold` temporal queda fuera del MVP salvo que se necesite reservar provisionalmente mientras el customer completa registro/pago.

### 7. Customer Account

| ID | Caso de uso | Backend | Frontend | Diseno | Postpuesto |
| --- | --- | --- | --- | --- | --- |
| 7.1 | Registrar customer. | [x] | [x] | [x] | [ ] |
| 7.2 | Login de customer con JWT. | [x] | [x] | [x] | [ ] |
| 7.3 | Obtener perfil del customer autenticado. | [ ] | [ ] | [ ] | [ ] |
| 7.4 | Actualizar perfil de customer. | [ ] | [ ] | [ ] | [ ] |
| 7.5 | Cambiar password de customer. | [ ] | [ ] | [ ] | [ ] |
| 7.6 | Verificar email de customer. | [ ] | [ ] | [ ] | [ ] |
| 7.7 | Resetear password por email. | [ ] | [ ] | [ ] | [ ] |

### 8. Admin Account Y Auth

| ID | Caso de uso | Backend | Frontend | Diseno | Postpuesto |
| --- | --- | --- | --- | --- | --- |
| 8.1 | Login de admin con JWT. | [x] | [x] | [x] | [ ] |
| 8.2 | Cambiar password de admin. | [ ] | [ ] | [ ] | [ ] |
| 8.3 | Verificar email de admin. | [ ] | [ ] | [ ] | [ ] |
| 8.4 | Resetear password por email. | [ ] | [ ] | [ ] | [ ] |
| 8.5 | Refrescar token o renovar sesion. | [ ] | [ ] | [ ] | [ ] |
| 8.6 | Logout/revocacion de refresh tokens si se implementan refresh tokens. | [ ] | [ ] | [ ] | [ ] |
| 8.7 | Anadir roles admin adicionales si habra mas de un admin por business. | [ ] | [ ] | [ ] | [ ] |
| 8.8 | Anadir permisos granulares si habra recepcion, manager, owner, etc. | [ ] | [ ] | [ ] | [ ] |

### 9. Notificaciones

| ID | Caso de uso | Backend | Frontend | Diseno | Postpuesto |
| --- | --- | --- | --- | --- | --- |
| 9.1 | Anadir notificaciones email para appointment creado/cancelado/reprogramado. | [ ] | [ ] | [ ] | [ ] |
| 9.2 | Anadir recordatorios de appointment. | [ ] | [ ] | [ ] | [ ] |

### 10. Dashboard Y Reporting

| ID | Caso de uso | Backend | Frontend | Diseno | Postpuesto |
| --- | --- | --- | --- | --- | --- |
| 10.1 | Anadir endpoint de resumen dashboard admin: appointments de hoy, proximos, ingresos estimados. | [ ] | [ ] | [ ] | [ ] |
| 10.2 | Anadir endpoint de metricas basicas: appointments por estado, services mas reservados, staff mas reservado. | [ ] | [ ] | [ ] | [ ] |

### 11. Business Publico Y Descubrimiento

| ID | Caso de uso | Backend | Frontend | Diseno | Postpuesto |
| --- | --- | --- | --- | --- | --- |
| 11.1 | Obtener business publico por businessId. | [x] | [x] | [x] | [ ] |
| 11.2 | Obtener profile publico de business por businessId. | [x] | [x] | [x] | [ ] |
| 11.3 | Obtener profile publico de business por slug. | [x] | [x] | [x] | [ ] |
| 11.4 | Listar services activos de un business. | [x] | [x] | [x] | [ ] |
| 11.5 | Obtener un service activo concreto de un business. | [x] | [x] | [x] | [ ] |
| 11.6 | Listar staff members activos de un business. | [x] | [x] | [x] | [ ] |
| 11.7 | Obtener un staff member activo concreto de un business. | [x] | [x] | [x] | [ ] |
| 11.8 | Validar disponibilidad de slug y sugerir alternativas si ya esta ocupado. | [ ] | [ ] | [ ] | [ ] |
| 11.9 | Buscar businesses publicos por texto, ciudad o categoria. | [ ] | [ ] | [ ] | [ ] |
| 11.10 | Listar businesses publicos destacados o activos para landing/search. | [ ] | [ ] | [ ] | [ ] |
| 11.11 | Anadir categoria/tipo de business si se quiere busqueda por sector. | [ ] | [ ] | [ ] | [ ] |
| 11.12 | Anadir cache/ETag al endpoint publico de business profile by slug si crece el trafico. | [ ] | [ ] | [ ] | [x] |

### 12. Media / Assets

| ID | Caso de uso | Backend | Frontend | Diseno | Postpuesto |
| --- | --- | --- | --- | --- | --- |
| 12.1 | Anadir imagenes/logo del business. | [ ] | [ ] | [ ] | [x] |
| 12.2 | Anadir imagenes de services. | [ ] | [ ] | [ ] | [x] |
| 12.3 | Anadir avatar/foto de staff member. | [ ] | [ ] | [ ] | [x] |

### 13. Seguridad, Operacion Y Plataforma

| ID | Caso de uso | Backend | Frontend | Diseno | Postpuesto |
| --- | --- | --- | --- | --- | --- |
| 13.1 | Health check simple. | [x] | [ ] | [ ] | [ ] |
| 13.2 | Anadir soft delete consistente para entidades principales si no queremos borrado fisico. | [ ] | [ ] | [ ] | [ ] |
| 13.3 | Anadir auditoria basica de cambios importantes: business, services, staff, appointments. | [ ] | [ ] | [ ] | [ ] |
| 13.4 | Anadir paginacion/filtros en listados admin grandes. | [ ] | [ ] | [ ] | [ ] |
| 13.5 | Anadir rate limiting en endpoints publicos sensibles como login, register y disponibilidad. | [ ] | [ ] | [ ] | [ ] |
| 13.6 | Anadir endpoint de health/readiness para DB ademas del /health simple. | [ ] | [ ] | [ ] | [ ] |
| 13.7 | Anadir seed/dev data endpoint o script para facilitar pruebas locales. | [ ] | [ ] | [ ] | [x] |

### 14. Integraciones

| ID | Caso de uso | Backend | Frontend | Diseno | Postpuesto |
| --- | --- | --- | --- | --- | --- |
| 14.1 | Anadir integracion futura con calendario externo: Google Calendar/Outlook. | [ ] | [ ] | [ ] | [x] |

### 15. Multi-Sede / Escalado Del Modelo

| ID | Caso de uso | Backend | Frontend | Diseno | Postpuesto |
| --- | --- | --- | --- | --- | --- |
| 15.1 | Anadir soporte multi-sede si un business puede tener varias ubicaciones. | [ ] | [ ] | [ ] | [x] |
| 15.2 | Decidir si disponibilidad, services, staff y appointments cuelgan de business o de sede en un futuro multi-location. | [ ] | [ ] | [ ] | [x] |
