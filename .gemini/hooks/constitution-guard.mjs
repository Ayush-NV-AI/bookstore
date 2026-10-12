#!/usr/bin/env node
// constitution-guard — Gemini CLI BeforeTool hook (matcher: write_file|replace)
//
// Deterministic enforcement: runs BEFORE every file write the agent attempts and blocks the write if it would
// violate the constitution. The model cannot talk its way past this — it is code, not a prompt.
//
// Contract (Gemini CLI hooks):
//   stdin  : JSON { hook_event_name, tool_name, tool_input: { file_path, content | old_string/new_string }, ... }
//   stdout : exactly one JSON object — { "decision": "allow" } or { "decision": "deny", "reason": "..." }
//   exit 0 : stdout is parsed. (exit 2 would also block, using stderr as the reason.)
//   Never print anything else to stdout.
//
// Rules enforced (each maps to a principle in .specify/memory/constitution.md):
//   VI  — no writes to secret paths (.env*, secrets/, *.pem, *.pfx, *.key) and no key-shaped literals in source
//   IV  — no double/float for money-named members in C#
//   III — no FromSqlRaw/ExecuteSqlRaw built with interpolation or concatenation; controllers never touch the DbContext
//   V   — tests may not be skipped to get green

import { appendFileSync, mkdirSync, readFileSync } from 'node:fs'
import { dirname, join } from 'node:path'
import { fileURLToPath } from 'node:url'

const LOG_FILE = join(dirname(fileURLToPath(import.meta.url)), 'logs', 'constitution-guard.log')

function respond(decision, reason, filePath, toolName) {
  const out = { decision }
  if (decision === 'deny') {
    out.reason = reason
    out.systemMessage = `constitution-guard blocked ${toolName} on ${filePath}`
  }
  try {
    mkdirSync(dirname(LOG_FILE), { recursive: true })
    appendFileSync(LOG_FILE, `${new Date().toISOString()}\t${decision.toUpperCase()}\t${toolName}\t${filePath}\t${decision === 'deny' ? reason.replace(/\s+/g, ' ') : ''}\n`)
  } catch {
    // logging must never break the hook
  }
  process.stdout.write(JSON.stringify(out))
  process.exit(0)
}

let input
try {
  input = JSON.parse(readFileSync(0, 'utf8'))
} catch (error) {
  // Fail closed: if we cannot read the request we cannot vouch for it.
  respond('deny', `constitution-guard could not parse its input (${error.message}); write blocked for safety.`, '?', '?')
}

const toolName = String(input.tool_name ?? '')
const args = input.tool_input ?? {}
const filePath = String(args.file_path ?? args.absolute_path ?? args.path ?? '').replace(/\\/g, '/')
const text = String(args.content ?? args.new_string ?? '')
const lowerPath = filePath.toLowerCase()
const violations = []

// --- Principle VI: secret paths ------------------------------------------------------------------
const isEnvExample = /(^|\/)\.env\.example$/.test(lowerPath)
if (!isEnvExample && (/(^|\/)\.env(\.|$)/.test(lowerPath) || /(^|\/)secrets\//.test(lowerPath) || /\.(pem|pfx|key)$/.test(lowerPath))) {
  violations.push('VI Secrets Stay Out — agents may not write to secret files (.env*, secrets/, *.pem, *.pfx, *.key).')
}

// --- Principle VI: key-shaped literals anywhere in source/config ------------------------------------
if (/\b(sk|pk|rk)_(live|test)_[A-Za-z0-9]{8,}/.test(text) || /-----BEGIN [A-Z ]*PRIVATE KEY-----/.test(text)) {
  violations.push('VI Secrets Stay Out — the content contains an API-key or private-key literal. Use an environment variable.')
}
if (/password\s*=\s*[^;"'\s]+/i.test(text) && !/appsettings\.development\.json$/.test(lowerPath) && !/docker-compose\.ya?ml$/.test(lowerPath)) {
  violations.push('VI Secrets Stay Out — a connection-string password is only allowed in appsettings.Development.json (local Docker).')
}

if (lowerPath.endsWith('.cs')) {
  // --- Principle IV: money is decimal ----------------------------------------------------------------
  const money = text.match(/\b(double|float)\??\s+\w*(price|amount|total|subtotal|cost|discount|tax|balance)\w*/i)
  if (money) {
    violations.push(`IV Money Is Decimal — "${money[0]}" declares money as ${money[1]}. Use decimal (numeric(10,2) in PostgreSQL).`)
  }

  // --- Principle III: no string-built SQL -----------------------------------------------------------
  if (/(FromSqlRaw|ExecuteSqlRaw(Async)?)\s*\(\s*(\$@?"|@\$"|[^)]*"\s*\+)/.test(text)) {
    violations.push('III Layered Backend — raw SQL built with interpolation or concatenation. Use LINQ (or FromSql with a FormattableString).')
  }

  // --- Principle III: controllers never reference the DbContext -------------------------------------
  if (/controller\.cs$/.test(lowerPath) && /\bBookStoreDbContext\b/.test(text)) {
    violations.push('III Layered Backend — a controller references BookStoreDbContext. Move data access into the feature service.')
  }

  // --- Principle V: no skipped tests ----------------------------------------------------------------
  if (/\[(Fact|Theory)\s*\(\s*Skip\s*=/.test(text)) {
    violations.push('V Test-Gated Delivery — tests may not be skipped to get a green run. Fix the code or the test.')
  }
}

if (/\.(ts|tsx)$/.test(lowerPath) && /\b(test|it|describe)\.skip\s*\(/.test(text)) {
  violations.push('V Test-Gated Delivery — Playwright tests may not be skipped to get a green run.')
}

if (violations.length > 0) {
  respond(
    'deny',
    `Blocked by .gemini/hooks/constitution-guard.mjs (deterministic check):\n- ${violations.join('\n- ')}\nRewrite the change so it complies with .specify/memory/constitution.md, then try again.`,
    filePath,
    toolName,
  )
}

respond('allow', '', filePath, toolName)
