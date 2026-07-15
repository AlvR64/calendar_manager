# <Spec Name>

## Roadmap IDs

- `<id>`

## Current Status Matrix

Mirror the current state from `task-groups-roadmap.md` for quick context. The roadmap remains authoritative.

| ID | Backend | Frontend | Diseno | Postpuesto |
| --- | --- | --- | --- | --- |
| `<id>` | [ ] | [ ] | [ ] | [ ] |

## Goal

State the user/business outcome this slice must provide.

## Scope

- What is included in this spec.

## Non-Goals

- What is deliberately excluded, even if related.

## Backend Contract

Document the backend behavior the frontend or feature depends on.

| Endpoint / Use Case | Auth | Request | Success Response | Error Cases |
| --- | --- | --- | --- | --- |
| `METHOD /path` | Required/None | DTO/body/query | DTO/status | Validation/auth/not-found/conflict |

## Design References

- `designs/<file>.op`

## Frontend Routes And Screens

- `/route`

## UX States

- Loading
- Empty
- Error
- Validation errors
- Success
- Unauthorized/forbidden when applicable

## Data And Validation Rules

- Backend validation remains authoritative.
- Mirror simple client-side validation when useful.
- Note timezone/currency/date handling when applicable.

## Implementation Plan

1. Add or update API types/client functions.
2. Add or update query/mutation hooks.
3. Build forms/components/screens.
4. Wire route/page behavior.
5. Cover loading, empty, error, validation, and success states.
6. Add or update tests.
7. Update `task-groups-roadmap.md` when complete.

## Test Plan

- Unit/component tests:
- Integration/manual checks:
- Commands to run:

## Acceptance Criteria

- [ ] The feature satisfies the roadmap IDs listed above.
- [ ] UI follows the referenced OpenPencil design where a design exists.
- [ ] Backend contract is respected.
- [ ] Loading, empty, error, validation, and success states are handled where applicable.
- [ ] Relevant tests/checks pass.
- [ ] `task-groups-roadmap.md` is updated after completion.

## Open Questions

- None.
