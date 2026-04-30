import { test as base } from '@playwright/test';
import { HomePage } from '../page-objects/home.page';
import { SearchPage } from '../page-objects/search.page';
import { AboutPage } from '../page-objects/about.page';
import { LayoutComponent } from '../page-objects/layout.component';

type SmokeTestFixtures = {
    homePage: HomePage;
    searchPage: SearchPage;
    aboutPage: AboutPage;
    layout: LayoutComponent;
};

export const test = base.extend<SmokeTestFixtures>({
    homePage: async ({ page }, use) => {
        await use(new HomePage(page));
    },
    searchPage: async ({ page }, use) => {
        await use(new SearchPage(page));
    },
    aboutPage: async ({ page }, use) => {
        await use(new AboutPage(page));
    },
    layout: async ({ page }, use) => {
        await use(new LayoutComponent(page));
    },
});

export { expect } from '@playwright/test';
