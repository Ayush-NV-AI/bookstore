#!/usr/bin/env node
// Proves the BeforeTool hook fires — without spending a single model request.
// Usage (repo root): node .gemini/hooks/test-constitution-guard.mjs
import { spawnSync } from 'node:child_process'

const cases = [
  ['money as double', 'deny', { tool_name: 'write_file', tool_input: { file_path: 'src/BookStore.Api/Features/Catalog/Book.cs', content: 'public double DiscountPrice { get; set; }' } }],
  ['string-built SQL', 'deny', { tool_name: 'replace', tool_input: { file_path: 'src/BookStore.Api/Features/Catalog/CatalogService.cs', old_string: 'x', new_string: 'db.Books.FromSqlRaw($"SELECT * FROM \\"Books\\" WHERE \\"Title\\" = \'{t}\'")' } }],
  ['controller uses DbContext', 'deny', { tool_name: 'write_file', tool_input: { file_path: 'src/BookStore.Api/Features/Carts/CartController.cs', content: 'public CartController(BookStoreDbContext db) { }' } }],
  ['write to .env', 'deny', { tool_name: 'write_file', tool_input: { file_path: '.env', content: 'X=1' } }],
  ['API key literal', 'deny', { tool_name: 'write_file', tool_input: { file_path: 'src/BookStore.Api/appsettings.json', content: '{"Key":"sk_live_abcdefghijklmnop"}' } }],
  ['skipped test', 'deny', { tool_name: 'write_file', tool_input: { file_path: 'tests/BookStore.Api.Tests/Carts/CartApiTests.cs', content: '[Fact(Skip = "flaky")]' } }],
  ['decimal money (compliant)', 'allow', { tool_name: 'write_file', tool_input: { file_path: 'src/BookStore.Api/Features/Carts/CartContracts.cs', content: 'public sealed record CartView(decimal Subtotal);' } }],
  ['.env.example (compliant)', 'allow', { tool_name: 'write_file', tool_input: { file_path: '.env.example', content: 'POSTGRES_PASSWORD=\n' } }],
]

let failed = 0
for (const [name, expected, payload] of cases) {
  const run = spawnSync(process.execPath, ['.gemini/hooks/constitution-guard.mjs'], { input: JSON.stringify(payload), encoding: 'utf8' })
  const decision = JSON.parse(run.stdout || '{}').decision
  const ok = decision === expected && run.status === 0
  if (!ok) failed++
  console.log(`${ok ? 'PASS' : 'FAIL'}  ${name.padEnd(28)} expected ${expected.padEnd(5)} got ${decision} (exit ${run.status})`)
}
console.log(failed === 0 ? '\nAll hook cases behave as specified.' : `\n${failed} case(s) misbehaved.`)
process.exit(failed === 0 ? 0 : 1)
