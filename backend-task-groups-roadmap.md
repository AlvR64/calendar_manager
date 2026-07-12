# Backend Use Case Roadmap

Legend:

- `Hecho`: caso de uso implementado actualmente en backend.
- `Postpuesto`: caso de uso que queda deliberadamente para mas adelante o no aplica aun.
- Si ambos checks estan vacios, el caso de uso esta pendiente.

## 1. Gestion Del Business Por Admin

| ID | Caso de uso | Hecho | Postpuesto |
| --- | --- | --- | --- |
| 1.1 | Registrar business con su admin inicial. | [x] | [ ] |
| 1.2 | Actualizar datos publicos del business: nombre, descripcion, contacto, web, direccion, timezone, moneda. | [x] | [ ] |
| 1.3 | Activar/desactivar business desde admin. | [ ] | [x] |
| 1.4 | Configurar politica de cancelacion del business. | [ ] | [x] |
| 1.5 | Configurar antelacion minima para reservar. | [ ] | [x] |
| 1.6 | Configurar ventana maxima de reserva: por ejemplo hasta 30/60/90 dias. | [x] | [ ] |
| 1.7 | Configurar buffer antes/despues de appointments. | [ ] | [x] |

## 2. Services

| ID | Caso de uso | Hecho | Postpuesto |
| --- | --- | --- | --- |
| 2.1 | Crear service para el business del admin autenticado. | [x] | [ ] |
| 2.2 | Listar services del business para admin, incluyendo inactivos. | [x] | [ ] |
| 2.3 | Obtener service por id para admin, incluyendo inactivos. | [x] | [ ] |
| 2.4 | Actualizar service: nombre, descripcion, duracion, precio, orden. | [x] | [ ] |
| 2.5 | Desactivar/eliminar service. | [x] | [ ] |
| 2.6 | Anadir imagenes de services. | [ ] | [x] |

## 3. Staff Members

| ID | Caso de uso | Hecho | Postpuesto |
| --- | --- | --- | --- |
| 3.1 | Crear staff member para el business del admin autenticado. | [x] | [ ] |
| 3.2 | Listar staff members del business para admin, incluyendo inactivos. | [x] | [ ] |
| 3.3 | Obtener staff member por id para admin, incluyendo inactivos. | [x] | [ ] |
| 3.4 | Actualizar staff member: nombre, email, telefono, bio, orden. | [x] | [ ] |
| 3.5 | Desactivar/eliminar staff member. | [x] | [ ] |
| 3.6 | Anadir avatar/foto de staff member. | [ ] | [x] |

## 4. Asignaciones Staff-Service

| ID | Caso de uso | Hecho | Postpuesto |
| --- | --- | --- | --- |
| 4.1 | Asignar service a staff member desde la ruta de staff. | [x] | [ ] |
| 4.2 | Asignar staff member a service desde la ruta de service. | [x] | [ ] |
| 4.3 | Desasignar service de staff member. | [x] | [ ] |
| 4.4 | Activar/desactivar una asignacion staff-service sin borrarla. | [x] | [ ] |
| 4.5 | Listar services asignados a un staff member. | [ ] | [x] |
| 4.6 | Listar staff members asignados a un service. | [ ] | [x] |
| 4.7 | Configurar duracion custom por staff-service si un staff tarda distinto en el mismo service. | [ ] | [x] |
| 4.8 | Configurar precio custom por staff-service si aplica. | [ ] | [x] |

## 5. Disponibilidad Y Horarios

| ID | Caso de uso | Hecho | Postpuesto |
| --- | --- | --- | --- |
| 5.1 | Crear disponibilidad semanal de un staff member. | [x] | [ ] |
| 5.2 | Listar disponibilidad semanal de un staff member. | [x] | [ ] |
| 5.3 | Actualizar disponibilidad semanal de un staff member. | [x] | [ ] |
| 5.4 | Eliminar bloque de disponibilidad semanal. | [x] | [ ] |
| 5.5 | Crear excepcion de disponibilidad por fecha concreta: vacaciones, ausencia, horario especial. | [x] | [ ] |
| 5.6 | Listar excepciones de disponibilidad de un staff member. | [x] | [ ] |
| 5.7 | Actualizar excepcion de disponibilidad. | [ ] | [ ] |
| 5.8 | Eliminar excepcion de disponibilidad. | [ ] | [ ] |
| 5.9 | Calcular slots disponibles para business + service + fecha. | [ ] | [ ] |
| 5.10 | Calcular slots disponibles para business + service + staff member + fecha. | [ ] | [ ] |
| 5.11 | Validar que un appointment caiga dentro de disponibilidad y fuera de excepciones. | [ ] | [ ] |
| 5.12 | Validar que un appointment no solape con otro appointment activo. | [ ] | [ ] |

## 6. Appointments / Reservas

| ID | Caso de uso | Hecho | Postpuesto |
| --- | --- | --- | --- |
| 6.1 | Crear appointment como customer. | [ ] | [ ] |
| 6.2 | Crear appointment como admin. | [ ] | [ ] |
| 6.3 | Obtener appointment por id. | [ ] | [ ] |
| 6.4 | Listar appointments del customer autenticado. | [ ] | [ ] |
| 6.5 | Listar appointments del business para admin. | [ ] | [ ] |
| 6.6 | Listar appointments por staff member y rango de fechas. | [ ] | [ ] |
| 6.7 | Listar appointments por fecha/rango para calendario admin. | [ ] | [ ] |
| 6.8 | Cancelar appointment como customer. | [ ] | [ ] |
| 6.9 | Cancelar appointment como admin. | [ ] | [ ] |
| 6.10 | Reprogramar appointment. | [ ] | [ ] |
| 6.11 | Cambiar estado de appointment: scheduled, confirmed, completed, cancelled, no-show. | [ ] | [ ] |
| 6.12 | Guardar notas internas del appointment para admin/staff. | [ ] | [ ] |
| 6.13 | Guardar notas visibles del customer en el appointment. | [ ] | [ ] |

## 7. Customer Account

| ID | Caso de uso | Hecho | Postpuesto |
| --- | --- | --- | --- |
| 7.1 | Registrar customer. | [x] | [ ] |
| 7.2 | Login de customer con JWT. | [x] | [ ] |
| 7.3 | Obtener perfil del customer autenticado. | [ ] | [ ] |
| 7.4 | Actualizar perfil de customer. | [ ] | [ ] |
| 7.5 | Cambiar password de customer. | [ ] | [ ] |
| 7.6 | Verificar email de customer. | [ ] | [ ] |
| 7.7 | Resetear password por email. | [ ] | [ ] |

## 8. Admin Account Y Auth

| ID | Caso de uso | Hecho | Postpuesto |
| --- | --- | --- | --- |
| 8.1 | Login de admin con JWT. | [x] | [ ] |
| 8.2 | Cambiar password de admin. | [ ] | [ ] |
| 8.3 | Verificar email de admin. | [ ] | [ ] |
| 8.4 | Resetear password por email. | [ ] | [ ] |
| 8.5 | Refrescar token o renovar sesion. | [ ] | [ ] |
| 8.6 | Logout/revocacion de refresh tokens si se implementan refresh tokens. | [ ] | [ ] |
| 8.7 | Anadir roles admin adicionales si habra mas de un admin por business. | [ ] | [ ] |
| 8.8 | Anadir permisos granulares si habra recepcion, manager, owner, etc. | [ ] | [ ] |

## 9. Notificaciones

| ID | Caso de uso | Hecho | Postpuesto |
| --- | --- | --- | --- |
| 9.1 | Anadir notificaciones email para appointment creado/cancelado/reprogramado. | [ ] | [ ] |
| 9.2 | Anadir recordatorios de appointment. | [ ] | [ ] |

## 10. Dashboard Y Reporting

| ID | Caso de uso | Hecho | Postpuesto |
| --- | --- | --- | --- |
| 10.1 | Anadir endpoint de resumen dashboard admin: appointments de hoy, proximos, ingresos estimados. | [ ] | [ ] |
| 10.2 | Anadir endpoint de metricas basicas: appointments por estado, services mas reservados, staff mas reservado. | [ ] | [ ] |

## 11. Business Publico Y Descubrimiento

| ID | Caso de uso | Hecho | Postpuesto |
| --- | --- | --- | --- |
| 11.1 | Obtener business publico por businessId. | [x] | [ ] |
| 11.2 | Obtener profile publico de business por businessId. | [x] | [ ] |
| 11.3 | Obtener profile publico de business por slug. | [x] | [ ] |
| 11.4 | Listar services activos de un business. | [x] | [ ] |
| 11.5 | Obtener un service activo concreto de un business. | [x] | [ ] |
| 11.6 | Listar staff members activos de un business. | [x] | [ ] |
| 11.7 | Obtener un staff member activo concreto de un business. | [x] | [ ] |
| 11.8 | Validar disponibilidad de slug y sugerir alternativas si ya esta ocupado. | [ ] | [ ] |
| 11.9 | Buscar businesses publicos por texto, ciudad o categoria. | [ ] | [ ] |
| 11.10 | Listar businesses publicos destacados o activos para landing/search. | [ ] | [ ] |
| 11.11 | Anadir categoria/tipo de business si se quiere busqueda por sector. | [ ] | [ ] |
| 11.12 | Anadir cache/ETag al endpoint publico de business profile by slug si crece el trafico. | [ ] | [x] |

## 12. Media / Assets

| ID | Caso de uso | Hecho | Postpuesto |
| --- | --- | --- | --- |
| 12.1 | Anadir imagenes/logo del business. | [ ] | [x] |
| 12.2 | Anadir imagenes de services. | [ ] | [x] |
| 12.3 | Anadir avatar/foto de staff member. | [ ] | [x] |

## 13. Seguridad, Operacion Y Plataforma

| ID | Caso de uso | Hecho | Postpuesto |
| --- | --- | --- | --- |
| 13.1 | Health check simple. | [x] | [ ] |
| 13.2 | Anadir soft delete consistente para entidades principales si no queremos borrado fisico. | [ ] | [ ] |
| 13.3 | Anadir auditoria basica de cambios importantes: business, services, staff, appointments. | [ ] | [ ] |
| 13.4 | Anadir paginacion/filtros en listados admin grandes. | [ ] | [ ] |
| 13.5 | Anadir rate limiting en endpoints publicos sensibles como login, register y disponibilidad. | [ ] | [ ] |
| 13.6 | Anadir endpoint de health/readiness para DB ademas del /health simple. | [ ] | [ ] |
| 13.7 | Anadir seed/dev data endpoint o script para facilitar pruebas locales. | [ ] | [x] |

## 14. Integraciones

| ID | Caso de uso | Hecho | Postpuesto |
| --- | --- | --- | --- |
| 14.1 | Anadir integracion futura con calendario externo: Google Calendar/Outlook. | [ ] | [x] |

## 15. Multi-Sede / Escalado Del Modelo

| ID | Caso de uso | Hecho | Postpuesto |
| --- | --- | --- | --- |
| 15.1 | Anadir soporte multi-sede si un business puede tener varias ubicaciones. | [ ] | [x] |
| 15.2 | Decidir si disponibilidad, services, staff y appointments cuelgan de business o de sede en un futuro multi-location. | [ ] | [x] |

## Natural Suggested Implementation Order

1. Gestion Del Business Por Admin
2. Services
3. Staff Members
4. Asignaciones Staff-Service
5. Disponibilidad Y Horarios
6. Appointments / Reservas
7. Customer Account
8. Admin Account Y Auth
9. Notificaciones
10. Dashboard Y Reporting
11. Business Publico Y Descubrimiento
12. Media / Assets
13. Seguridad, Operacion Y Plataforma
14. Integraciones
15. Multi-Sede / Escalado Del Modelo
