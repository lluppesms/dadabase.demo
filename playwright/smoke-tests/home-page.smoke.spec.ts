import { test, expect } from '../fixtures/test-fixtures';

test.describe('Home Page Smoke Tests', () => {

    test.beforeEach(async ({ homePage }) => {
        await homePage.goto();
    });

    test('page loads with correct title', async ({ page }) => {
        await expect(page).toHaveTitle(/Dad/i);
    });

    test('heading is visible', async ({ homePage }) => {
        await expect(homePage.heading).toBeVisible();
    });

    test('joke card is present', async ({ homePage }) => {
        await expect(homePage.jokeCard).toBeVisible();
    });

    test('joke category element exists', async ({ homePage }) => {
        await expect(homePage.jokeCategory).toBeAttached();
    });

    test('joke text element exists', async ({ homePage }) => {
        await expect(homePage.jokeText).toBeAttached();
    });

    test('"Tell me another one!" button is visible', async ({ homePage }) => {
        await expect(homePage.tellMeAnotherButton).toBeVisible();
    });

    test('clicking "Tell me another one!" loads a new joke', async ({ homePage, page }) => {
        // Wait for initial joke to load
        await expect(homePage.jokeText).not.toBeEmpty({ timeout: 10000 });
        const firstJoke = await homePage.getJokeText();

        // Click and wait for new joke — retry a few times since random could return same joke
        let jokeChanged = false;
        for (let i = 0; i < 5; i++) {
            await homePage.clickTellMeAnother();
            await page.waitForTimeout(2000);
            const newJoke = await homePage.getJokeText();
            if (newJoke !== firstJoke && newJoke.length > 0) {
                jokeChanged = true;
                break;
            }
        }
        expect(jokeChanged).toBeTruthy();
    });

    test('joke image section is present', async ({ homePage }) => {
        await expect(homePage.jokeImageSection).toBeAttached();
    });
});
