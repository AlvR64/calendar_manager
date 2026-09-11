# Frontend2 Admin Dashboard

## Status

Complete

## Objective

Help a business owner scan the immediate state of the agenda and move into Appointment operations without presenting unsupported advanced analytics.

## Capability IDs

- `10.1`
- `F2-ADM-02`
- `F2-ADM-03`

## Canonical Product References

- `../../task-specs/appointments-mvp/06-dashboard-basics.md`

## Routes And Screens

- `/admin`

## Existing Behavior To Preserve

- Admin authentication is required.
- Load `GET /api/admin/dashboard-summary` with the Admin access token.
- Present today count, estimated revenue, upcoming Appointments, status counts, the response currency, and the response date window.
- Link to `/admin/appointments` for the full agenda.

## Redesign Direction

- Mode: Operate.
- Direction: Agenda of the current shift. A single large upcoming-Appointment surface leads the screen; compact, subdivided signals show today count, estimated revenue, and the active date window.
- The interface is dark-first and soft industrial: charcoal surfaces, visible fine borders, restrained lime attention, practical monospace for operational values, and no decorative gradients or floating-card stacks.
- The selected structure keeps the owner focused on what is next rather than treating the dashboard as an analytics report.

## Components And Layouts

- `AdminShell` with responsive Admin navigation.
- `AdminDashboardPage` with metric strip, agenda panel, status panel, loading state, error state, and empty agenda state.

## Backend Contract References

- `GET /api/admin/dashboard-summary` from the canonical dashboard spec.

## UX States

- Loading skeleton.
- Empty upcoming Appointments.
- Empty status data.
- API and forbidden error with retry.
- Populated operational summary.

## Responsive And Accessibility

- Mobile behavior: horizontal compact Admin navigation; signal strip stacks; agenda status moves beneath each Appointment row.
- Keyboard and focus behavior: visible focus for navigation, links, and retry action.
- Semantic and assistive-technology requirements: landmark navigation, headings, time elements, accessible loading label, and error alert.

## Test Plan

- Unit/component tests: populated, empty, error, and protected route behavior.
- Manual checks: desktop and mobile rendering against the selected direction.
- Commands: `npm run typecheck`, `npm run lint`, `npm test -- --run`, `npm run build`, and Impeccable detector after UI changes.

## Acceptance Criteria

- [x] Existing dashboard behavior is preserved against the backend contract.
- [x] `/admin` renders a responsive operational Dashboard instead of a placeholder.
- [x] Applicable loading, empty, error, and populated states are handled.
- [x] WCAG 2.2 AA requirements are addressed.
- [x] Relevant frontend2 checks pass.

## Open Questions

- The exact date display format will be consolidated with the shared localization approach when that foundation is implemented.
