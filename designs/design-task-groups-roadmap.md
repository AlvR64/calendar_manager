# Design Use Case Roadmap

Legend:

- `Hecho`: diseno creado actualmente en OpenPencil.
- `Postpuesto`: diseno que queda deliberadamente para mas adelante o no aplica aun.
- Si ambos checks estan vacios, el diseno esta pendiente.

## 1. Public Landing Y Auth

| ID | Diseno | Hecho | Postpuesto |
| --- | --- | --- | --- |
| 1.1 | Disenar pagina principal publica tipo marketplace. | [x] | [ ] |
| 1.2 | Disenar login de customer. | [x] | [ ] |
| 1.3 | Disenar login de business admin. | [x] | [ ] |
| 1.4 | Disenar registro de customer. | [ ] | [ ] |
| 1.5 | Disenar registro de business con admin inicial. | [x] | [ ] |

## 2. Public Business Y Booking

| ID | Diseno | Hecho | Postpuesto |
| --- | --- | --- | --- |
| 2.1 | Disenar perfil publico de business. | [ ] | [ ] |
| 2.2 | Disenar lista de services activos de un business. | [ ] | [ ] |
| 2.3 | Disenar detalle publico de service activo. | [ ] | [ ] |
| 2.4 | Disenar lista de staff members activos de un business. | [ ] | [ ] |
| 2.5 | Disenar detalle publico de staff member activo. | [ ] | [ ] |
| 2.6 | Disenar flujo publico de seleccion de appointment: service, staff member opcional, fecha y slot. | [ ] | [ ] |
| 2.7 | Disenar estados de slots disponibles: loading, vacio, error y fuera de ventana de reserva. | [ ] | [ ] |

## 3. Business Admin Shell Y Settings

| ID | Diseno | Hecho | Postpuesto |
| --- | --- | --- | --- |
| 3.1 | Disenar shell/layout base del business admin: sidebar, topbar y navegacion. | [ ] | [ ] |
| 3.2 | Disenar settings de business: datos publicos, contacto, direccion, timezone y moneda. | [ ] | [ ] |
| 3.3 | Disenar configuracion de ventana maxima de reserva. | [ ] | [ ] |

## 4. Business Admin Services

| ID | Diseno | Hecho | Postpuesto |
| --- | --- | --- | --- |
| 4.1 | Disenar listado admin de services, incluyendo activos e inactivos. | [ ] | [ ] |
| 4.2 | Disenar crear service. | [ ] | [ ] |
| 4.3 | Disenar editar service. | [ ] | [ ] |
| 4.4 | Disenar activar/desactivar service. | [ ] | [ ] |
| 4.5 | Disenar eliminar service y conflicto si tiene appointments. | [ ] | [ ] |
| 4.6 | Disenar empty state de services. | [ ] | [ ] |

## 5. Business Admin Staff Members

| ID | Diseno | Hecho | Postpuesto |
| --- | --- | --- | --- |
| 5.1 | Disenar listado admin de staff members, incluyendo activos e inactivos. | [ ] | [ ] |
| 5.2 | Disenar crear staff member. | [ ] | [ ] |
| 5.3 | Disenar editar staff member. | [ ] | [ ] |
| 5.4 | Disenar activar/desactivar staff member. | [ ] | [ ] |
| 5.5 | Disenar eliminar staff member y conflicto si tiene appointments. | [ ] | [ ] |
| 5.6 | Disenar empty state de staff members. | [ ] | [ ] |

## 6. Business Admin Staff-Service Assignments

| ID | Diseno | Hecho | Postpuesto |
| --- | --- | --- | --- |
| 6.1 | Disenar asignar service a staff member desde detalle de staff. | [ ] | [ ] |
| 6.2 | Disenar asignar staff member a service desde detalle de service. | [ ] | [ ] |
| 6.3 | Disenar activar/desactivar asignacion staff-service. | [ ] | [ ] |
| 6.4 | Disenar desasignar service de staff member y conflicto si tiene appointments. | [ ] | [ ] |

## 7. Business Admin Availability

| ID | Diseno | Hecho | Postpuesto |
| --- | --- | --- | --- |
| 7.1 | Disenar gestion de disponibilidad semanal por staff member. | [ ] | [ ] |
| 7.2 | Disenar crear bloque de disponibilidad semanal. | [ ] | [ ] |
| 7.3 | Disenar editar bloque de disponibilidad semanal. | [ ] | [ ] |
| 7.4 | Disenar eliminar bloque de disponibilidad semanal. | [ ] | [ ] |
| 7.5 | Disenar listado de excepciones de disponibilidad. | [ ] | [ ] |
| 7.6 | Disenar crear excepcion cerrada por fecha. | [ ] | [ ] |
| 7.7 | Disenar crear excepcion con horario especial por fecha. | [ ] | [ ] |
| 7.8 | Disenar editar excepcion de disponibilidad. | [ ] | [ ] |
| 7.9 | Disenar eliminar excepcion de disponibilidad. | [ ] | [ ] |
| 7.10 | Disenar validaciones visuales de solapes y rangos invalidos. | [ ] | [ ] |

## 8. Customer Account Futuro

| ID | Diseno | Hecho | Postpuesto |
| --- | --- | --- | --- |
| 8.1 | Disenar perfil de customer autenticado. | [ ] | [x] |
| 8.2 | Disenar editar perfil de customer. | [ ] | [x] |
| 8.3 | Disenar cambiar password de customer. | [ ] | [x] |
| 8.4 | Disenar reset de password de customer. | [ ] | [x] |

## 9. Appointments Futuro

| ID | Diseno | Hecho | Postpuesto |
| --- | --- | --- | --- |
| 9.1 | Disenar confirmacion final de crear appointment. | [ ] | [x] |
| 9.2 | Disenar listado de appointments del customer. | [ ] | [x] |
| 9.3 | Disenar detalle de appointment. | [ ] | [x] |
| 9.4 | Disenar cancelar appointment como customer. | [ ] | [x] |
| 9.5 | Disenar calendario/listado admin de appointments. | [ ] | [x] |
| 9.6 | Disenar cancelar appointment como admin. | [ ] | [x] |
| 9.7 | Disenar reprogramar appointment. | [ ] | [x] |

## 10. Dashboard Y Reporting Futuro

| ID | Diseno | Hecho | Postpuesto |
| --- | --- | --- | --- |
| 10.1 | Disenar dashboard admin con resumen de appointments de hoy, proximos e ingresos estimados. | [ ] | [x] |
| 10.2 | Disenar metricas basicas: appointments por estado, services mas reservados y staff mas reservado. | [ ] | [x] |

## 11. MVP De Disenos Basado En Backend Implementado

Este apartado lista solo los primeros disenos que haria para cubrir un MVP visual basado en casos de uso ya implementados en backend. Cuenta los disenos ya creados.

| ID | Diseno | Hecho | Postpuesto |
| --- | --- | --- | --- |
| 11.1 | Pagina principal publica tipo marketplace. | [x] | [ ] |
| 11.2 | Login de customer. | [x] | [ ] |
| 11.3 | Login de business admin. | [x] | [ ] |
| 11.4 | Registro de customer. | [ ] | [ ] |
| 11.5 | Registro de business con admin inicial. | [x] | [ ] |
| 11.6 | Perfil publico de business con datos publicos, services y staff members activos. | [ ] | [ ] |
| 11.7 | Flujo publico de seleccion de appointment hasta elegir slot: service, staff member opcional, fecha y horario. | [ ] | [ ] |
| 11.8 | Business admin shell/layout base. | [ ] | [ ] |
| 11.9 | Business settings: datos publicos, contacto, direccion, timezone, moneda y ventana maxima de reserva. | [ ] | [ ] |
| 11.10 | Admin services: listado, crear, editar, activar/desactivar, eliminar y empty/conflict states. | [ ] | [ ] |
| 11.11 | Admin staff members: listado, crear, editar, activar/desactivar, eliminar y empty/conflict states. | [ ] | [ ] |
| 11.12 | Admin staff-service assignments desde detalle de staff o service. | [ ] | [ ] |
| 11.13 | Admin availability: disponibilidad semanal y excepciones por staff member. | [ ] | [ ] |

## Suggested MVP Design Order

1. Pagina principal publica tipo marketplace. `[x]` Landing publica para descubrir businesses y explicar el producto.
2. Login de customer. `[x]` Acceso de customers a su cuenta cuando exista area privada.
3. Login de business admin. `[x]` Acceso de admins al panel privado del business.
4. Registro de business con admin inicial. `[x]` Alta del business y creacion del primer admin con email y password.
5. Business admin shell/layout base. Pantalla inicial tras login con navegacion, topbar y contenedor del panel admin.
6. Business settings. Edicion de datos publicos, contacto, direccion, timezone, moneda y ventana maxima de reserva.
7. Admin services. Gestion de services: listado, crear, editar, activar/desactivar, eliminar y estados vacios/conflictos.
8. Admin staff members. Gestion de staff members: listado, crear, editar, activar/desactivar, eliminar y estados vacios/conflictos.
9. Admin staff-service assignments. Asignacion de services a staff members y gestion de asignaciones activas/inactivas.
10. Admin availability. Configuracion de disponibilidad semanal y excepciones por staff member.
11. Perfil publico de business. Pagina publica del negocio con datos, services activos y staff members activos.
12. Flujo publico de seleccion de appointment hasta elegir slot. Seleccion de service, staff member opcional, fecha y slot disponible.
13. Registro de customer. Alta de customer con email, password y datos basicos antes o durante el flujo de appointment.

## Natural Suggested Design Order

1. Public Landing Y Auth
2. Public Business Y Booking
3. Business Admin Shell Y Settings
4. Business Admin Services
5. Business Admin Staff Members
6. Business Admin Staff-Service Assignments
7. Business Admin Availability
8. Customer Account Futuro
9. Appointments Futuro
10. Dashboard Y Reporting Futuro
11. MVP De Disenos Basado En Backend Implementado
