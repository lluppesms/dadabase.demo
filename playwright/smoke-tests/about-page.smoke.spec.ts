import { test, expect } from '../fixtures/test-fixtures';

test.describe('About Page Smoke Tests', () => {

    test.beforeEach(async ({ aboutPage }) => {
        await aboutPage.goto();
    });

    test('page loads with correct title', async ({ page }) => {
        await expect(page).toHaveTitle(/About/i);
    });

    test('about heading is visible', async ({ aboutPage }) => {
        await expect(aboutPage.title).toBeVisible();
    });

    test('contains SQL Dad-A-Base subtitle', async ({ aboutPage }) => {
        await expect(aboutPage.subtitle).toContainText('Dad-A-Base');
    });

    test('description paragraph is visible', async ({ aboutPage }) => {
        await expect(aboutPage.description).toBeVisible();
        await expect(aboutPage.description).toContainText('dad jokes');
    });

    test('about container is present', async ({ aboutPage }) => {
        await expect(aboutPage.aboutContainer).toBeVisible();
    });
});
