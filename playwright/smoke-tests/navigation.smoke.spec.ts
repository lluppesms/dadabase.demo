import { test, expect } from '../fixtures/test-fixtures';

test.describe('Navigation Smoke Tests', () => {

    test.beforeEach(async ({ page }) => {
        await page.goto('/');
    });

    test('app bar is visible', async ({ layout }) => {
        await expect(layout.appBar).toBeVisible();
    });

    test('logo image is present with alt text', async ({ layout }) => {
        await expect(layout.logo).toBeVisible();
        await expect(layout.logo).toHaveAttribute('alt', 'Logo');
    });

    test('brand title "The Dad-A-Base" is visible', async ({ layout }) => {
        await expect(layout.brandTitle).toBeVisible();
        await expect(layout.brandTitle).toContainText('The Dad-A-Base');
    });

    test('home tab is present and active on home page', async ({ layout }) => {
        await expect(layout.homeTab).toBeVisible();
        await expect(layout.homeTab).toHaveClass(/active/);
    });

    test('search tab navigates to /Search', async ({ layout, page }) => {
        await layout.searchTab.click();
        await expect(page).toHaveURL(/\/Search/i);
        await expect(layout.searchTab).toHaveClass(/active/);
    });

    test('about tab navigates to /About', async ({ layout, page }) => {
        await layout.aboutTab.click();
        await expect(page).toHaveURL(/\/About/i);
        await expect(layout.aboutTab).toHaveClass(/active/);
    });

    test('active tab has aria-current attribute', async ({ layout }) => {
        const activeTab = await layout.getActiveTab();
        await expect(activeTab).toHaveAttribute('aria-current', 'page');
    });

    test('login link is visible and points to sign-in', async ({ layout }) => {
        await expect(layout.loginLink).toBeVisible();
        await expect(layout.loginLink).toHaveAttribute('href', /MicrosoftIdentity\/Account\/SignIn/i);
    });

    test('footer contains copyright text', async ({ layout }) => {
        await expect(layout.footer).toContainText('Copyright');
        await expect(layout.footer).toContainText('Luppes Consulting');
    });

    test('footer privacy link points to luppes.com', async ({ layout }) => {
        await expect(layout.footerPrivacyLink).toHaveAttribute('href', /luppes\.com\/Privacy/);
    });

    test('footer license link points to luppes.com', async ({ layout }) => {
        await expect(layout.footerLicenseLink).toHaveAttribute('href', /luppes\.com\/License/);
    });

    test('footer displays build number', async ({ layout }) => {
        await expect(layout.footerBuildNumber).toBeVisible();
        const buildText = await layout.footerBuildNumber.textContent();
        // Build number format: YYYY.MM.DD.NN
        expect(buildText).toMatch(/\d{4}\.\d{2}\.\d{2}\.\d+/);
    });
});
