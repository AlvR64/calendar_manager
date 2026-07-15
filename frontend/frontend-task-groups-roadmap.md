# Frontend Use Case Roadmap

Legend:

- `Hecho`: caso de uso implementado actualmente en frontend.
- `Postpuesto`: caso de uso que queda deliberadamente para mas adelante o no aplica aun.
- Si ambos checks estan vacios, el caso de uso esta pendiente.

Este roadmap cubre solo el primer MVP frontend basado en el backend ya implementado y en los disenos OpenPencil ya creados. No incluye crear/cancelar/listar appointments porque el backend aun no expone esos casos de uso.

## 1. Frontend Shell Y Setup

| ID | Caso de uso | Hecho | Postpuesto |
| --- | --- | --- | --- |
| 1.1 | Inicializar Vite React SPA con TypeScript. | [x] | [ ] |
| 1.2 | Configurar npm scripts para dev, build, lint, typecheck, test y preview. | [x] | [ ] |
| 1.3 | Configurar React Router con rutas publicas, auth y admin del MVP. | [x] | [ ] |
| 1.4 | Configurar TanStack Query como provider de server state. | [x] | [ ] |
| 1.5 | Configurar Tailwind CSS y base compatible con shadcn/ui. | [x] | [ ] |
| 1.6 | Configurar aliases `@/*`, tsconfig, ESLint y Vitest. | [x] | [ ] |
| 1.7 | Crear cliente HTTP base con API URL desde `VITE_API_BASE_URL`. | [x] | [ ] |
| 1.8 | Crear contratos TypeScript iniciales alineados con el backend existente. | [x] | [ ] |
| 1.9 | Crear storage de auth MVP y guard de rutas admin. | [x] | [ ] |
| 1.10 | Anadir test smoke del shell frontend. | [x] | [ ] |

## 2. Public Landing Y Auth

| ID | Caso de uso | Hecho | Postpuesto |
| --- | --- | --- | --- |
| 2.1 | Implementar pagina principal publica segun `designs/homepage.op`. | [ ] | [ ] |
| 2.2 | Implementar login de customer segun `designs/customer-login.op`. | [ ] | [ ] |
| 2.3 | Integrar login de customer con backend y guardar sesion separada de admin. | [ ] | [ ] |
| 2.4 | Implementar login de business admin segun `designs/business-admin-login.op`. | [ ] | [ ] |
| 2.5 | Integrar login de business admin con backend y proteger rutas admin. | [ ] | [ ] |
| 2.6 | Implementar registro de customer segun `designs/customer-register.op`. | [ ] | [ ] |
| 2.7 | Integrar registro de customer con backend. | [ ] | [ ] |
| 2.8 | Implementar registro de business con admin inicial segun `designs/business-admin-register.op`. | [ ] | [ ] |
| 2.9 | Integrar registro de business con backend y entrar al panel admin tras exito. | [ ] | [ ] |

## 3. Public Business Profile

| ID | Caso de uso | Hecho | Postpuesto |
| --- | --- | --- | --- |
| 3.1 | Implementar ruta `/b/:slug` segun `designs/business-public-profile.op`. | [ ] | [ ] |
| 3.2 | Cargar profile publico de business por slug desde backend. | [ ] | [ ] |
| 3.3 | Mostrar datos publicos del business: nombre, descripcion, contacto, web, direccion y timezone. | [ ] | [ ] |
| 3.4 | Mostrar services activos del business. | [ ] | [ ] |
| 3.5 | Mostrar staff members activos del business. | [ ] | [ ] |
| 3.6 | Mostrar estados loading, empty y error para profile publico. | [ ] | [ ] |
| 3.7 | Enlazar CTA hacia `/b/:slug/appointment`. | [ ] | [ ] |

## 4. Public Appointment Slot Flow

| ID | Caso de uso | Hecho | Postpuesto |
| --- | --- | --- | --- |
| 4.1 | Implementar ruta `/b/:slug/appointment` segun `designs/public-appointment-slot-flow.op`. | [ ] | [ ] |
| 4.2 | Permitir seleccionar service activo. | [ ] | [ ] |
| 4.3 | Permitir seleccionar staff member opcional. | [ ] | [ ] |
| 4.4 | Permitir seleccionar fecha dentro de la ventana maxima de reserva del business. | [ ] | [ ] |
| 4.5 | Cargar slots disponibles desde backend usando business, service, staff member opcional y fecha. | [ ] | [ ] |
| 4.6 | Mostrar slots usando timezone local del business y datos backend-provided. | [ ] | [ ] |
| 4.7 | Mostrar estados loading, empty, error y fuera de ventana de reserva. | [ ] | [ ] |
| 4.8 | Permitir seleccionar slot y mantener seleccion local. | [ ] | [ ] |

## 5. Business Admin Shell Y Settings

| ID | Caso de uso | Hecho | Postpuesto |
| --- | --- | --- | --- |
| 5.1 | Implementar shell/layout admin segun `designs/business-admin-dashboard.op`. | [ ] | [ ] |
| 5.2 | Implementar navegacion admin entre dashboard, settings, services, staff members y availability. | [ ] | [ ] |
| 5.3 | Mostrar estado de sesion admin y accion de logout local. | [ ] | [ ] |
| 5.4 | Implementar pagina business settings segun `designs/business-settings.op`. | [ ] | [ ] |
| 5.5 | Cargar datos del business del admin autenticado desde backend. | [ ] | [ ] |
| 5.6 | Actualizar datos publicos, contacto, web, direccion, timezone y moneda. | [ ] | [ ] |
| 5.7 | Actualizar ventana maxima de reserva. | [ ] | [ ] |
| 5.8 | Mostrar validaciones de formulario, loading, success y error states. | [ ] | [ ] |

## 6. Business Admin Services

| ID | Caso de uso | Hecho | Postpuesto |
| --- | --- | --- | --- |
| 6.1 | Implementar pagina admin services segun `designs/admin-services.op`. | [ ] | [ ] |
| 6.2 | Listar services del business del admin, incluyendo inactivos. | [ ] | [ ] |
| 6.3 | Crear service con nombre, descripcion, duracion, precio y orden. | [ ] | [ ] |
| 6.4 | Editar service existente. | [ ] | [ ] |
| 6.5 | Activar/desactivar service. | [ ] | [ ] |
| 6.6 | Eliminar service. | [ ] | [ ] |
| 6.7 | Mostrar empty state, loading, success, validation errors y conflictos de eliminacion. | [ ] | [ ] |

## 7. Business Admin Staff Members

| ID | Caso de uso | Hecho | Postpuesto |
| --- | --- | --- | --- |
| 7.1 | Implementar pagina admin staff members segun `designs/admin-staff-members.op`. | [ ] | [ ] |
| 7.2 | Listar staff members del business del admin, incluyendo inactivos. | [ ] | [ ] |
| 7.3 | Crear staff member con nombre, email, telefono, bio y orden. | [ ] | [ ] |
| 7.4 | Editar staff member existente. | [ ] | [ ] |
| 7.5 | Activar/desactivar staff member. | [ ] | [ ] |
| 7.6 | Eliminar staff member. | [ ] | [ ] |
| 7.7 | Mostrar empty state, loading, success, validation errors y conflictos de eliminacion. | [ ] | [ ] |

## 8. Business Admin Staff-Service Assignments

| ID | Caso de uso | Hecho | Postpuesto |
| --- | --- | --- | --- |
| 8.1 | Mostrar services asignados en el detalle/modal de staff member. | [ ] | [ ] |
| 8.2 | Asignar service a staff member desde la vista de staff. | [ ] | [ ] |
| 8.3 | Mostrar staff members asignados en el detalle/modal de service. | [ ] | [ ] |
| 8.4 | Asignar staff member a service desde la vista de service. | [ ] | [ ] |
| 8.5 | Activar/desactivar asignacion staff-service. | [ ] | [ ] |
| 8.6 | Desasignar service de staff member. | [ ] | [ ] |
| 8.7 | Mostrar estados loading, success, error y conflictos de desasignacion. | [ ] | [ ] |

## 9. Business Admin Availability

| ID | Caso de uso | Hecho | Postpuesto |
| --- | --- | --- | --- |
| 9.1 | Implementar pagina admin availability segun `designs/admin-availability.op`. | [ ] | [ ] |
| 9.2 | Seleccionar staff member para gestionar su disponibilidad. | [ ] | [ ] |
| 9.3 | Listar disponibilidad semanal del staff member. | [ ] | [ ] |
| 9.4 | Crear bloque de disponibilidad semanal. | [ ] | [ ] |
| 9.5 | Editar bloque de disponibilidad semanal. | [ ] | [ ] |
| 9.6 | Eliminar bloque de disponibilidad semanal. | [ ] | [ ] |
| 9.7 | Listar excepciones de disponibilidad del staff member. | [ ] | [ ] |
| 9.8 | Crear excepcion cerrada por fecha concreta. | [ ] | [ ] |
| 9.9 | Crear excepcion con horario especial por fecha concreta. | [ ] | [ ] |
| 9.10 | Editar excepcion de disponibilidad. | [ ] | [ ] |
| 9.11 | Eliminar excepcion de disponibilidad. | [ ] | [ ] |
| 9.12 | Mostrar validaciones visuales de solapes, rangos invalidos y timezone del business. | [ ] | [ ] |

## Suggested MVP Frontend Order

1. Frontend Shell Y Setup `[x]` Base tecnica, rutas, providers, API client, auth storage y test smoke.
2. Public Landing Y Auth `[ ]` Entradas publicas y autenticacion separada de customer/admin.
3. Business Admin Shell Y Settings `[ ]` Primer area privada util tras login admin.
4. Business Admin Services `[ ]` Gestion de services necesarios para publicar oferta.
5. Business Admin Staff Members `[ ]` Gestion de staff members necesarios para disponibilidad.
6. Business Admin Staff-Service Assignments `[ ]` Relacion entre oferta y staff que puede prestar cada service.
7. Business Admin Availability `[ ]` Disponibilidad semanal y excepciones por staff member.
8. Public Business Profile `[ ]` Vista publica consumiendo el business ya configurado.
9. Public Appointment Slot Flow `[ ]` Seleccion publica de service, staff member opcional, fecha y slot disponible.
