# Migration briefing — Gemini CLI → Antigravity CLI

*Day 3 closing briefing (10 minutes). Status as of the course date; re-check Google's announcements before acting.*

## What is happening

- Agent mode in Gemini Code Assist is a **Preview** feature, and it runs Gemini CLI underneath.
- Google has announced a transition of the CLI towards **Antigravity CLI**; the Gemini CLI documentation now
  carries a notice to that effect.
- For **Standard and Enterprise** customers, access to Gemini CLI is stated to be unchanged. The individual,
  AI Pro and AI Ultra tiers stopped being served by the IDE extensions and Gemini CLI on 18 June 2026 — which
  is why this course required licensed seats.

## What carries over unchanged (it is in your repository, not in the tool)

| Asset | Path | Why it survives a tool change |
|---|---|---|
| Constitution | `.specify/memory/constitution.md` | Plain Markdown; Spec Kit supports many agents |
| Specs, plans, tasks, contracts | `specs/**` | Plain Markdown and OpenAPI |
| Tests | `tests/**`, `web/e2e/**` | Tool-independent evidence |
| Headless gate | `scripts/ci/constitution-gate.mjs` | Only the CLI invocation line changes |
| Review style guide | `.gemini/styleguide.md` | Used by the GitHub review agent, not by the CLI |

## What to re-verify after any migration

| Asset | Path | Check |
|---|---|---|
| Context file + imports | `GEMINI.md`, `docs/context/*` | Is the file name and `@import` syntax the same? |
| Exclusion files | `.aiexclude`, `.geminiignore` | Are they still honoured? |
| Custom commands | `.gemini/commands/**/*.toml` | TOML schema, `{{args}}`, `!{}`, `@{}` |
| Subagents | `.gemini/agents/*.md` | Frontmatter fields |
| Hooks | `.gemini/settings.json` → `hooks` | Event names and the deny/allow JSON contract |
| MCP servers | `.gemini/settings.json` → `mcpServers` | Allow-lists and env-var expansion |
| Spec Kit integration | `specify integration install …` | Re-install the commands for the new agent |

## The one-sentence takeaway

Invest in the artifacts — constitution, specs, contracts, tests, gates — because they outlive any single tool.
