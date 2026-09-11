# Frontend2 Homepage

## Status

In design exploration.

## Objective

Create a compact, marketplace-first public Homepage that helps visitors start a real business search while establishing the frontend2 dark, modular visual system.

## Capability IDs

- `F2-00`
- `F2-01`
- `F2-04`
- `F2-05`
- `F2-MKT-01`
- `0.8`
- `0.10`
- `7.2`
- `8.1`
- `11.9`
- `11.11`
- `11.13`
- `11.14`
- `11.15`

## Canonical Product References

- `../../task-specs/marketplace-discovery-mvp/03-marketplace-search-frontend.md`
- `../../task-specs/marketplace-discovery-mvp/06-marketplace-filter-catalogs.md`
- `../../task-specs/marketplace-discovery-mvp/07-public-session-header-and-auth-boundaries.md`

## Routes And Screens

- Homepage: `/`
- Search destination: `/search`
- Related public destinations: `/b/:slug` and `/b/:slug/appointment`
- Design exploration: `../../designs/frontend2/homepage-explorations.op`

## Existing Behavior To Preserve

- The Homepage starts a search at `/search` using optional `query`, `city`, `category`, and `service` query parameters.
- Category shortcuts use canonical values: `barber`, `beauty`, `physiotherapy`, `classes`, and `consulting`.
- Anonymous visitors can access Customer login and business registration.
- Customer sessions show access to `/customer/appointments`; Admin sessions show access to `/admin`.
- Customer and Admin remain distinct account types, and logout returns the public shell to its anonymous state.
- The public homepage does not create Appointments or make Admin a Customer appointment creator.

## Design Direction

- Follow `../../designs/frontend2/README.md`.
- Use a dark-first, friendly, modular marketplace composition with visible but soft borders and restrained accent color.
- Keep the page compact: the search experience is the main subject, not long-form marketing content.
- Compare three desktop directions: Amber Grid, Blue Search Deck, and Teal Matrix.
- Create a mobile design only after a desktop direction is approved.

## Page Content

1. Session-aware public header.
2. Brief marketplace proposition.
3. Search panel with text, city, category, service, and submit action.
4. Canonical category shortcuts.
5. Illustrative business preview clearly labeled as an example.
6. Compact value statements: clear service details, connected StaffMembers, and business-local timezone.
7. Compact Customer and business calls to action.

## Data Boundaries

- The initial design uses an illustrative preview, not a live homepage business list.
- Do not present staff names, slots, availability, popularity, ratings, images, distance, or featured status as live data.
- Do not add maps, reviews, favorites, immediate-availability filters, or additional categories.
- The preview may show representative service, duration, and price formatting only when labeled as illustrative.

## UX States

- Anonymous header.
- Customer header.
- Admin header.
- Logout to anonymous state.
- Default, partially completed, and completed search form.
- Category selected.
- Compact mobile search and navigation after the desktop direction is approved.

## Responsive And Accessibility

- Design an intentional 390px mobile frame after desktop approval.
- Keep login, business-registration, authenticated destination, and logout reachable on mobile.
- Use visible focus states and touch-friendly search/category controls.
- Preserve semantic heading order and do not rely on hover-only actions.
- Support long business names, descriptions, Customer/Admin names, and emails without clipping essential content.

## Design Plan

1. Generate three desktop exploration frames in OpenPencil.
2. Review and select or combine a direction.
3. Create the approved desktop and mobile frames.
4. Add public-header session variants.
5. Record approved tokens, components, and behavior in this spec.
6. Implement in frontend2 only after explicit approval.

## Acceptance Criteria

- [x] Three desktop concepts exist in `homepage-explorations.op`.
- [ ] Each concept contains the real four-filter search model.
- [ ] Each concept uses only canonical categories.
- [ ] The business preview is visibly illustrative.
- [ ] The design is compact, dark-first, modular, and friendly.
- [ ] The design does not imply unsupported data or functionality.
- [ ] An approved desktop direction, mobile frame, and header states are documented before implementation.
