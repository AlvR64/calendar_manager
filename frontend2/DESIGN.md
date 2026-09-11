# Frontend2 Design System

## Overview

Frontend2 uses a dark-first operational interface for business owners who need to scan and act on an appointment agenda. The system is structured through shared panels, fine borders, and practical data typography rather than floating-card stacks or decorative effects.

## Colors

- Background: `#0f0f0f`.
- Primary surface: `#161616`.
- Raised or interactive surface: `#1b1b19` and `#20201d`.
- Borders and dividers: `#2a2a28` through `#454540`.
- Main text: `#f3f4ed`; secondary text: `#989994` through `#aaa9a4`.
- Operational accent: `#9cdc17`. Reserve it for primary actions, focused navigation, and live operational markers.
- Status colors are muted and semantic: green for scheduled, teal for completed, and warm red for cancelled states.

## Typography

- Use the platform sans-serif stack for content, navigation, and controls.
- Use `ui-monospace` only for operational labels, times, dates, ranges, statuses, and numeric values.
- Headings carry hierarchy through scale, weight, and tight but readable tracking. Do not add decorative eyebrow text to compensate for weak headings.

## Layout

- Admin surfaces use a persistent desktop sidebar and a compact horizontal navigation bar on small screens.
- Compose pages from large shared panels with internal dividers. Avoid nested cards.
- The page's most immediate operational task receives the largest surface. On the Admin Dashboard, this is the upcoming Appointment agenda.
- Stack metric strips and agenda metadata on mobile without hiding task-critical information.

## Elevation & Depth

- Rely on luminance changes and 1px borders. Shadows are exceptional, not the default separation mechanism.
- Do not use gradients, glass effects, glows, or detached floating elements as decoration.

## Shapes

- Use square to lightly rounded operational surfaces; status badges can be compact rectangles.
- Avoid ubiquitous pills and oversized radii.

## Components

- Primary action: lime fill with dark text and a clear border.
- Navigation: muted text by default; active state has a dark raised surface and fine border.
- Agenda rows: time column, Appointment details, and a status marker; rows divide rather than float.
- Loading, empty, and error states keep the same panel language and make recovery actions explicit.

## Do's and Don'ts

- Do make time, status, and next actions easy to scan.
- Do maintain visible focus indicators and semantic HTML.
- Do use the accent sparingly.
- Do not use hacker-terminal motifs, cyberpunk neon, generic white SaaS treatment, glassmorphism, decorative gradients, or analytics charts without a real product need.
