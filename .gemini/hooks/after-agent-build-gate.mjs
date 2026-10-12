#!/usr/bin/env node
// after-agent-build-gate — Gemini CLI AfterAgent hook (OPTIONAL, Day 3 AM extension)
//
// When the agent says it is finished, build whatever it changed. If the build is red, answer "deny" with the
// compiler errors as the reason, so the CLI sends the agent back to fix them instead of ending the turn.
// A retry budget stops an infinite fix-loop.
//
// stdout must contain only the final JSON object.

import { execSync } from 'node:child_process'
import { existsSync, readFileSync, writeFileSync } from 'node:fs'
import { tmpdir } from 'node:os'
import { join } from 'node:path'

const MAX_RETRIES = 2

function respond(obj) {
  process.stdout.write(JSON.stringify(obj))
  process.exit(0)
}

let input = {}
try {
  input = JSON.parse(readFileSync(0, 'utf8'))
} catch {
  respond({}) // never block the end of a turn on a parse problem — this gate is advisory
}

const session = String(input.session_id ?? 'unknown').replace(/[^\w-]/g, '')
const counterFile = join(tmpdir(), `bookstore-build-gate-${session}.json`)
const attempts = existsSync(counterFile) ? JSON.parse(readFileSync(counterFile, 'utf8')).attempts : 0

function run(cmd) {
  try {
    execSync(cmd, { stdio: 'pipe', encoding: 'utf8', timeout: 170_000 })
    return null
  } catch (error) {
    const output = `${error.stdout ?? ''}${error.stderr ?? ''}`
    return output.split(/\r?\n/).filter((l) => /error/i.test(l)).slice(0, 20).join('\n') || output.slice(-2000)
  }
}

let changed = ''
try {
  changed = execSync('git status --porcelain', { encoding: 'utf8' })
} catch {
  respond({})
}

const failures = []
if (/\.(cs|csproj)\s*$/m.test(changed)) {
  const err = run('dotnet build --nologo --verbosity quiet')
  if (err) failures.push(`dotnet build failed:\n${err}`)
}
if (/web\/(src|e2e)\/.*\.(ts|tsx)\s*$/m.test(changed)) {
  const err = run('npm --prefix web run build --silent')
  if (err) failures.push(`web build failed:\n${err}`)
}

if (failures.length === 0) {
  if (existsSync(counterFile)) writeFileSync(counterFile, JSON.stringify({ attempts: 0 }))
  respond({})
}

if (attempts >= MAX_RETRIES) {
  writeFileSync(counterFile, JSON.stringify({ attempts: 0 }))
  respond({ systemMessage: `build-gate: still red after ${MAX_RETRIES} automatic retries — handing back to you.` })
}

writeFileSync(counterFile, JSON.stringify({ attempts: attempts + 1 }))
respond({
  decision: 'deny',
  reason: `You reported the task as done, but the build is red. Fix these errors before finishing:\n\n${failures.join('\n\n')}`,
  systemMessage: `build-gate: build red — sending the agent back (retry ${attempts + 1}/${MAX_RETRIES}).`,
})
