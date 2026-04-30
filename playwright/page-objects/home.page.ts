import { type Page, type Locator } from '@playwright/test';

export class HomePage {
    readonly page: Page;
    readonly heading: Locator;
    readonly jokeCard: Locator;
    readonly jokeCategory: Locator;
    readonly jokeText: Locator;
    readonly tellMeAnotherButton: Locator;
    readonly jokeImageSection: Locator;

    constructor(page: Page) {
        this.page = page;
        this.heading = page.getByRole('heading', { name: /Tell me a Joke/i });
        this.jokeCard = page.locator('.JokeCard');
        this.jokeCategory = page.locator('.JokeCategory');
        this.jokeText = page.locator('.JokeText');
        this.tellMeAnotherButton = page.getByRole('button', { name: /Tell me another one/i });
        this.jokeImageSection = page.locator('.joke-image-section');
    }

    async goto(): Promise<void> {
        await this.page.goto('/');
    }

    async getJokeText(): Promise<string> {
        return (await this.jokeText.textContent()) ?? '';
    }

    async clickTellMeAnother(): Promise<void> {
        await this.tellMeAnotherButton.click();
    }
}
