# Frontend 2 OpenPencil Archive

This folder preserves the historical OpenPencil exploration work for the isolated `frontend2/` redesign. It is not the active visual authority. The active Impeccable context lives in `frontend2/PRODUCT.md`, `frontend2/DESIGN.md`, and `frontend2/.impeccable/`.

## Historical Scope And Separation

- Files in `../` are existing frontend references. Treat them as read-only.
- Do not update these OpenPencil designs unless a task explicitly reactivates OpenPencil.
- Do not overwrite, rename, or use an existing frontend design as the destination for frontend2 work.
- Use one `.op` file per page or flow, for example `homepage-explorations.op`.
- Keep competing visual directions in the same exploration file until one is approved.
- Record approved design decisions and implementation scope in `docs/task-specs-frontend2/`.
- Do not implement frontend code, change backend behavior, or change business rules during a design-only task unless explicitly requested.

## Visual Direction

The visual language is dark-first, modular, compartmentalized, modern, friendly, clear, and technically distinctive.

It should feel solid, structured, useful, pleasant, and coherent across screens. It must not feel hacker-like, bleak, aggressively cyberpunk, terminal-only, or like generic white SaaS.

## Composition Principle

Use large panels with meaningful internal divisions rather than a loose collection of floating cards.

- Organize content through panels, boxes, visible borders, dividers, and clear hierarchy.
- Favor robust grids and aligned groups of related content.
- Use a large panel with internal columns or rows where it improves comprehension.
- Avoid elements that appear to float without a structural relationship.
- Group dashboard metrics inside a shared panel with dividers when appropriate.

## Surfaces And Borders

- Use charcoal, graphite, and dark slate surfaces instead of pure black.
- Differentiate surfaces with subtle luminance changes.
- Make borders visible but soft; borders are a primary part of the visual language.
- Use borders and dividers for panels, subpanels, inputs, filters, lists, metric groups, and form sections.
- Rely primarily on surface contrast, borders, spacing, and typography for separation.
- Use shadows rarely, subtly, and only when they add clear hierarchy.
- Do not use decorative glassmorphism, gratuitous blur, or pervasive decorative gradients.

## Shape And Density

- Large panels: approximately 12px to 16px radius.
- Standard components: approximately 8px to 12px radius.
- Inputs and buttons: approximately 8px to 10px radius.
- Use pill shapes only when their semantics justify them, such as compact status badges.
- Avoid oversized radii and an interface made entirely of pills.

## Typography And Color

- Use sans-serif for navigation, forms, names, descriptions, and general content.
- Use monospace selectively for time, dates, metrics, numeric values, or small operational labels.
- Keep typography human and readable; reserve strong weights for headings, key metrics, and important labels.
- Do not make every label heavy or high contrast.
- Use one primary accent color, or a tightly controlled accent palette.
- Let the accent guide attention through primary actions, key labels, selected states, and occasional leading panels.
- Do not let accent color dominate the whole screen or replicate another product's visual identity.

## Hierarchy And Iconography

- Clearly distinguish title, primary data, secondary data, context, action, and status.
- Keep secondary text quieter without compromising legibility.
- Use simple, practical icons to support reading and action.
- Do not use large decorative icons or iconography as the main visual subject.

## Product Worlds

Public, Customer, and Admin areas share one design system but have different density.

### Admin

- More dense and operational.
- Stronger modular organization and grouped data.
- Optimized for control, scanning, forms, filters, and repeated actions.

### Public And Customer

- More breathing room, reassurance, and clarity.
- Still panel-based and structured, but less dense than Admin.
- Optimized for trust, service discovery, appointment selection, and understanding next steps.

## Reusable Patterns

Build screens from a small, consistent set of patterns:

- App shell and sidebar.
- Page header and top summary panel.
- Panel and panel header.
- Metric group and section label.
- Buttons, inputs, selects, and textareas.
- Status badges.
- Empty states, loading states, and error states.
- Record and list items.
- Filter bars and filter panels.
- Split-panel layouts and grouped data blocks.
- Form sections and accessible destructive-action dialogs.

## Previous Design Workflow

When designing a screen or flow:

1. Read the relevant entry in `docs/task-specs-frontend2/PAGES.md`.
2. Read the linked canonical product spec in `docs/task-specs/` and inspect existing behavior when needed.
3. Check whether a related frontend2 `.op` file already exists.
4. This previous workflow used OpenPencil MCP to create or update frontend2 design files.
5. Preserve existing functionality; do not invent product features except obvious minor UX improvements.
6. Improve information hierarchy and presentation when useful without changing product behavior unnecessarily.
7. Keep desktop and mobile behavior intentional.
8. Identify loading, empty, error, validation, success, unauthorized, and destructive-action states where applicable.
9. If designs conflict, propose a reasonable normalization based on this guide.
10. The active workflow records approved decisions in frontend2 Impeccable context and the relevant frontend2 spec before implementation.

## Acceptance Criteria

A frontend2 design is valid when it:

- Feels dark-first, modular, compartmentalized, and friendly.
- Uses panels and meaningful internal divisions rather than disconnected cards.
- Avoids hacker, terminal, harsh cyberpunk, and generic white-SaaS aesthetics.
- Uses controlled color, soft but visible borders, and restrained shadows.
- Has disciplined typographic hierarchy and readable contrast.
- Shares a coherent system with the other frontend2 screens.
- Supports responsive layouts and accessible interaction patterns.
- Can serve as a reliable reference for future designs and implementation.

## Adjustable Decisions

These principles are stable. Refine the exact accent color, typeface, density, radius scale, and page compositions progressively through the exploration files until they are approved.
