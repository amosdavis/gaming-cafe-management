import { defineConfig, devices } from '@playwright/test';

export default defineConfig({
  testDir: './tests',
  timeout: 30_000,
  fullyParallel: true,
  reporter: [['html', { open: 'never' }], ['list']],
  use: {
    baseURL: 'http://127.0.0.1:5000',
    trace: 'on-first-retry',
  },
  projects: [
    { name: 'chromium', use: { ...devices['Desktop Chrome'] } },
  ],
  // NOTE: No webServer config — CI workflow pre-starts the server.
  // For local runs: start the server first with:
  //   ASPNETCORE_URLS=http://127.0.0.1:5000 dotnet run --project ../GameCafe.ManagementServer
});
