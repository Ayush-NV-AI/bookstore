#!/usr/bin/env node
// constitution-gate — runs the constitution review HEADLESSLY and turns the verdict into an exit code.
//
//   node scripts/ci/constitution-gate.mjs                    prepare + call `gemini -p` + evaluate (local / any CI)
//   node scripts/ci/constitution-gate.mjs --prepare-only     write gate-input.md and stop (CI step 1)
//   node scripts/ci/constitution-gate.mjs --evaluate FILE    evaluate a saved model response (CI step 3)
//
// Options:  --base <ref>   diff base (default: $GATE_BASE_REF, else origin/main, else main)
//
// Exit codes (the contract CI relies on):
//   0  PASS   — no BLOCKER or MAJOR finding (or nothing to review)
//   1  FAIL   — at least one BLOCKER or MAJOR finding
//   2  ERROR  — the gate could not decide: Gemini CLI exited non-zero (1 API error, 42 input error,
//               53 turn limit) or the response was not the JSON we asked for. Treat as "not approved".
//
// Writes gate-report.md (human-readable) next to the repo root for the PR / CI summary.

import { execSync, spawnSync } from 'node:child_process'
import { existsSync, readFileSync, writeFileSync } from 'node:fs'

const args = process.argv.slice(2)
const flag = (name) => args.includes(name)
const option = (name) => {
  const i = args.indexOf(name)
  return i >= 0 ? args[i + 1] : undefined
}

const INPUT_FILE = 'gate-input.md'
const REPORT_FILE = 'gate-report.md'
const CLI_EXIT_MEANING = { 1: 'general error or API failure', 42: 'input error (invalid prompt or arguments)', 53: 'turn limit exceeded' }

function git(command) {
  return execSync(`git ${command}`, { encoding: 'utf8', maxBuffer: 20 * 1024 * 1024 }).trim()
}

function resolveBase() {
  const candidates = [option('--base'), process.env.GATE_BASE_REF, 'origin/main', 'main'].filter(Boolean)
  for (const ref of candidates) {
    try {
      git(`rev-parse --verify --quiet ${ref}`)
      return ref
    } catch {
      // try the next candidate
    }
  }
  throw new Error(`none of these base refs exist: ${candidates.join(', ')}`)
}

function prepare() {
  const base = resolveBase()
  const diff = git(`diff --unified=3 ${base}...HEAD -- src tests web/src web/e2e`)
  const files = git(`diff --name-only ${base}...HEAD -- src tests web/src web/e2e`)
  const constitution = readFileSync('.specify/memory/constitution.md', 'utf8')
  const styleguide = existsSync('.gemini/styleguide.md') ? readFileSync('.gemini/styleguide.md', 'utf8') : '(no style guide yet)'

  const content = `# Constitution gate input

Base: ${base} · Head: ${git('rev-parse --short HEAD')}

## Constitution (the only rules that decide the verdict)

${constitution}

## Style guide (advisory; never on its own a BLOCKER)

${styleguide}

## Changed files

${files || '(none)'}

## Diff

\`\`\`diff
${diff}
\`\`\`

## Your task (merge gate)

You are a strict code reviewer acting as a merge gate. Judge ONLY the added or changed lines in the diff
against constitution principles I–VII.

- BLOCKER: violates a NON-NEGOTIABLE principle (IV Money Is Decimal, V Test-Gated Delivery) or exposes a secret.
- MAJOR: violates any other principle.
- MINOR: style-guide or readability issue only.

Do not invent findings. If you are not sure, leave it out. Answer with ONE JSON object and nothing else:

\`\`\`json
{"verdict":"FAIL","findings":[{"principle":"IV","severity":"BLOCKER","file":"src/BookStore.Api/Features/Example/ExampleService.cs","line":12,"finding":"Discount is a double","fix":"Use decimal"}]}
\`\`\`

verdict is FAIL if and only if there is at least one BLOCKER or MAJOR finding; otherwise PASS with the remaining findings.
`
  writeFileSync(INPUT_FILE, content)
  return { base, empty: diff.length === 0 }
}

// Short on purpose: it travels on the command line. The full task is at the end of gate-input.md (stdin).
const INSTRUCTION = 'Act as the merge gate described at the end of the input. Answer with the JSON object only.'

function callGemini() {
  const input = readFileSync(INPUT_FILE, 'utf8')
  // shell:true lets Windows resolve gemini.cmd; the instruction contains no shell metacharacters.
  const run = spawnSync('gemini', ['-p', `"${INSTRUCTION}"`, '--output-format', 'json'], {
    input,
    encoding: 'utf8',
    shell: true,
    maxBuffer: 20 * 1024 * 1024,
    timeout: 300_000,
  })
  if (run.error) return { cliExit: 2, error: run.error.message }
  if (run.status !== 0) return { cliExit: run.status ?? 2, error: (run.stderr || run.stdout || '').slice(-1500) }
  try {
    return { cliExit: 0, response: JSON.parse(run.stdout).response }
  } catch {
    return { cliExit: 0, response: run.stdout }
  }
}

function parseVerdict(text) {
  const cleaned = String(text ?? '').replace(/```(?:json)?/g, '').trim()
  const start = cleaned.indexOf('{')
  const end = cleaned.lastIndexOf('}')
  if (start < 0 || end <= start) return null
  try {
    const parsed = JSON.parse(cleaned.slice(start, end + 1))
    if (!['PASS', 'FAIL'].includes(parsed.verdict) || !Array.isArray(parsed.findings)) return null
    return parsed
  } catch {
    return null
  }
}

function report(verdict, findings, note) {
  const rows = findings.map((f) => `| ${f.severity} | ${f.principle} | \`${f.file}:${f.line ?? '?'}\` | ${String(f.finding).replace(/\|/g, '/')} | ${String(f.fix ?? '').replace(/\|/g, '/')} |`)
  const md = [
    `## Constitution gate: ${verdict}`,
    '',
    note ?? '',
    '',
    findings.length ? '| Severity | Principle | Location | Finding | Fix |\n|---|---|---|---|---|\n' + rows.join('\n') : '_No findings._',
    '',
  ].join('\n')
  writeFileSync(REPORT_FILE, md)
  console.log(md)
}

function evaluate(responseText) {
  const verdict = parseVerdict(responseText)
  if (!verdict) {
    report('ERROR', [], 'The model did not return the JSON verdict the gate asked for. Treat as not approved.')
    return 2
  }
  // Recompute the verdict from the findings — never trust the model's own summary line.
  const blocking = verdict.findings.filter((f) => ['BLOCKER', 'MAJOR'].includes(String(f.severity).toUpperCase()))
  const final = blocking.length > 0 ? 'FAIL' : 'PASS'
  report(final, verdict.findings, final !== verdict.verdict ? `Note: model said ${verdict.verdict}; gate recomputed ${final} from the findings.` : undefined)
  return final === 'PASS' ? 0 : 1
}

try {
  if (flag('--evaluate')) {
    const file = option('--evaluate')
    process.exit(evaluate(readFileSync(file, 'utf8')))
  }

  const { base, empty } = prepare()
  console.log(`constitution-gate: prepared ${INPUT_FILE} (base ${base})`)
  if (flag('--prepare-only')) process.exit(0)

  if (empty) {
    report('PASS', [], 'Nothing to review: no changes under src, tests, web/src or web/e2e.')
    process.exit(0)
  }

  const result = callGemini()
  if (result.cliExit !== 0) {
    report('ERROR', [], `Gemini CLI exited ${result.cliExit} (${CLI_EXIT_MEANING[result.cliExit] ?? 'unknown'}).\n\n\`\`\`\n${result.error ?? ''}\n\`\`\``)
    process.exit(2)
  }
  process.exit(evaluate(result.response))
} catch (error) {
  report('ERROR', [], `The gate itself failed: ${error.message}`)
  process.exit(2)
}
