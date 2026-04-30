import { type Page, type Locator } from '@playwright/test';

export class AboutPage {
    readonly page: Page;
    readonly title: Locator;
    readonly subtitle: Locator;
    readonly description: Locator;
    readonly aboutContainer: Locator;

    constructor(page: Page) {
        this.page = page;
        this.title = page.getByRole('heading', { name: /About Dad-A-Base/i });
        this.subtitle = page.locator('.about-subtitle');
        this.description = page.locator('.about-card p');
        this.aboutContainer = page.locator('.about-container');
    }

    async goto(): Promise<void> {
        await this.page.goto('/About');
    }
}
