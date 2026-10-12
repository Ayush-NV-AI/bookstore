# Security rules

- Never read, echo, summarise or move: `.env`, `.env.*`, `secrets/**`, `**/*.pfx`, `**/*.pem`,
  `appsettings.*.local.json`. These are also listed in `.aiexclude` and `.geminiignore`.
- No credentials in source, config files committed to git, prompts or commit messages. Configuration comes from
  environment variables (`ConnectionStrings__BookStore`) or `dotnet user-secrets`.
- The only exception: the local-only Docker development password in `appsettings.Development.json`, which
  grants access to a throwaway container on `localhost` and nothing else.
- SQL is parameterised (LINQ). No string-built SQL, ever.
- CORS allows only `http://localhost:5173` in Development. Never `AllowAnyOrigin()`.
- Agent-generated code is reviewed by a human before merge. In agent mode Gemini Code Assist does not show
  source citations, so the reviewer owns licence and attribution checks.
