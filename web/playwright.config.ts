import { defineConfig, devices } from '@playwright/test'

// E2E_BASE_URL lets the same suite run against the Docker stack (Day 3): E2E_BASE_URL=http://localhost:8080
const externalBaseUrl = process.env.E2E_BASE_URL

export default defineConfig({
  testDir: './e2e',
  fullyParallel: false,
  workers: 1,
  retries: process.env.CI ? 1 : 0,
  reporter: [['list'], ['html', { open: 'never' }]],
  use: {
    baseURL: externalBaseUrl ?? 'http://localhost:5173',
    trace: 'retain-on-failure',
  },
  projects: [{ name: 'chromium', use: { ...devices['Desktop Chrome'] } }],
  // Starts the API (needs `docker compose up -d db`) and Vite unless they are already running.
  webServer: externalBaseUrl
    ? undefined
    : [
        {
          command: 'dotnet run --project ../src/BookStore.Api',
          url: 'http://localhost:5080/health',
          reuseExistingServer: true,
          timeout: 180_000,
        },
        {
          command: 'npm run dev',
          url: 'http://localhost:5173',
          reuseExistingServer: true,
          timeout: 60_000,
        },
      ],
})
