import { type Page, type Locator } from '@playwright/test';

export class LayoutComponent {
    readonly page: Page;
    readonly appBar: Locator;
    readonly logo: Locator;
    readonly brandTitle: Locator;
    readonly homeTab: Locator;
    readonly searchTab: Locator;
    readonly aboutTab: Locator;
    readonly loginLink: Locator;
    readonly themeDropdownTrigger: Locator;
    readonly themeMenu: Locator;
    readonly themeOptionLight: Locator;
    readonly themeOptionDark: Locator;
    readonly themeOption90s: Locator;
    readonly themeOptionSystem: Locator;
    readonly footer: Locator;
    readonly footerPrivacyLink: Locator;
    readonly footerLicenseLink: Locator;
    readonly footerBuildNumber: Locator;

    constructor(page: Page) {
        this.page = page;
        this.appBar = page.locator('header.mud-appbar');
        this.logo = page.locator('header img[alt="Logo"]');
        this.brandTitle = page.locator('#headerPageTitle');
        this.homeTab = page.locator('a.nav-tab-top', { has: page.locator('text=Home') });
        this.searchTab = page.locator('a.nav-tab-top', { has: page.locator('text=Search') });
        this.aboutTab = page.locator('a.nav-tab-top', { has: page.locator('text=About') });
        this.loginLink = page.locator('.navbar-login a');
        this.themeDropdownTrigger = page.locator('#themeDropdown');
        this.themeMenu = page.locator('#themeDropdown + ul.dropdown-menu');
        this.themeOptionLight = this.themeMenu.locator('text=Light');
        this.themeOptionDark = this.themeMenu.locator('text=Dark');
        this.themeOption90s = this.themeMenu.locator("text=90's Theme");
        this.themeOptionSystem = this.themeMenu.locator('text=System Default');
        this.footer = page.locator('.LayoutFooterStyle');
        this.footerPrivacyLink = this.footer.locator('a[href*="Privacy"]');
        this.footerLicenseLink = this.footer.locator('a[href*="License"]');
        this.footerBuildNumber = this.footer.locator('.build-number');
    }

    async getActiveTab(): Promise<Locator> {
        return this.page.locator('a.nav-tab-top.active');
    }

    async openThemeMenu(): Promise<void> {
        // Bootstrap dropdown requires clicking the toggle element
        await this.themeDropdownTrigger.click();
        // Wait for Bootstrap to add the 'show' class
        await this.page.waitForSelector('.dropdown-menu.show', { state: 'visible', timeout: 5000 });
    }
}
