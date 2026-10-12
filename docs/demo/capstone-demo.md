# Capstone demo — 6 minutes per team

The demo shows the **pipeline**, not the app. Every claim is backed by an artifact on screen.

| # | Minute | Show | Say (one line) |
|---|---|---|---|
| 1 | 0:00 | `.specify/memory/constitution.md` (scroll to IV and V) | "These seven rules decided everything you are about to see." |
| 2 | 0:40 | `specs/001-book-catalog/` tree → `analysis-report.md` row K1 | "The analyzer caught a missing test before a line of code existed." |
| 3 | 1:20 | `specs/002-orders-cart/plan.md` → Agent Orchestration diagram | "Three agents in parallel, one contract as the seam, the parent integrates." |
| 4 | 2:00 | Terminal: `node .gemini/hooks/test-constitution-guard.mjs` | "This is enforcement that does not depend on the model choosing to comply." |
| 5 | 2:40 | The merged pull request: one Gemini Code Assist comment and the fix commit | "The reviewer flagged it, a human decided, the fix is traceable." |
| 6 | 3:20 | Terminal: `node scripts/ci/constitution-gate.mjs` → exit code | "Same check, headless, as a gate. Zero means merge, one means stop." |
| 7 | 4:00 | `docs/lab/regression-*.md` (all GREEN) | "Every suite, both stacks, one command." |
| 8 | 4:40 | Browser on `http://localhost:8080`: search → book → add to cart → checkout → confirmation | "The product — the last thing we show, because it is the least surprising." |
| 9 | 5:30 | One sentence: what you would change in the constitution after this week | — |

## Rules for presenters

- No live coding. If something breaks, show the artifact and say what broke — that is a valid demo.
- Keep the terminal font at 18 pt or larger.
- One presenter drives, one narrates.
