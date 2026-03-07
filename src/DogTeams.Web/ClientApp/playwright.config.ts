import { defineConfig, devices } from '@playwright/test';

// Detect the actual frontend URL from Aspire environment variables or default
const getBaseURL = (): string => {
  // Priority 1: Explicit VITE_BASE_URL
  if (process.env.VITE_BASE_URL) {
    return process.env.VITE_BASE_URL;
  }
  
  // Priority 2: Aspire service discovery (http endpoint)
  if (process.env.services__web__http__0) {
    return process.env.services__web__http__0;
  }
  
  // Priority 3: Aspire service discovery (https endpoint)
  if (process.env.services__web__https__0) {
    return process.env.services__web__https__0;
  }
  
  // Fallback to default port
  return 'http://localhost:5173';
};

export default defineConfig({
  testDir: './tests',
  testMatch: '**/*.spec.ts',
  fullyParallel: false,
  forbidOnly: !!process.env.CI,
  retries: process.env.CI ? 2 : 0,
  workers: process.env.CI ? 1 : 1,
  reporter: 'html',
  use: {
    baseURL: getBaseURL(),
    trace: 'on-first-retry',
  },

  projects: [
    {
      name: 'chromium',
      use: { ...devices['Desktop Chrome'] },
    },
  ],

  webServer: undefined, // Backend must be running separately
});
