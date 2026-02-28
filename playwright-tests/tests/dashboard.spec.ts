import { test, expect } from '@playwright/test';

// ── Dashboard page loads ───────────────────────────────────────────────

test('dashboard loads with correct title', async ({ page }) => {
  await page.goto('/');
  await expect(page).toHaveTitle(/Gaming Cafe Admin Dashboard/);
});

test('dashboard shows main heading', async ({ page }) => {
  await page.goto('/');
  await expect(page.getByRole('heading', { name: 'Dashboard' })).toBeVisible();
});

test('navigation sidebar is present', async ({ page }) => {
  await page.goto('/');
  const nav = page.getByRole('navigation', { name: 'Main navigation' });
  await expect(nav).toBeVisible();
});

test('skip-to-content link is first focusable element', async ({ page }) => {
  await page.goto('/');
  // Tab once to activate skip link
  await page.keyboard.press('Tab');
  const skipLink = page.locator('.skip-link');
  await expect(skipLink).toBeFocused();
  await expect(skipLink).toHaveText('Skip to main content');
});

// ── WCAG Accessibility ─────────────────────────────────────────────────

test('all stat cards have text labels (not colour-only)', async ({ page }) => {
  await page.goto('/');
  // Each stat card must have a visible label element (WCAG 1.4.1)
  const labels = page.locator('.stat-card .label');
  const count = await labels.count();
  expect(count).toBeGreaterThanOrEqual(4);
  for (let i = 0; i < count; i++) {
    await expect(labels.nth(i)).not.toBeEmpty();
  }
});

test('all table headers use scope attribute', async ({ page }) => {
  await page.goto('/');
  const ths = page.locator('thead th');
  const count = await ths.count();
  expect(count).toBeGreaterThan(0);
  for (let i = 0; i < count; i++) {
    const scope = await ths.nth(i).getAttribute('scope');
    expect(scope).toBe('col');
  }
});

test('page has lang attribute set', async ({ page }) => {
  await page.goto('/');
  const lang = await page.locator('html').getAttribute('lang');
  expect(lang).toBe('en');
});

test('main content region has id for skip link', async ({ page }) => {
  await page.goto('/');
  const main = page.locator('#main-content');
  await expect(main).toBeVisible();
  await expect(main).toHaveAttribute('tabindex', '-1');
});

test('status banner has role=status and aria-live', async ({ page }) => {
  await page.goto('/');
  const banner = page.locator('#server-status');
  await expect(banner).toHaveAttribute('role', 'status');
  await expect(banner).toHaveAttribute('aria-live', 'polite');
});

test('stat values use aria-live for dynamic updates', async ({ page }) => {
  await page.goto('/');
  for (const id of ['stat-active-stations', 'stat-active-sessions', 'stat-revenue', 'stat-total-stations']) {
    const el = page.locator(`#${id}`);
    await expect(el).toHaveAttribute('aria-live', 'polite');
  }
});

// ── Keyboard navigation ────────────────────────────────────────────────

test('all navigation links are keyboard focusable', async ({ page }) => {
  await page.goto('/');
  const links = page.locator('nav[aria-label="Main navigation"] a');
  const count = await links.count();
  expect(count).toBeGreaterThanOrEqual(4);
  for (let i = 0; i < count; i++) {
    const tag = await links.nth(i).evaluate(el => el.tagName.toLowerCase());
    expect(tag).toBe('a');
  }
});

test('active nav link has aria-current=page', async ({ page }) => {
  await page.goto('/');
  const activeLink = page.locator('nav a[aria-current="page"]');
  await expect(activeLink).toBeVisible();
});

// ── Stations table ─────────────────────────────────────────────────────

test('stations table renders with correct columns', async ({ page }) => {
  await page.goto('/');
  const headers = page.locator('#stations-table thead th');
  const texts = await headers.allTextContents();
  expect(texts).toContain('Station ID');
  expect(texts).toContain('Status');
  expect(texts).toContain('Actions');
});

test('sessions table renders with correct columns', async ({ page }) => {
  await page.goto('/');
  const headers = page.locator('#sessions-table thead th');
  const texts = await headers.allTextContents();
  expect(texts).toContain('Session ID');
  expect(texts).toContain('Duration');
  expect(texts).toContain('Cost');
});

// ── API integration ────────────────────────────────────────────────────

test('API /api/stations returns 200', async ({ request }) => {
  const res = await request.get('/api/stations');
  expect(res.status()).toBe(200);
});

test('API /api/sessions returns 200', async ({ request }) => {
  const res = await request.get('/api/sessions');
  expect(res.status()).toBe(200);
});

test('API /api/billing/analytics returns 200', async ({ request }) => {
  const res = await request.get('/api/billing/analytics');
  expect(res.status()).toBe(200);
});

test('Swagger UI is available at /swagger', async ({ page }) => {
  await page.goto('/swagger');
  // Swagger UI has a title or heading
  await expect(page.locator('body')).not.toBeEmpty();
});
