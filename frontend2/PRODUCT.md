# Product

<!-- impeccable:product-schema 1 -->

## Platform

web

## Users

- Primary: business owners acting as Admins. They configure the business and need a reliable operational view of services, StaffMembers, availability, and Appointments.
- Secondary: Customers who discover a business and reserve an Appointment for an available service and StaffMember.
- StaffMembers represent people or resources with agendas. They do not log in during the MVP.

## Product Purpose

Calendar Manager helps service businesses publish their offering and operate their appointment schedule. It connects public discovery and reservation with the Admin controls needed to maintain services, StaffMembers, availability, and Appointments.

## Positioning

Unlike a general-purpose calendar, Calendar Manager combines public appointment reservation with operational controls tied to a business's services, StaffMembers, and availability.

## Operating Context

- Admins use the product to configure their business and scan the day-to-day state of their agenda.
- Customers use public routes to find a business, select a service and StaffMember, choose a backend-provided available slot, and create an Appointment.
- The Admin dashboard summarizes today's Appointments, upcoming Appointments, estimated revenue, and appointment status counts.

## Capabilities and Constraints

- Admin and Customer are separate account types and authentication flows.
- An Appointment is scheduled against a StaffMember; use Appointment rather than Booking in product language.
- Availability belongs to a StaffMember, not a business.
- Business timezones use IANA timezone identifiers. Backend appointment instants are UTC-first; user-facing schedules respect the business timezone.
- Prefer backend-provided available slot data rather than client-side availability calculations.
- `frontend2/` is an isolated redesign candidate. `frontend/` is a read-only functional reference and is not a visual authority.
- Product behavior and backend API contracts remain canonical in `docs/task-specs/` and `backend/`.

## Evidence on Hand

- Canonical product behavior and backend contracts: `../docs/task-specs/`.
- Frontend2 route inventory: `../docs/task-specs-frontend2/PAGES.md`.
- Existing functional dashboard behavior: `../frontend/src/pages/admin/AdminDashboardPage.tsx` and its tests.
- No verified testimonials, customer logos, comparative benchmarks, advanced analytics, or historical performance claims are available for the redesign.

## Product Principles

- Keep appointment operations legible at a glance for a business owner.
- Preserve accurate business, appointment, StaffMember, status, currency, and timezone information.
- Keep public reservation and Admin operations coherent without conflating their account boundaries.
- Prefer clear, direct interactions over decorative complexity.

## Accessibility & Inclusion

- Meet WCAG 2.2 AA for applicable web UI: contrast, keyboard operation, visible focus, semantic structure, accessible names, and responsive reflow.
- Support desktop and mobile use for every implemented screen.
