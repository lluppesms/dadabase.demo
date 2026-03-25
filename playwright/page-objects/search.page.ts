import { type Page, type Locator } from '@playwright/test';

export class SearchPage {
    readonly page: Page;
    readonly heading: Locator;
    readonly searchInput: Locator;
    readonly categoryDropdown: Locator;
    readonly searchButton: Locator;
    readonly resultsList: Locator;
    readonly resultItems: Locator;

    constructor(page: Page) {
        this.page = page;
        this.heading = page.getByRole('heading', { name: /Search the Dad-A-Base/i });
        this.searchInput = page.getByRole('textbox', { name: 'Search For' });
        this.categoryDropdown = page.locator('.mud-select input.mud-select-input');
        this.searchButton = page.getByRole('button', { name: 'Search' });
        this.resultsList = page.locator('#jokeList');
        this.resultItems = page.locator('#jokeList li');
    }

    async goto(): Promise<void> {
        await this.page.goto('/Search');
        // Wait for Blazor/SignalR to hydrate
        await this.page.waitForSelector('input[type="text"]', { state: 'visible' });
        await this.page.waitForTimeout(1000);
    }

    async searchFor(term: string): Promise<void> {
        await this.searchInput.click();
        await this.searchInput.fill(term);
        await this.searchButton.click();
        // Wait for results to render
        await this.page.waitForTimeout(2000);
    }

    async getCategoryValue(): Promise<string> {
        return (await this.categoryDropdown.inputValue()) ?? '';
    }

    async getResultCount(): Promise<number> {
        return await this.resultItems.count();
    }

    async openCategoryDropdown(): Promise<void> {
        // MudBlazor renders a hidden input; click the visible select container instead
        await this.page.locator('.mud-select .mud-input-control').click();
    }
}
