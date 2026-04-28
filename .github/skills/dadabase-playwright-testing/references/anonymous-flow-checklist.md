# Anonymous Flow Checklist

Use this checklist when exploring or testing the public Dadabase experience in `dadabase.demo.gh`.

## Local startup defaults

If the user did not provide a URL:

1. Probe `http://localhost:5178`.
2. Fall back to `https://localhost:7273` if HTTP is unavailable.
3. If neither responds, start the local web app in a background or detached session with the command below.
4. Note whether the app was already running or whether you started it.

The first local run may take longer because `dotnet run` can restore and build before the site becomes reachable.

The local web project lives at:

`src\web\Website\DadABase.Web.csproj`

The repo quick start also documents:

```powershell
dotnet run --project .\src\web\Website\DadABase.Web.csproj --launch-profile https
```

## Anonymous exploration checklist

### Site entry

- Load the base URL.
- Note the page title and first stable landmark.
- Record the controls visible without logging in.

### Search page navigation

- From the home page, click the `Search` link when present.
- Confirm the heading `Search the Dad-A-Base`.
- Record whether the route resolves to `/search`.

### Category discovery

- Use the `Category` combobox on the Search page.
- Record the default state, including whether `All Categories` appears.
- Exercise at least two category selections if the environment supports them.
- Record the visible result after each action.

### Joke search

- Use the `Search For` textbox.
- Try a likely-hit term such as `chicken`.
- Click the `Search` button.
- Record whether `#jokeList` appears and whether results are returned.
- Capture the empty-state behavior when practical.

## Known public selectors

- Search link: `getByRole('link', { name: 'Search' })`
- Search heading: `getByRole('heading', { name: 'Search the Dad-A-Base' })`
- Search textbox: `getByRole('textbox', { name: 'Search For' })`
- Category selector: `getByRole('combobox', { name: 'Category' })`
- Search button: `getByRole('button', { name: 'Search' })`
- Results list: `#jokeList`

## Recommended evidence to collect

- Final URL after each flow
- Whether local startup was reused or initiated by the skill
- A screenshot of the landing page and one meaningful search or filter state
- Stable locators with a short reason each one is reliable
- Any messages, counts, or headings that make good assertions

## Playwright test planning hints

Translate exploration into small tests:

1. Homepage loads and title contains `Dad`.
2. Search page opens from the home page.
3. Category selection changes visible results or route state.
4. Joke search produces a visible result container or a clear empty state.

Keep the tests flexible when data is unstable. Prefer assertions about interaction outcomes over exact content strings unless the environment is known to be seeded.

## Deferred future extension

Authenticated admin coverage, including add-joke flows, belongs in a follow-up version of this skill once safe test credentials and expected admin behavior are available.
