## What and why

<!-- One paragraph. Link the spec: specs/###-feature/spec.md -->

## Traceability

- Spec: `specs/___/spec.md`
- Tasks implemented: T___, T___

## Evidence (Constitution V)

- [ ] `dotnet test` green — paste the summary line
- [ ] `npm --prefix web run test:e2e` green — paste the summary line
- [ ] `/team:preflight capstone/integration` → `PREFLIGHT: READY`
- [ ] `node scripts/ci/constitution-gate.mjs` → exit 0

## Reviewer checklist (human)

- [ ] Every Gemini Code Assist comment of severity HIGH or CRITICAL is fixed or answered with a reason
- [ ] Licence / attribution: agent mode shows no source citations — I checked no large verbatim third-party code was added
- [ ] No change under `.github/workflows/` slipped through without a human read (the review agent skips that folder)
