# Frontend 2 Redesign Specs

This folder contains implementation specs for the isolated `frontend2/` redesign candidate.

Start from `PAGES.md` to select the next business concept, page, or subflow to design.

## Boundaries

- `../task-specs/` remains the canonical source for product use cases, backend behavior, and API contracts.
- `../../task-groups-roadmap.md` remains authoritative for product delivery status.
- These specs document the new UI, interaction design, responsive behavior, accessibility, and tests for `frontend2/`.
- Reference canonical specs rather than copying backend contracts. Record only frontend2-specific presentation considerations here.
- Do not mark a product capability complete in the roadmap for a scaffold or a placeholder.

## Workflow

1. Start a redesign slice from `TEMPLATE.md`.
2. Link the relevant canonical product spec and roadmap IDs.
3. Specify the behavior that must remain compatible with the current frontend.
4. Implement and verify the route in `frontend2/`.
5. Record the implementation state in the redesign spec.

The current frontend in `../../frontend/` remains independent and is not changed by these specs.
