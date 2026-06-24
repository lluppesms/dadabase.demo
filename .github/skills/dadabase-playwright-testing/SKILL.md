---
name: dadabase-playwright-testing
description: 'Explore and test the anonymous Dadabase website experience in this repository with Playwright. Use whenever the user wants to open the Dadabase site, run a quick homepage smoke test, start the local web app when no URL is provided, browse categories, search jokes, capture selectors or screenshots, smoke-test the public UI, or generate Playwright coverage for anonymous joke discovery. This includes homepage smoke tests, quick Playwright checks, category filtering, and joke search.'
compatibility: 'Requires this dadabase.demo.gh repository, a local .NET SDK for local startup, and either Playwright-capable browser automation or the ability to generate Playwright tests.'
---

# Dadabase Anonymous Playwright Testing

Use this skill to exercise the public Dadabase browsing experience in `dadabase.demo.gh` and turn what you find into reusable Playwright coverage. This version is intentionally limited to anonymous flows so it can be used safely without login details.

## Scope

### In scope

- Open the Dadabase site and confirm the anonymous landing experience
- Start the local app when the user does not provide a URL
- Discover how categories are exposed to public users
- Exercise category browsing or filtering
- Exercise joke searching from the public site
- Document stable locators, navigation paths, screenshots, and expected outcomes
- Generate or update Playwright tests for anonymous flows

### Out of scope

- Admin login
- Add joke, edit joke, delete joke
- Any workflow that requires credentials or privileged access

If the user asks for authenticated flows, explain that this skill deliberately stops short of those actions until safe test credentials or a dedicated test account are available.

## What to ask first

1. If the user supplied a URL, use it.
2. If the user did not supply a URL, start or reuse the local web app before testing.
3. Clarify whether the user wants exploration notes, executable Playwright tests, or both.
4. Confirm whether the target is local, staging, or production-like so you do not make assumptions about data freshness.

## Default workflow

### 1. Resolve the base URL

If the user provides a URL, use it directly.

If the user does not provide a URL:

1. Probe `http://localhost:5178` first, then `https://localhost:7273`.
2. If one already responds, reuse it.
3. If neither responds, start the local web app in a background or detached PowerShell session so it can keep running while tests execute.
4. Poll the local URLs until one responds, then use that URL for the rest of the run.
5. If HTTPS is used for browser automation, allow for local development certificate handling such as `ignoreHTTPSErrors` when needed.

The first local run can spend extra time on restore or build work, so allow a few minutes before deciding startup failed.

Use this exact local startup command:

```powershell
dotnet run --project .\src\web\Website\DadABase.Web.csproj --launch-profile https
```

### 2. Open the site

1. Navigate to the resolved base URL.
2. Wait for a stable ready signal such as a heading, navigation landmark, or the Search link.
3. Record the initial URL, page title, and the anonymous entry points visible from the home page.
4. Capture a screenshot when it helps document the flow or a failure.

A good homepage smoke assertion is that the page title contains `Dad`.

### 3. Map the public navigation

Identify the controls anonymous users can actually reach. In this application, the important public flow already exercised by existing Playwright tests is:

1. Open `/`
2. Click the `Search` link
3. Confirm the `Search the Dad-A-Base` heading on `/search`

Prefer resilient selectors in this order:

1. `data-testid`
2. role plus accessible name
3. label or placeholder text
4. stable URL assertions
5. CSS selectors only as a last resort

Known stable public selectors in this repo include:

- `getByRole('link', { name: 'Search' })`
- `getByRole('heading', { name: 'Search the Dad-A-Base' })`
- `getByRole('textbox', { name: 'Search For' })`
- `getByRole('combobox', { name: 'Category' })`
- `getByRole('button', { name: 'Search' })`
- `#jokeList` for the search results list

### 4. Exercise category discovery

Work with the UI the app actually exposes. In this repository, category discovery is expected to happen from the `/search` page through the `Category` select control rather than a dedicated public category page.

For this flow:

1. Go to `/search` or reach it through the Search link.
2. Confirm the `Category` combobox is visible.
3. Record the default category state, including whether it starts on `All Categories`.
4. Interact with at least two concrete category options if data is available.
5. Record the control used, the filtering behavior, and what changes in the visible results after each selection.
6. If the environment does not expose meaningful category-specific results, say so clearly instead of inventing assertions.

### 5. Exercise joke search

1. Find the `Search For` textbox.
2. Use the Search page rather than inventing a different route.
3. Try one query likely to match seeded jokes, such as `chicken`, because the existing UI tests already use that term.
4. Click the `Search` button and wait for the results list.
5. Record what a stable assertion looks like for both a successful result and an empty-result state.

Good search assertions in this repo usually focus on:

- The search page heading being visible
- The results list `#jokeList` appearing
- The result count being greater than zero for a likely-hit term
- A visible empty state or zero items for a miss, if the environment supports it

### 6. Produce reusable testing output

When documenting or generating tests, make the result directly reusable. Include:

- The base URL used
- Whether the local app was started or reused
- The navigation path for each anonymous flow
- The locator strategy for each important control
- The expected result after each step
- Any screenshots or notes that explain fragile behavior
- Gaps or ambiguities that need a product or engineering decision

### 7. Call out deferred admin coverage

End by explicitly noting that the admin add-joke flow was not attempted because this skill is anonymous-only by design.

## Output structure

Use this structure unless the user asked for something else:

```markdown
# Dadabase anonymous test notes
## Base URL
## Local startup
## Anonymous entry points
## Category discovery flow
## Joke search flow
## Stable locators and assertions
## Risks or unknowns
## Deferred authenticated flows
```

If the user asked for Playwright tests, add:

```markdown
## Proposed Playwright coverage
- Anonymous homepage loads
- Search page opens from the home page
- Category selection changes or filters the visible search results
- Joke search returns expected results or an empty state
```

## Guidance for Playwright tests

When writing tests from this exploration:

1. Keep tests anonymous-only.
2. Prefer a few focused tests over one long end-to-end script.
3. Assert visible user outcomes, not just DOM presence.
4. Reuse the repo's known public selectors where they remain accurate.
5. Include one empty-state assertion if the UI supports search or filtering.
6. Do not hardcode joke text unless the environment guarantees seeded data.
7. When testing locally without a supplied URL, prefer `http://localhost:5178` unless the app redirects elsewhere.

Good anonymous coverage for this repo usually includes:

- A homepage smoke test
- A Search-link navigation test
- A category-filter discovery test on `/search`
- A joke-search happy-path test
- A joke-search empty-state test, if supported

## Troubleshooting

### No URL was provided

Check the local URLs first. If neither responds, start `dotnet run --project .\src\web\Website\DadABase.Web.csproj --launch-profile https` in a background or detached PowerShell session and wait for `http://localhost:5178` or `https://localhost:7273` to become reachable. If startup still fails, surface the failure clearly and include the expected project path `src\web\Website\DadABase.Web.csproj`. Remember that the first restore or build can take longer than a warm start.

### The site opens but the flow is unclear

Slow down and inspect the anonymous navigation before writing tests. The important thing is to model the real public journey, not the journey you expected.

### Search controls are not obvious

In this repo, the search UI is expected on `/search` with a `Search For` textbox, a `Category` combobox, a `Search` button, and the `#jokeList` results container. If the running environment differs, document the difference instead of forcing the expected layout.

### Results vary between environments

Favor assertions about navigation, visibility, state changes, and counts over exact joke text unless the environment is seeded and stable.

### The user asks for admin add-joke coverage

Do not improvise login or protected steps. State that authenticated coverage is a future extension of this skill and requires safe credentials or a test account.

## Reference

For the reusable checklist, known local URLs, startup command, and concrete public selectors, read `references/anonymous-flow-checklist.md`.
