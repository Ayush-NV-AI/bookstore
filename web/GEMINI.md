# Web directory rules (loaded just-in-time when a tool touches web/)

- Run `npm --prefix web run build` after every change; it type-checks with `tsc` first.
- New pages go in `web/src/pages`, typed API calls in `web/src/api`. Do not grow `App.tsx` beyond routing and layout.
- Do not add npm packages. React Router is the only routing library allowed.
- Every element a Playwright test needs gets a `data-testid`.
