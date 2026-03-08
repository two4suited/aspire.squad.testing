import { Page } from '@playwright/test';

const TEST_PASSWORD = 'TestPassword123!';
const TEST_NAME = 'Test User';

export async function registerAndLogin(page: Page) {
  // Generate unique email for each test to avoid conflicts
  const TEST_EMAIL = `test+${Date.now()}@example.com`;
  
  // Go to register page
  await page.goto('/register');
  await page.fill('#name', TEST_NAME);
  await page.fill('#email', TEST_EMAIL);
  await page.fill('#password', TEST_PASSWORD);
  await page.click('button:has-text("Create account")');
  await page.waitForURL('/');

  return { email: TEST_EMAIL, password: TEST_PASSWORD, name: TEST_NAME };
}

export async function logout(page: Page) {
  await page.click('button:has-text("Sign out")');
  await page.waitForURL('/login');
}

export async function createTeam(page: Page, name: string, description?: string) {
  await page.click('button:has-text("Add Team")');
  await page.fill('input[placeholder="e.g., City Dog Club"]', name);
  if (description) {
    await page.fill('textarea[placeholder="Team description (optional)"]', description);
  }
  await page.click('button:has-text("Create Team")');
  // Wait for the team to appear on the dashboard
  await page.locator(`body:has-text("${name}")`).waitFor({ timeout: 30000 });
}

export async function navigateToTeam(page: Page, teamName: string) {
  await page.click(`a:has-text("${teamName}")`);
  // Wait for URL to change to /teams/:id
  await page.waitForURL(/\/teams\/.+/);
  // Check if we got an error page or loading page
  const pageText = await page.locator('body').textContent();
  console.log('[Test Helper] Page content after navigation:', pageText?.substring(0, 200));
  // Wait for the page to fully load the team name (h1 with the team name)
  await page.locator(`h1:has-text("${teamName}")`).waitFor({ timeout: 30000 });
}

export async function createOwner(page: Page, name: string, email: string) {
  await page.click('button:has-text("Add Owner")');
  await page.fill('input[placeholder="e.g., John Smith"]', name);
  await page.fill('input[placeholder="john@example.com"]', email);
  await page.click('button:has-text("Add Owner")');
  // Wait for the owner to appear in the owners list
  await page.locator(`h3:has-text("${name}")`).waitFor({ timeout: 30000 });
}

export async function createDog(page: Page, ownerName: string, dogName: string, breed: string) {
  await page.click('button:has-text("Add Dog")');
  await page.selectOption('select', { label: ownerName });
  await page.fill('input[placeholder="e.g., Max"]', dogName);
  await page.fill('input[placeholder="e.g., Golden Retriever"]', breed);
  await page.click('button:has-text("Add Dog")');
  // Wait for the dog to appear in the dogs list
  await page.locator(`text=${dogName}`).waitFor({ timeout: 30000 });
}
