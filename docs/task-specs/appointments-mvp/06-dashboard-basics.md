# Dashboard Basics

## Incremento

Appointments MVP.

## IDs De Capacidad

- `10.1`

## Matriz De Estado Actual

| ID | Backend | Frontend | Diseno | Postpuesto |
| --- | --- | --- | --- | --- |
| `10.1` | [ ] | [ ] | [ ] | [ ] |

## Objetivo

Actualizar el dashboard admin para mostrar un resumen basico basado en appointments reales.

## Alcance

- Appointments de hoy.
- Proximos appointments.
- Ingresos estimados de un rango.
- Conteo basico por status.
- Actualizar `/admin` para dejar de ser una pantalla principalmente estatica.

## Fuera De Alcance

- Metricas avanzadas.
- Graficos complejos.
- Comparativas historicas.
- Exportacion.
- Staff/service ranking; queda para `10.2`.

## Contrato Backend

| Endpoint / Use Case | Auth | Request | Success Response | Error Cases |
| --- | --- | --- | --- | --- |
| `GET /api/admin/dashboard-summary` | Admin | Query `from?`, `to?` | `200 AdminDashboardSummaryResponse` | `400`, `401`, `403` |

### `AdminDashboardSummaryResponse`

- `todayAppointmentCount`
- `upcomingAppointments: AppointmentSummaryResponse[]`
- `estimatedRevenueAmount`
- `currencyCode`
- `statusCounts: { status, count }[]`
- `rangeStartLocalDate`
- `rangeEndLocalDate`

### Reglas Backend

- Business id sale del claim `business_id`.
- Usar timezone del business para calcular hoy/rangos locales.
- Ingresos estimados usan `PriceAmountSnapshot` de appointments no cancelados.
- Default recomendado: hoy + proximos 7 dias.

## Referencias De Diseno

- `designs/business-admin-dashboard.op` como base visual existente.
- Falta diseno actualizado con datos reales de appointments.

## Rutas Y Pantallas Frontend

- `/admin`

## Estados UX

- Loading.
- Empty no appointments.
- Error.
- Success summary.

## Reglas De Datos Y Validacion

- Mostrar moneda desde response.
- Mostrar fechas en timezone del business.
- Limitar upcoming appointments mostrados en dashboard; link a `/admin/appointments` para ver mas.

## Plan De Implementacion

1. Crear query backend `GetAdminDashboardSummary`.
2. Anadir endpoint `GET /api/admin/dashboard-summary`.
3. Anadir tests backend de business isolation, rango y calculo de ingresos.
4. Anadir API/types frontend.
5. Actualizar `AdminDashboardPage` con datos reales.
6. Linkar a `/admin/appointments`.
7. Cubrir estados UX.
8. Actualizar roadmap al completar.

## Plan De Tests

- Backend: counts, revenue excluding cancelled, business isolation.
- Frontend: loading, empty, success, error.
- Comandos: `dotnet build`, `dotnet test`, `npm run typecheck`, `npm run lint`, `npm test -- --run`, `npm run build`.

## Criterios De Aceptacion

- [ ] Dashboard muestra appointments de hoy.
- [ ] Dashboard muestra proximos appointments.
- [ ] Dashboard calcula ingresos estimados con snapshots.
- [ ] Dashboard excluye cancelados de ingresos.
- [ ] UI enlaza a admin appointments.
- [ ] Roadmap actualizado.

## Preguntas Abiertas

- Definir rango default final: proximos 7 dias o proximos 14 dias.
