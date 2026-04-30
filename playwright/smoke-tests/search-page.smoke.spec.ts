import { test, expect } from '../fixtures/test-fixtures';

test.describe('Search Page Smoke Tests', () => {

    test.beforeEach(async ({ searchPage }) => {
        await searchPage.goto();
    });

    test('page loads with correct heading', async ({ searchPage }) => {
        await expect(searchPage.heading).toBeVisible();
    });

    test('search text input is visible', async ({ searchPage }) => {
        await expect(searchPage.searchInput).toBeVisible();
    });

    test('category dropdown defaults to ALL', async ({ searchPage }) => {
        const value = await searchPage.getCategoryValue();
        expect(value).toBe('ALL');
    });

    test('search button is visible', async ({ searchPage }) => {
        await expect(searchPage.searchButton).toBeVisible();
    });

    test('searching for a term returns results', async ({ searchPage }) => {
        await searchPage.searchFor('chicken');
        const count = await searchPage.getResultCount();
        expect(count).toBeGreaterThan(0);
    });

    test('empty search returns results', async ({ searchPage }) => {
        await searchPage.searchButton.click();
        await searchPage.page.waitForTimeout(2000);
        const count = await searchPage.getResultCount();
        expect(count).toBeGreaterThan(0);
    });

    test('category dropdown can be opened', async ({ searchPage, page }) => {
        await searchPage.openCategoryDropdown();
        // MudBlazor renders list items inside the popover when opened
        const popoverContent = page.locator('.mud-popover-provider .mud-list-item, .mud-popover-provider .mud-list-item-text');
        await expect(popoverContent.first()).toBeVisible({ timeout: 5000 });
    });
});
