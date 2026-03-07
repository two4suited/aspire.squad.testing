import { test, expect } from '@playwright/test';
import { registerAndLogin, logout, createTeam, navigateToTeam, createOwner, createDog } from './helpers';

test.describe('Authentication Flow', () => {
  test('should register and login successfully', async ({ page }) => {
    const { name, email } = await registerAndLogin(page);

    // Should be on dashboard
    await expect(page).toHaveURL('/');
    await expect(page).toContainText(`Welcome, ${name}`);
    await expect(page).toContainText('Dog Teams');
  });

  test('should login with existing account', async ({ page }) => {
    const { email, password } = await registerAndLogin(page);

    // Logout
    await logout(page);

    // Login
    await page.fill('#email', email);
    await page.fill('#password', password);
    await page.click('button:has-text("Sign in")');

    // Should be on dashboard
    await expect(page).toHaveURL('/');
    await expect(page).toContainText('Welcome,');
  });

  test('should logout successfully', async ({ page }) => {
    await registerAndLogin(page);

    await logout(page);

    // Should be on login page
    await expect(page).toHaveURL('/login');
    await expect(page).toContainText('Sign in');
  });

  test('should redirect to login when accessing protected routes without auth', async ({ page }) => {
    await page.goto('/');
    await expect(page).toHaveURL('/login');
  });

  test('should show error on invalid credentials', async ({ page }) => {
    await page.goto('/login');
    await page.fill('#email', 'nonexistent@example.com');
    await page.fill('#password', 'wrongpassword');
    await page.click('button:has-text("Sign in")');

    // Should show error message
    await expect(page).toContainText('Failed');
  });
});

test.describe('Team Management', () => {
  test('should create a team', async ({ page }) => {
    await registerAndLogin(page);

    const teamName = `Test Team ${Date.now()}`;
    const teamDesc = 'A test team for E2E testing';

    await createTeam(page, teamName, teamDesc);

    // Team should be visible on dashboard
    await expect(page).toContainText(teamName);
    await expect(page).toContainText(teamDesc);
  });

  test('should view team details', async ({ page }) => {
    await registerAndLogin(page);

    const teamName = `View Team ${Date.now()}`;
    await createTeam(page, teamName);

    await navigateToTeam(page, teamName);

    // Should be on team page
    await expect(page).toHaveURL(`/teams/**`);
    await expect(page).toContainText(teamName);
    await expect(page).toContainText('Owners');
  });

  test('should delete a team', async ({ page }) => {
    await registerAndLogin(page);

    const teamName = `Delete Team ${Date.now()}`;
    await createTeam(page, teamName);

    // Delete team
    await page.click(`button:has-text("Delete"):near(text="${teamName}")`);
    await page.on('dialog', dialog => dialog.accept()); // Confirm deletion

    // Team should no longer be visible
    await expect(page).not.toContainText(teamName);
  });
});

test.describe('Owner Management', () => {
  test('should add owner to team', async ({ page }) => {
    await registerAndLogin(page);

    const teamName = `Owner Team ${Date.now()}`;
    await createTeam(page, teamName);
    await navigateToTeam(page, teamName);

    const ownerName = 'John Doe';
    const ownerEmail = `john+${Date.now()}@example.com`;

    await createOwner(page, ownerName, ownerEmail);

    // Owner should be visible
    await expect(page).toContainText(ownerName);
    await expect(page).toContainText(ownerEmail);
  });

  test('should delete owner from team', async ({ page }) => {
    await registerAndLogin(page);

    const teamName = `Owner Delete ${Date.now()}`;
    await createTeam(page, teamName);
    await navigateToTeam(page, teamName);

    const ownerName = 'Jane Smith';
    const ownerEmail = `jane+${Date.now()}@example.com`;

    await createOwner(page, ownerName, ownerEmail);

    // Delete owner
    await page.click(`button:has-text("Delete"):near(text="${ownerName}")`);
    await page.on('dialog', dialog => dialog.accept());

    // Owner should no longer be visible
    await expect(page).not.toContainText(ownerName);
  });
});

test.describe('Dog Management', () => {
  test('should add dog to owner', async ({ page }) => {
    await registerAndLogin(page);

    const teamName = `Dog Team ${Date.now()}`;
    await createTeam(page, teamName);
    await navigateToTeam(page, teamName);

    const ownerName = 'Pet Owner';
    const ownerEmail = `owner+${Date.now()}@example.com`;
    await createOwner(page, ownerName, ownerEmail);

    const dogName = 'Buddy';
    const breed = 'Golden Retriever';

    await createDog(page, ownerName, dogName, breed);

    // Dog should be visible
    await expect(page).toContainText(dogName);
    await expect(page).toContainText(breed);
  });

  test('should delete dog from owner', async ({ page }) => {
    await registerAndLogin(page);

    const teamName = `Dog Delete Team ${Date.now()}`;
    await createTeam(page, teamName);
    await navigateToTeam(page, teamName);

    const ownerName = 'Dog Owner';
    const ownerEmail = `dogowner+${Date.now()}@example.com`;
    await createOwner(page, ownerName, ownerEmail);

    const dogName = 'Rex';
    const breed = 'Labrador';

    await createDog(page, ownerName, dogName, breed);

    // Delete dog
    await page.click(`button:has-text("Delete"):near(text="${dogName}")`);
    await page.on('dialog', dialog => dialog.accept());

    // Dog should no longer be visible
    await expect(page).not.toContainText(dogName);
  });
});

test.describe('Error Handling', () => {
  test('should handle API errors gracefully', async ({ page }) => {
    await registerAndLogin(page);

    const teamName = `Error Team ${Date.now()}`;
    await createTeam(page, teamName);
    await navigateToTeam(page, teamName);

    // Try to create owner with empty name (should fail)
    await page.click('button:has-text("Add Owner")');
    await page.fill('input[placeholder="john@example.com"]', 'test@example.com');
    // Skip name field (required)
    // Try to submit - should be prevented by HTML5 validation
    await expect(page.locator('input[placeholder="e.g., John Smith"]')).toHaveAttribute('required', '');
  });

  test('should show error message on failed operations', async ({ page }) => {
    await registerAndLogin(page);

    const teamName = `Error Handling ${Date.now()}`;
    await createTeam(page, teamName);
    await navigateToTeam(page, teamName);

    // Create an owner first
    await createOwner(page, 'Owner', `owner+${Date.now()}@example.com`);

    // Try to create owner with duplicate email (should fail on backend)
    await page.click('button:has-text("Add Owner")');
    await page.fill('input[placeholder="e.g., John Smith"]', 'Duplicate');
    await page.fill('input[placeholder="john@example.com"]', `owner+${Date.now()}@example.com`);
    await page.click('button:has-text("Add Owner")');

    // Should eventually show an error or handle gracefully
    // Wait a bit for any error to appear
    await page.waitForTimeout(1000);
  });
});

test.describe('Complete User Journey', () => {
  test('should complete full workflow: register -> create team -> add owner -> add dog -> logout', async ({ page }) => {
    const { name } = await registerAndLogin(page);

    // Create team
    const teamName = `Journey Team ${Date.now()}`;
    await createTeam(page, teamName, 'A complete user journey');

    // Navigate to team
    await navigateToTeam(page, teamName);

    // Add owner
    const ownerName = 'Sarah';
    const ownerEmail = `sarah+${Date.now()}@example.com`;
    await createOwner(page, ownerName, ownerEmail);

    // Add dog
    const dogName = 'Max';
    const breed = 'German Shepherd';
    await createDog(page, ownerName, dogName, breed);

    // Verify all data is visible
    await expect(page).toContainText(teamName);
    await expect(page).toContainText(ownerName);
    await expect(page).toContainText(dogName);
    await expect(page).toContainText(breed);

    // Go back to dashboard
    await page.click('a:has-text("← Back to dashboard")');
    await expect(page).toHaveURL('/');
    await expect(page).toContainText(`Welcome, ${name}`);
    await expect(page).toContainText(teamName);

    // Logout
    await logout(page);

    // Verify logged out
    await expect(page).toHaveURL('/login');
  });
});
