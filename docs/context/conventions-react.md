# React / TypeScript conventions

- TypeScript `strict: true`. No `any` — model API responses in `web/src/api/types.ts`.
- Function components + hooks. One component per file, PascalCase file names.
- API calls only through `web/src/api/*.ts`, which call relative URLs (`/api/v1/...`). Never hard-code
  `http://localhost:5080` — the Vite dev proxy and the nginx container both forward `/api`.
- Every async view renders three states: loading, error (with a retry), and empty.
- **Prices come from the API.** The client formats with `formatInr()` in `web/src/lib/money.ts`; it never
  calculates discounts, tax or totals.
- Stable `key` props (the entity id), never `Math.random()` or the array index.
- Every interactive element a test touches has a `data-testid` (kebab-case, e.g. `catalog-search-input`).
- Styles live in `web/src/styles.css` with class names; no inline style objects for layout.
