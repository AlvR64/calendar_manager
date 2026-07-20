# Public Session Header And Auth Boundaries

## Incremento

Marketplace Discovery MVP.

## IDs De Capacidad

- `0.8`
- `0.10`
- `0.12`
- `6.1`
- `7.2`
- `8.1`
- `11.13`
- `11.14`
- `11.15`

## Matriz De Estado Actual

Mirror del estado actual de `../../../task-groups-roadmap.md` para contexto rapido. El roadmap sigue siendo autoritativo.

| ID | Backend | Frontend | Diseno | Postpuesto |
| --- | --- | --- | --- | --- |
| `0.8` | [ ] | [x] | [ ] | [ ] |
| `0.10` | [ ] | [x] | [x] | [ ] |
| `0.12` | [ ] | [x] | [x] | [ ] |
| `6.1` | [x] | [x] | [ ] | [ ] |
| `7.2` | [x] | [x] | [x] | [ ] |
| `8.1` | [x] | [x] | [x] | [ ] |
| `11.13` | [ ] | [x] | [x] | [ ] |
| `11.14` | [ ] | [x] | [x] | [ ] |
| `11.15` | [ ] | [x] | [x] | [ ] |

## Objetivo

Unificar el header publico para que siempre quede claro si hay sesion activa y de que tipo, y separar responsabilidades Admin/Customer sin impedir que Admin pueda navegar partes publicas en modo lectura.

## Alcance

- Crear un header publico compartido para pantallas publicas.
- Mostrar estado de sesion para usuario anonimo, Customer y Admin.
- Mostrar tipo de usuario junto al nombre/email.
- Adaptar el header mobile para no ocultar el acceso customer.
- Implementar single active session: login Admin limpia Customer y login Customer limpia Admin.
- Permitir que Admin navegue Home, search, perfil publico y slots.
- Bloquear confirmacion de appointments cuando la sesion activa es Admin.
- Mostrar mensaje/CTAs claros para Admin en el flujo publico de appointment.
- Mantener Customer con capacidad de confirmar appointments.
- Actualizar tests frontend afectados.

## Fuera De Alcance

- Cambios backend de autorizacion.
- Multi-account switcher.
- Selector de rol activo.
- Crear appointments como Admin.
- Redisenar por completo el admin layout.
- Cambios de permisos persistidos en backend.

## Contrato Backend

No cambia. La separacion de responsabilidades de esta slice es frontend/session UX.

| Endpoint / Use Case | Auth | Request | Success Response | Error Cases |
| --- | --- | --- | --- | --- |
| Customer login | Ninguna | Credenciales customer | Customer session | `400`/auth errors |
| Admin login | Ninguna | Credenciales admin | Admin session | `400`/auth errors |
| Crear appointment | Customer | Appointment request existente | Appointment creado | Auth/validation/conflict |

## Referencias De Diseno

- `designs/homepage.op`
- `designs/marketplace-search.op`
- `designs/business-public-profile.op`
- `designs/public-appointment-slot-flow.op`

## Rutas Y Pantallas Frontend

- `/`
- `/search`
- `/b/:slug`
- `/b/:slug/appointment`
- `/appointments/:appointmentId`
- `/auth/customer/login`
- `/auth/admin/login`

## Estados UX

- Header sin sesion.
- Header con Customer.
- Header con Admin.
- Header mobile compacto.
- Flujo de appointment sin sesion.
- Flujo de appointment con Customer.
- Flujo de appointment con Admin en modo lectura sin confirmacion.

## Reglas De Datos Y Validacion

- Solo puede existir una sesion activa en frontend.
- `setAuthSession(Admin)` debe eliminar sesion Customer.
- `setAuthSession(Customer)` debe eliminar sesion Admin.
- Admin puede navegar pantallas publicas pero no confirmar appointments.
- Customer puede confirmar appointments.
- Usuario anonimo debe seguir viendo login/register customer para confirmar.
- Usar `Appointment`, no `Booking`, en copy nuevo.

## Plan De Implementacion

1. Crear componente `PublicHeader` compartido.
2. Crear badge/summary de sesion generico para Admin y Customer.
3. Aplicar header a pantallas publicas relevantes.
4. Ajustar auth storage o login success para single active session.
5. Ajustar appointment slot flow para bloquear confirmacion con sesion Admin.
6. Actualizar tests de auth, header y slot flow.
7. Ejecutar checks frontend.
8. Actualizar README del incremento y roadmap al completar.

## Plan De Tests

- Login Customer limpia sesion Admin.
- Login Admin limpia sesion Customer.
- Header sin sesion muestra login/CTA correctos.
- Header con Customer muestra tipo Customer y acceso a appointments.
- Header con Admin muestra tipo Admin y acceso a dashboard.
- Mobile mantiene acceso customer/admin visible mediante UI compacta.
- Admin puede ver slots pero no confirmar appointment.
- Customer puede confirmar appointment como antes.
- Comandos: `npm run typecheck`, `npm run lint`, `npm test -- --run`, `npm run build`.

## Criterios De Aceptacion

- [x] Header publico unificado en pantallas publicas.
- [x] La UI deja claro el tipo de sesion activa.
- [x] No pueden coexistir sesiones Admin y Customer.
- [x] Admin puede navegar partes publicas pero no confirmar appointments.
- [x] Customer conserva el flujo de appointment.
- [x] Mobile no oculta el acceso customer.
- [x] Los tests/checks relevantes pasan.
- [x] `../../../task-groups-roadmap.md` esta actualizado tras completar.

## Preguntas Abiertas

- Ninguna.
