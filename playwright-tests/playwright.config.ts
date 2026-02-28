import { defineConfig, devices } from '@playwright/test';

export default defineConfig({
  testDir: './tests',
  timeout: 30_000,
  fullyParallel: true,
  reporter: [['html', { open: 'never' }], ['list']],
  use: {
    baseURL: 'http://localhost:5000',
    trace: 'on-first-retry',
  },
  projects: [
    { name: 'chromium', use: { ...devices['Desktop Chrome'] } },
  ],
  // Start the Management Server before running tests
  webServer: {
    command: 'dotnet run --project ../GameCafe.ManagementServer',
    url: 'http://localhost:5000',
    reuseExistingServer: !process.env.CI,
    timeout: 240_000,
    env: {
      ASPNETCORE_URLS: 'http://localhost:5000',
      ASPNETCORE_ENVIRONMENT: 'Development',
    },
  },
});
