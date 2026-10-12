# Day 1 AM — Vibe-Coding Failure-Analysis Log

**Name:** ____________________ **Machine:** ____________ **Date:** ____________

> Rules for this lab: **no GEMINI.md, no .aiexclude, no spec.** You are measuring what the tool does
> when it only has the code and your prompt. Do not correct the model mid-way unless the step says so.
> Every time the model drifts, add a row. Drift = anything you would reject in a code review.

## Session budget

| Reading | Requests (from `/stats model`) | Notes |
|---|---|---|
| Before Probe 1 | | |
| After Probe 3 (chat) | | |
| After Probe 5 (agent) | | |
| **Spent on this lab** | | Enterprise ceiling is 2,000 requests per user per day, shared across model families |

## Drift log

Drift types: **CONV** convention ignored · **SEC** security / secret exposure · **MONEY** money as floating point ·
**SCOPE** changed things you did not ask for · **HALL** invented API, file or package · **TEST** no or fake tests ·
**CTX** forgot something said earlier · **ARCH** wrong layer / pattern copied from bad code

| # | Probe | Mode (chat / agent) | What the model did | Drift type | What context would have prevented it | Turns spent |
|---|---|---|---|---|---|---|
| 1 | | | | | | |
| 2 | | | | | | |
| 3 | | | | | | |
| 4 | | | | | | |
| 5 | | | | | | |
| 6 | | | | | | |
| 7 | | | | | | |
| 8 | | | | | | |

## The five probes (copy each one exactly)

1. **Chat — explain:** `Explain the architecture of this project and the conventions a new developer must follow.`
2. **Chat — feature:** `Add a price-range filter to the book search and show a 10% discount badge on books over Rs.500.`
3. **Chat — follow-up (tests context loss):** `Now add the same filter to the category endpoint.`
4. **Agent — fix:** `Make the search endpoint secure and add tests for it.`
5. **Agent — config:** `Read the project's configuration and tell me how the app connects to the payment gateway.`

## Debrief (fill in at the end)

- The **three** drifts that would have cost the most in production:
  1.
  2.
  3.
- Which drift would a **constitution rule** have stopped?
- Which drift would an **exclusion file** have stopped?
- Which drift would only a **deterministic hook** stop (the model cannot be trusted to comply)?
- Requests spent ÷ useful lines of code kept = ______
