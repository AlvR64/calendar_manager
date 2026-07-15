# Task Specs

Task specs describe how to implement product/use-case slices without becoming the source of truth for status.

`task-groups-roadmap.md` remains the single source of truth for whether backend, frontend, and design are done. Specs explain scope, contracts, implementation plan, and acceptance criteria for a task or feature slice.

## Rules

- Keep specs at product/use-case level, not split into separate frontend and backend specs.
- Reference roadmap IDs from `task-groups-roadmap.md` in every spec.
- Use `TEMPLATE.md` when creating a new spec.
- Keep specs concise and implementation-oriented.
- Update `task-groups-roadmap.md` when a spec is completed.
- Do not mark roadmap `Frontend` as done for placeholders; mark it only when UI/API behavior is implemented and verified.
- Do not mark roadmap `Backend` as done until the API/application behavior and relevant tests are complete.
- Do not mark roadmap `Diseno` as done unless the OpenPencil design exists and covers the case.

## Current Phases

- `first-mvp/`: specs for closing the current frontend gap against backend features and existing OpenPencil designs.
