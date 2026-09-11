# Frontend 2 Page Inventory

This is the working inventory for the `frontend2/` redesign. It groups existing product behavior by business concept so a design can be selected and implemented one slice at a time.

`docs/task-specs/` remains canonical for product behavior and backend contracts. `task-groups-roadmap.md` remains canonical for product delivery status. This file is a frontend2 design tracker only.

## How To Use This Inventory

For each item, track work in a dedicated frontend2 spec created from `TEMPLATE.md`.

- [ ] Design selected
- [ ] Frontend2 spec prepared
- [ ] Implemented in frontend2
- [ ] Responsive review complete
- [ ] Tests and verification complete

The current frontend remains in `../../frontend/`. A frontend2 placeholder does not count as an implementation.

## 0. Shared Foundations

These are reusable foundations, not independent product pages. Define them before or alongside the first page that needs them.

| ID | Item | Scope |
| --- | --- | --- |
| `F2-00` | Visual system | Typography, color, spacing, elevation, radii, icons, and breakpoints. |
| `F2-01` | Public shell | Header for anonymous, Customer, and Admin sessions; logout; mobile navigation. |
| `F2-02` | Auth shell | Shared structure for Customer and Admin authentication or registration. |
| `F2-03` | Admin shell | Admin identity, navigation, logout, and responsive navigation behavior. |
| `F2-04` | Shared UX states | Loading, empty, error, validation, success, and forbidden states. |
| `F2-05` | Shared components | Forms, cards, badges, pagination, toast feedback, and accessible confirmation dialogs. |

## 1. Marketplace And Discovery

Discovering businesses and moving from a search result to a public business profile or appointment selection.

### `F2-MKT-01` Homepage

- Route: `/`
- Audience: public, Customer, and Admin.
- Purpose: marketplace entry point and search starting point.
- Design: hero, search form, categories, business highlights, product benefits, and session-aware calls to action.
- Important states: anonymous, Customer session, Admin session, logout, and mobile search.
- Existing reference: `designs/homepage.op`.
- Canonical references: Marketplace Discovery specs, especially `03-marketplace-search-frontend.md` and `07-public-session-header-and-auth-boundaries.md`.
- Frontend2 spec: `marketplace/01-homepage.md`.

### `F2-MKT-02` Marketplace Search

- Route: `/search`
- Audience: public.
- Purpose: search, filter, compare, and navigate to active businesses.
- Design: search form, active filter chips, results count, business cards, and pagination.
- Important states: unfiltered listing, filtered results, loading, no results, no businesses, API error, and first/middle/last pages.
- Existing reference: `designs/marketplace-search.op`.
- Canonical references: Marketplace Discovery specs `03` through `07`.

### `F2-MKT-03` Public Business Profile

- Route: `/b/:slug`
- Audience: public.
- Purpose: show a business, its services, its StaffMembers, and the appointment call to action.
- Design: business hero, contact and location details, service catalog, staff list, and appointment actions.
- Important states: loading, not found, API error, sparse business data, no active services, and no active StaffMembers.
- Existing reference: `designs/business-public-profile.op`.
- Canonical reference: `docs/task-specs/first-mvp/07-public-business-profile.md`.

## 2. Customer

Customer identity and access. Customer appointment management is listed under Appointments because that is its primary business concept.

### `F2-CUS-01` Customer Login

- Route: `/auth/customer/login`
- Purpose: establish a Customer session and return to the requested safe route.
- Design: email/password form, registration link, and optional product context.
- Important states: validation, submitting, credentials error, registration-success message, and `returnTo` appointment flow.
- Existing reference: `designs/customer-login.op`.
- Canonical references: `docs/task-specs/first-mvp/01-auth-and-registration.md` and appointment creation behavior.

### `F2-CUS-02` Customer Registration

- Route: `/auth/customer/register`
- Purpose: create a Customer account.
- Design: name, optional phone, email, password, and login link.
- Important states: validation, duplicate email, submitting, API error, success, and preserved appointment return route.
- Existing reference: `designs/customer-register.op`.
- Canonical references: `docs/task-specs/first-mvp/01-auth-and-registration.md` and appointment creation behavior.

## 3. Appointments

Selecting, creating, viewing, cancelling, and operating Appointments. An Appointment is always scheduled against a StaffMember.

### `F2-APT-01` Appointment Selection

- Route: `/b/:slug/appointment`
- Audience: public.
- Purpose: select service, optional StaffMember, date, and a backend-provided available slot.
- Internal steps: preferences, slot loading, and slot selection.
- Important states: profile loading/error, no services, no StaffMembers, no staff for a service, invalid date, slots loading, no slots, and selected slot.
- Existing reference: `designs/public-appointment-slot-flow.op`.
- Canonical references: `docs/task-specs/first-mvp/08-public-slot-flow.md` and `docs/task-specs/appointments-mvp/01-customer-appointment-creation.md`.

### `F2-APT-02` Appointment Confirmation

- Route: `/b/:slug/appointment`
- Audience: Customer; anonymous visitors are gated and Admin remains read-only.
- Purpose: confirm the selected slot and create an Appointment.
- Design: selection summary, Customer notes, auth gate, confirmation, and result.
- Important states: no selected slot, anonymous Customer, Admin read-only, Customer authenticated, creating, validation/API error, slot conflict (`409`), and created success.
- Existing reference: partial coverage in `designs/public-appointment-slot-flow.op`.
- Canonical reference: `docs/task-specs/appointments-mvp/01-customer-appointment-creation.md`.

### `F2-APT-03` Appointment Detail

- Route: `/appointments/:appointmentId`
- Audience: Customer owner or Admin of the business.
- Purpose: reloadable confirmation and detail view.
- Design: business, service, StaffMember, Customer, local date/time, business timezone, status, notes, and cancellation information.
- Important states: anonymous gate, loading, scheduled, completed, no-show, cancelled, not found, forbidden, and API error.
- Existing reference: none.
- Canonical reference: `docs/task-specs/appointments-mvp/02-appointment-detail.md`.

### `F2-APT-04` Customer Appointments

- Route: `/customer/appointments`
- Audience: Customer.
- Purpose: review upcoming, past, and cancelled Appointments.
- Design: filters, grouped or sectioned appointment cards, detail links, and logout.
- Important states: loading, no Appointments, no filtered results, empty sections, and API error.
- Existing reference: none.
- Canonical reference: `docs/task-specs/appointments-mvp/03-customer-appointment-list-and-cancellation.md`.

### `F2-APT-05` Customer Cancellation

- Parent route: `/customer/appointments`
- Type: dialog or mobile bottom sheet, not an independent route.
- Purpose: cancel an eligible future Appointment with an optional reason.
- Important states: confirmation, pending, success, conflict/already cancelled, and API error.
- Existing reference: none.

### `F2-APT-06` Admin Appointment Management

- Route: `/admin/appointments`
- Audience: Admin.
- Purpose: operate the business agenda by date range, StaffMember, service, and status.
- Design: filters, grouped appointments, Customer details, status, internal notes, and appointment actions.
- Important states: default range, draft/applied filters, loading, empty range, dense days, API error, and cancelled appointments.
- Existing reference: none.
- Canonical references: `docs/task-specs/appointments-mvp/04-admin-appointment-calendar-and-list.md` and `05-admin-appointment-actions.md`.

### `F2-APT-07` Admin Appointment Actions

- Parent route: `/admin/appointments`
- Type: inline controls and accessible dialogs, not independent routes.
- Actions: change operational status, edit internal notes, and cancel an Appointment.
- Important states: saving, success, error, conflict, confirmation, and disabled actions after cancellation.
- Existing reference: none.

## 4. Admin

Admin identity, navigation, and operational overview. Business settings and management areas are grouped under their respective business concepts.

### `F2-ADM-01` Admin Login

- Route: `/auth/admin/login`
- Purpose: establish an Admin session and return to a safe requested route.
- Design: email/password form, business-registration link, and optional product context.
- Important states: validation, submitting, credentials error, and registration-success message.
- Existing reference: `designs/business-admin-login.op`.
- Canonical reference: `docs/task-specs/first-mvp/01-auth-and-registration.md`.

### `F2-ADM-02` Admin Shell

- Routes: all `/admin` routes.
- Type: shared layout.
- Purpose: consistently expose Admin identity, navigation, business context, logout, and public-site access.
- Important states: desktop sidebar, compact navigation, mobile menu open/closed, and expired session.
- Existing references: the existing admin design files provide partial layout context.

### `F2-ADM-03` Admin Dashboard

- Route: `/admin`
- Purpose: summarize business activity and link to operational work.
- Design: today count, estimated revenue, upcoming Appointments, status summary, and agenda shortcut.
- Important states: loading, no upcoming Appointments, no status data, API error, and populated data.
- Existing reference: `designs/business-admin-dashboard.op` is a partial visual reference only.
- Canonical reference: `docs/task-specs/appointments-mvp/06-dashboard-basics.md`.

## 5. Business

Creating a business and maintaining its public and booking configuration.

### `F2-BUS-01` Business Registration

- Route: `/auth/business/register`
- Audience: prospective Admin.
- Purpose: create a business and its initial Admin.
- Design: business name, slug, IANA timezone, currency, Admin details, and sign-in link.
- Important states: validation, invalid slug/timezone/currency, slug or email conflict, submitting, API error, and success redirect.
- Existing reference: `designs/business-admin-register.op`.
- Canonical reference: `docs/task-specs/first-mvp/01-auth-and-registration.md`.

### `F2-BUS-02` Business Settings

- Route: `/admin/business-settings`
- Audience: Admin.
- Purpose: maintain the public business profile and its maximum booking window.
- Design: public details, category, contact, website, address, timezone, currency, slug indicator, and booking-window controls.
- Important states: initial loading, load error, dirty form, validation, independently saving details or booking window, success, and error.
- Existing reference: `designs/business-settings.op`.
- Canonical reference: `docs/task-specs/first-mvp/02-business-settings.md`.

## 6. Services

The catalog of services offered by a business and the StaffMembers assigned to each service.

### `F2-SRV-01` Service Management

- Route: `/admin/services`
- Audience: Admin.
- Purpose: create, edit, activate, deactivate, and delete services.
- Design: active/inactive lists, summary, create/edit form, and destructive-action feedback.
- Important states: loading, empty catalog, create/edit, validation, saving, success/error, and deletion conflict.
- Existing reference: `designs/admin-services.op`.
- Canonical reference: `docs/task-specs/first-mvp/03-services.md`.

### `F2-SRV-02` StaffMember Assignments To A Service

- Parent route: `/admin/services`
- Type: nested panel or section.
- Purpose: assign, activate/deactivate, and unassign StaffMembers for a service.
- Important states: assignment loading, no assignable StaffMembers, inactive assignment or StaffMember warning, duplicate conflict, and unassign confirmation.
- Existing reference: partial coverage in `designs/admin-services.op`.
- Canonical reference: `docs/task-specs/first-mvp/05-staff-service-assignments.md`.

## 7. Staff Members

The people or resources that perform services and receive Appointments.

### `F2-STF-01` StaffMember Management

- Route: `/admin/staff-members`
- Audience: Admin.
- Purpose: create, edit, activate, deactivate, and delete StaffMembers.
- Design: active/inactive lists, summary, contact information, bio, ordering, and create/edit form.
- Important states: loading, empty team, create/edit, validation, saving, success/error, and deletion conflict.
- Existing reference: `designs/admin-staff-members.op`.
- Canonical reference: `docs/task-specs/first-mvp/04-staff-members.md`.

### `F2-STF-02` Service Assignments To A StaffMember

- Parent route: `/admin/staff-members`
- Type: nested panel or section.
- Purpose: assign, activate/deactivate, and unassign services for a StaffMember.
- Important states: assignment loading, no assignable services, inactive assignment or service warning, duplicate conflict, and unassign confirmation.
- Existing reference: partial coverage in `designs/admin-staff-members.op`.
- Canonical reference: `docs/task-specs/first-mvp/05-staff-service-assignments.md`.

## 8. Availability

The working time configured per StaffMember and the exceptions that determine available appointment slots.

### `F2-AVL-01` StaffMember Availability

- Route: `/admin/availability`
- Audience: Admin.
- Purpose: configure recurring weekly availability and date-specific exceptions per StaffMember.
- Design: StaffMember selector, business timezone context, weekly blocks, exceptions, and create/edit controls.
- Important states: staff/business loading, no StaffMembers, changing StaffMember, selected-staff data loading/error, no weekly availability, and no exceptions.
- Existing reference: `designs/admin-availability.op`.
- Canonical reference: `docs/task-specs/first-mvp/06-availability.md`.

### `F2-AVL-02` Weekly Availability Editor

- Parent route: `/admin/availability`
- Type: nested create/edit section.
- Purpose: manage recurring weekly time blocks.
- Important states: create, edit, start/end validation, overlap conflict, saving, error, and delete confirmation.

### `F2-AVL-03` Availability Exception Editor

- Parent route: `/admin/availability`
- Type: nested create/edit section.
- Purpose: configure a closed day, vacation, absence, or special hours.
- Important states: closed-all-day and special-hours variants, validation, conflict, saving, error, and delete confirmation.

## Recommended Design Sequence

1. `F2-00` through `F2-05`: shared visual and interaction foundations.
2. `F2-MKT-01` through `F2-MKT-03`: marketplace discovery.
3. `F2-CUS-01` and `F2-CUS-02`: Customer access.
4. `F2-APT-01` and `F2-APT-02`: appointment selection and confirmation.
5. `F2-APT-03` through `F2-APT-05`: Customer appointment lifecycle.
6. `F2-ADM-01`, `F2-ADM-02`, and `F2-BUS-01`: Admin access and business onboarding.
7. `F2-BUS-02`, `F2-SRV-01`, `F2-SRV-02`, `F2-STF-01`, and `F2-STF-02`: business setup.
8. `F2-AVL-01` through `F2-AVL-03`: availability setup.
9. `F2-APT-06`, `F2-APT-07`, and `F2-ADM-03`: daily operations and summary.
10. Cross-route pass: responsive behavior, accessibility, long content, API outage, forbidden access, and destructive-action dialogs.
