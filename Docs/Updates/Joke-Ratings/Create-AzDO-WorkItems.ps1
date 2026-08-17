<#
.SYNOPSIS
    Creates (or repairs) the Feature / User Story / Task hierarchy for the Joke
    Rating System in Azure DevOps, based on Joke-Rating-Implementation-Plan.md
    and Joke-Rating-Implementation-Tasks.md.

.DESCRIPTION
    Uses the Azure CLI `azure-devops` extension to create one Feature, seven
    User Stories (one per implementation phase), and their child Tasks, wiring
    up parent/child relations as it goes. Each work item's Description and
    Acceptance Criteria are populated with enough detail (files touched,
    guidance, and Definition of Done) for a developer to pick up and run with.

    IMPORTANT — why descriptions are single-line HTML:
    `az` on Windows is a shim (az.cmd) executed through cmd.exe. Any argument
    containing a raw newline gets truncated by cmd.exe's argument parser (the
    text after the first newline is silently dropped, and multi-field
    `--fields "A=B"` arguments with embedded newlines can be dropped entirely).
    To avoid this, every Description/AcceptanceCriteria value below is authored
    as a single-line string using `<br/>` for line breaks instead of real
    newlines — safe for both cmd.exe and Azure DevOps' rich-text rendering.

    Modes:
    - Create (default): creates brand-new work items and links them.
    - Update: updates the *already created* work items (ids hard-coded below,
      matching the original 2026-08-17 creation run) with corrected
      Description/AcceptanceCriteria content. Does not touch relations.

    Re-running in Create mode will create duplicate work items — it is not
    idempotent. Update mode is safe to re-run.

.PARAMETER Organization
    Azure DevOps organization URL. Defaults to https://dev.azure.com/lyleluppes.

.PARAMETER Project
    Azure DevOps project name. Defaults to GitHubDevOps.

.PARAMETER Mode
    'Create' to create new work items, 'Update' to fix the existing ones.

.EXAMPLE
    ./Create-AzDO-WorkItems.ps1 -Mode Update
#>
[CmdletBinding()]
param(
    [string]$Organization = 'https://dev.azure.com/lyleluppes',
    [string]$Project = 'GitHubDevOps',
    [ValidateSet('Create', 'Update')]
    [string]$Mode = 'Create'
)

$ErrorActionPreference = 'Stop'

az devops configure --defaults "organization=$Organization" "project=$Project" | Out-Null

# Joins an array of HTML fragment lines into one single-line string (no raw
# newlines) so it survives cmd.exe argument parsing when passed to az.cmd.
function Join-Html {
    param([Parameter(Mandatory)] [string[]]$Lines)
    ($Lines -join '')
}

function New-WorkItem {
    param(
        [Parameter(Mandatory)] [string]$Title,
        [Parameter(Mandatory)] [string]$Type,
        [Parameter(Mandatory)] [string]$Description,
        [string]$AcceptanceCriteria,
        [int]$ParentId
    )

    $cliArgs = @(
        'boards', 'work-item', 'create',
        '--title', $Title,
        '--type', $Type,
        '--description', $Description,
        '--output', 'json'
    )
    if ($AcceptanceCriteria) {
        $cliArgs += @('--fields', "Microsoft.VSTS.Common.AcceptanceCriteria=$AcceptanceCriteria")
    }

    $json = az @cliArgs
    $item = $json | ConvertFrom-Json
    Write-Host "Created $Type $($item.id): $Title"

    if ($ParentId) {
        az boards work-item relation add --id $item.id --relation-type parent --target-id $ParentId | Out-Null
    }

    return $item.id
}

function Set-WorkItem {
    param(
        [Parameter(Mandatory)] [int]$Id,
        [Parameter(Mandatory)] [string]$Description,
        [string]$AcceptanceCriteria
    )

    $cliArgs = @(
        'boards', 'work-item', 'update',
        '--id', $Id,
        '--description', $Description,
        '--output', 'json'
    )
    if ($AcceptanceCriteria) {
        $cliArgs += @('--fields', "Microsoft.VSTS.Common.AcceptanceCriteria=$AcceptanceCriteria")
    }

    az @cliArgs | Out-Null
    Write-Host "Updated work item $Id"
}

# ---------------------------------------------------------------------------
# Feature (Id 666 — already created; see Azure-DevOps-Backlog.md)
# ---------------------------------------------------------------------------
$featureDescription = Join-Html @(
    '<p><b>Goal:</b> Enable users (logged-in or anonymous) to submit a 1-5 star rating '
    'for each dad joke, with exactly one rating per joke per user identity, while '
    'keeping the JSON and SQL data modes functionally aligned.</p>'
    '<p><b>Reference docs (in repo):</b><br/>'
    'Docs/Updates/Joke-Ratings/Joke-Rating-Implementation-Plan.md<br/>'
    'Docs/Updates/Joke-Ratings/Joke-Rating-Implementation-Tasks.md</p>'
    '<p><b>Key requirement:</b> A uniqueness rule must prevent duplicate ratings from '
    'the same user for the same joke, while still allowing multiple anonymous users '
    'to rate the same joke independently. Logged-in users are keyed by their stable '
    'authenticated identifier; anonymous users are keyed by a salted hash of their '
    'client IP (ANON_IP_&lt;hash&gt;).</p>'
    '<p><b>Recommended delivery order:</b> Database schema + upsert procedure &rarr; '
    'Repository methods &rarr; API endpoints &rarr; UI wiring &rarr; Tests &rarr; Hardening.</p>'
)
$featureAC = Join-Html @(
    '- A logged-in user can rate any joke and update their own rating.<br/>'
    '- Anonymous users can rate any joke; different anonymous users (different IPs) are not blocked by the uniqueness rule.<br/>'
    '- The same effective user key cannot create duplicate rating rows for the same joke.<br/>'
    '- Joke.Rating (average) and Joke.VoteCount remain accurate after inserts and updates.<br/>'
    '- Feature works identically in both SQL and JSON data modes.'
)

$FeatureId = 666

# ---------------------------------------------------------------------------
# User Stories + Tasks
# ---------------------------------------------------------------------------
$stories = @(
    @{
        Id = 667
        Title = 'Database schema and stored procedure for joke ratings'
        Description = Join-Html @(
            '<p>As a developer, I need the database updated to support a durable, unique '
            'rating-per-user-per-joke model so that upstream layers (repository, API, UI) '
            'have a reliable data foundation to build on.</p>'
            '<p><b>Guidance:</b><br/>'
            '- Add a <code>RatingUserKey nvarchar(255) not null</code> column to <code>Dad.JokeRating</code> '
            '(see src/sql.database/Dad/Tables/JokeRating.sql).<br/>'
            '- Add a unique index/constraint on (JokeId, RatingUserKey) to enforce one rating per joke per user key.<br/>'
            '- Keep the existing UserRating range validation (1-5).<br/>'
            '- Create/update stored procedure <code>usp_Joke_Rate</code> that validates the joke exists, validates the '
            'star value range, upserts by (JokeId, RatingUserKey), recomputes Joke.Rating and Joke.VoteCount from '
            'Dad.JokeRating for that joke, and returns the updated summary plus whether an insert or update occurred.<br/>'
            '- Provide an idempotent migration/patch script for existing environments that adds the column, adds the '
            'constraint, and backfills existing rows (e.g. with legacy CreateUserName or an ANON_LEGACY placeholder) '
            'without erroring on re-run.</p>'
        )
        AcceptanceCriteria = Join-Html @(
            '- Unique index blocks a duplicate row for the same (JokeId, RatingUserKey).<br/>'
            '- Different anonymous IP-derived keys can each rate the same joke independently.<br/>'
            '- usp_Joke_Rate upsert procedure handles both insert and update paths correctly and is idempotent.<br/>'
            '- Joke.Rating and Joke.VoteCount aggregates stay in sync after upsert.<br/>'
            '- Patch script runs cleanly against an existing database with no errors, including on re-run.'
        )
        Tasks = @(
            @{ Id = 668; Title = 'Add RatingUserKey column to JokeRating table'; Description = Join-Html @(
                '<p><b>File(s):</b> src/sql.database/Dad/Tables/JokeRating.sql<br/>'
                '<b>Change:</b> Add column RatingUserKey nvarchar(255) not null. Make CreateUserName nullable or deprecate '
                'it if RatingUserKey becomes the canonical key.<br/>'
                '<b>Acceptance:</b> Column present, default constraint if needed, can insert test rows with the new column.<br/>'
                '<b>Effort:</b> 1 dev-hour</p>'
            ) }
            @{ Id = 669; Title = 'Add unique constraint (JokeId + RatingUserKey)'; Description = Join-Html @(
                '<p><b>File(s):</b> src/sql.database/Dad/Tables/JokeRating.sql<br/>'
                '<b>Change:</b> Add unique index/constraint on (JokeId, RatingUserKey).<br/>'
                '<b>Acceptance:</b> Constraint in place; attempts to insert a duplicate fail with an integrity error.<br/>'
                '<b>Effort:</b> 0.5 dev-hour</p>'
            ) }
            @{ Id = 670; Title = 'Create/update usp_Joke_Rate stored procedure'; Description = Join-Html @(
                '<p><b>File(s):</b> Create src/sql.database/Dad/Stored Procedures/usp_Joke_Rate.sql<br/>'
                '<b>Parameters:</b> @jokeId int, @userRating int (validate 1-5), @ratingUserKey nvarchar(255)<br/>'
                '<b>Logic:</b> Validate JokeId exists; validate UserRating in [1,5]; upsert (insert if (JokeId, RatingUserKey) '
                'not found, else update); after upsert, recompute Joke.Rating and Joke.VoteCount from JokeRating aggregates; '
                'return JokeId, UserRating, AverageRating, VoteCount, @WasInsert bit.<br/>'
                '<b>Acceptance:</b> Procedure callable, idempotent, aggregates stay in sync.<br/>'
                '<b>Effort:</b> 3 dev-hours</p>'
            ) }
            @{ Id = 671; Title = 'Create migration patch script for RatingUserKey'; Description = Join-Html @(
                '<p><b>File(s):</b> Create src/sql.database/Patch/Patch-20260706-add-rating-user-key.sql<br/>'
                '<b>Scope:</b> For existing environments without the RatingUserKey column.<br/>'
                '<b>Logic:</b> Add column, create constraint, backfill existing rows with legacy CreateUserName or ANON_LEGACY.<br/>'
                '<b>Acceptance:</b> Script runs idempotently on an existing DB, no errors.<br/>'
                '<b>Effort:</b> 1.5 dev-hours</p>'
            ) }
        )
    },
    @{
        Id = 672
        Title = 'Repository and data access layer for joke ratings'
        Description = Join-Html @(
            '<p>As a developer, I need the repository layer (both SQL and JSON modes) to expose explicit rating '
            'operations so the API and UI never need to know how ratings are persisted.</p>'
            '<p><b>Guidance:</b><br/>'
            '- Update IJokeRepository (src/web/Data/Repositories/IJokeRepository.cs) with: '
            'SubmitOrUpdateRating(jokeId, userRating, ratingUserKey, requestingUserName), '
            'GetUserRatingForJoke(jokeId, ratingUserKey), GetRatingSummaryForJoke(jokeId).<br/>'
            '- Implement in JokeSQLRepository via the usp_Joke_Rate stored procedure (ExecuteSqlInterpolated or '
            'equivalent), handling transaction semantics where needed.<br/>'
            '- Implement a JSON-mode fallback (JokeJsonRepository) using an in-memory dictionary keyed by '
            '(JokeId, RatingUserKey) so local/dev parity is maintained with SQL mode.</p>'
        )
        AcceptanceCriteria = Join-Html @(
            '- Interface methods are async with clear, strongly-typed return values.<br/>'
            '- SQL repository: unit tests pass for insert, update, and duplicate-reject paths.<br/>'
            '- JSON repository: behavior parity with SQL - a single anonymous key blocks duplicates, different keys do not.<br/>'
            '- Average rating and vote count are correct after mixed inserts/updates in both modes.'
        )
        Tasks = @(
            @{ Id = 673; Title = 'Update IJokeRepository interface for rating operations'; Description = Join-Html @(
                '<p><b>File(s):</b> src/web/Data/Repositories/IJokeRepository.cs<br/>'
                '<b>Methods to add:</b><br/>'
                'Task&lt;(bool Success, int UserRating, decimal AverageRating, int VoteCount, bool WasInsert)&gt; '
                'SubmitOrUpdateRating(int jokeId, int userRating, string ratingUserKey, string requestingUserName);<br/>'
                'Task&lt;int?&gt; GetUserRatingForJoke(int jokeId, string ratingUserKey);<br/>'
                'Task&lt;(decimal AverageRating, int VoteCount)&gt; GetRatingSummaryForJoke(int jokeId);<br/>'
                '<b>Acceptance:</b> Interface compiles, methods are async, clear return types.<br/>'
                '<b>Effort:</b> 1 dev-hour</p>'
            ) }
            @{ Id = 674; Title = 'Implement rating methods in JokeSQLRepository'; Description = Join-Html @(
                '<p><b>File(s):</b> src/web/Data/Repositories/JokeSQLRepository.cs<br/>'
                '<b>Implementation:</b> Call stored procedure usp_Joke_Rate via _context.Database.ExecuteSqlInterpolated '
                '(or equivalent); return result via SqlDataReader or EF parameter mapping; handle transaction semantics '
                'if needed.<br/>'
                '<b>Acceptance:</b> Unit tests pass for insert, update, duplicate-reject paths.<br/>'
                '<b>Effort:</b> 3 dev-hours</p>'
            ) }
            @{ Id = 675; Title = 'Implement fallback rating methods in JokeJsonRepository'; Description = Join-Html @(
                '<p><b>File(s):</b> src/web/Data/Repositories/JokeJsonRepository.cs<br/>'
                '<b>Implementation:</b> In-memory dictionary Dictionary&lt;(int JokeId, string RatingUserKey), int UserRating&gt;; '
                'upsert logic mirrors SQL behavior; aggregate computation from dictionary entries; persist aggregates back '
                'to Joke objects.<br/>'
                '<b>Acceptance:</b> Behavior parity with SQL; a single anonymous key blocks duplicates, different keys do not.<br/>'
                '<b>Effort:</b> 2.5 dev-hours</p>'
            ) }
        )
    },
    @{
        Id = 676
        Title = 'Rating user key resolution service (auth + anonymous IP)'
        Description = Join-Html @(
            '<p>As a developer, I need a single, consistent way to derive the "RatingUserKey" used for uniqueness '
            'across API and Blazor component code paths, so authenticated and anonymous users are keyed correctly '
            'and securely.</p>'
            '<p><b>Guidance:</b><br/>'
            '- Logged-in: use a stable authenticated claim/identity name as the key.<br/>'
            '- Anonymous: resolve the client IP, normalize it, hash it with an application-level salt, and prefix with '
            'ANON_IP_ (do not store plain-text IPs).<br/>'
            '- Only respect X-Forwarded-For when a proxy trust list is configured; fall back safely to RemoteIpAddress '
            'otherwise. This avoids IP spoofing via untrusted client-supplied headers.<br/>'
            '- Add appsettings configuration for the proxy trust list and the hash salt.</p>'
        )
        AcceptanceCriteria = Join-Html @(
            '- Authenticated requests return a consistent, stable key across calls.<br/>'
            '- Anonymous key format is ANON_IP_&lt;hash&gt; and is deterministic for the same IP + salt.<br/>'
            '- Different client IPs produce different anonymous keys.<br/>'
            '- X-Forwarded-For is honored only when a proxy trust list is configured; safe fallback to RemoteIpAddress otherwise.<br/>'
            '- Service is registered in DI and resolves without errors at runtime.'
        )
        Tasks = @(
            @{ Id = 677; Title = 'Create RatingUserKeyResolver service'; Description = Join-Html @(
                '<p><b>File(s):</b> Create src/web/Website/Services/RatingUserKeyResolver.cs<br/>'
                '<b>Logic:</b> Input HttpContext; if authenticated, extract stable claim/identity name; if anonymous, '
                'resolve client IP, hash with salt, return ANON_IP_&lt;hash&gt;; respect X-Forwarded-For only if a '
                'configured proxy list exists; fall back safely to RemoteIpAddress when forwarding is unavailable.<br/>'
                '<b>Configuration:</b> Add appsettings option for proxy trust list and hash salt.<br/>'
                '<b>Acceptance:</b> Unit tests cover auth and anonymous paths, IP normalization, and safe fallback.<br/>'
                '<b>Effort:</b> 2 dev-hours</p>'
            ) }
            @{ Id = 678; Title = 'Register RatingUserKeyResolver in DI'; Description = Join-Html @(
                '<p><b>File(s):</b> src/web/Website/Program.cs<br/>'
                '<b>Change:</b> Add builder.Services.AddScoped&lt;RatingUserKeyResolver&gt;();<br/>'
                '<b>Acceptance:</b> Service resolves at runtime without errors.<br/>'
                '<b>Effort:</b> 0.5 dev-hour</p>'
            ) }
        )
    },
    @{
        Id = 679
        Title = 'API endpoints for rating submit and summary'
        Description = Join-Html @(
            '<p>As a developer, I need REST endpoints that let both anonymous and authenticated clients submit/update '
            'a rating and read rating aggregates, so the UI (and any external client) has a stable contract to call.</p>'
            '<p><b>Guidance:</b><br/>'
            '- POST /api/joke/rate - [AllowAnonymous], validates jokeId exists, resolves the rating user key via '
            'RatingUserKeyResolver, calls SubmitOrUpdateRating, and returns '
            '{ jokeId, userRating, averageRating, voteCount, wasInsert }.<br/>'
            '- GET /api/joke/{id}/rating/summary - [AllowAnonymous], returns { jokeId, averageRating, voteCount } via '
            'GetRatingSummaryForJoke.<br/>'
            '- GET /api/joke/{id}/rating/current (optional, for UI context) - resolves the rating user key and returns '
            '{ jokeId, userRating } or null if not yet rated.<br/>'
            '- All endpoints implemented in src/web/Website/API/JokeController.cs.</p>'
        )
        AcceptanceCriteria = Join-Html @(
            '- POST /api/joke/rate is callable anonymously and while authenticated, returns the documented JSON shape, '
            'and handles invalid input/errors gracefully (400/422 as appropriate).<br/>'
            '- GET /api/joke/{id}/rating/summary returns current aggregates.<br/>'
            "- GET /api/joke/{id}/rating/current returns the current user's rating or null.<br/>"
            '- Two anonymous requests from different IPs can both rate the same joke.<br/>'
            '- The same anonymous IP updates its prior rating rather than creating a duplicate.'
        )
        Tasks = @(
            @{ Id = 680; Title = 'Add rating submit/update endpoint (POST /api/joke/rate)'; Description = Join-Html @(
                '<p><b>File(s):</b> src/web/Website/API/JokeController.cs<br/>'
                '<b>Endpoint:</b> POST /api/joke/rate<br/>'
                '<b>Request body:</b> { "jokeId": 5, "userRating": 4 }<br/>'
                '<b>Response:</b> { "jokeId": 5, "userRating": 4, "averageRating": 3.8, "voteCount": 42, "wasInsert": false }<br/>'
                '<b>Attributes:</b> [AllowAnonymous], [ApiKey]<br/>'
                '<b>Logic:</b> Validate jokeId exists; resolve rating user key; call repository SubmitOrUpdateRating; '
                'return payload.<br/>'
                '<b>Acceptance:</b> Endpoint callable, returns correct structure, handles errors gracefully.<br/>'
                '<b>Effort:</b> 2 dev-hours</p>'
            ) }
            @{ Id = 681; Title = 'Add rating summary endpoint (GET /api/joke/{id}/rating/summary)'; Description = Join-Html @(
                '<p><b>File(s):</b> src/web/Website/API/JokeController.cs<br/>'
                '<b>Endpoint:</b> GET /api/joke/{id}/rating/summary<br/>'
                '<b>Response:</b> { "jokeId": 5, "averageRating": 3.8, "voteCount": 42 }<br/>'
                '<b>Attributes:</b> [AllowAnonymous]<br/>'
                '<b>Logic:</b> Call repository GetRatingSummaryForJoke.<br/>'
                '<b>Acceptance:</b> Endpoint returns current aggregates.<br/>'
                '<b>Effort:</b> 1 dev-hour</p>'
            ) }
            @{ Id = 682; Title = 'Add current user rating endpoint (GET /api/joke/{id}/rating/current)'; Description = Join-Html @(
                '<p><b>File(s):</b> src/web/Website/API/JokeController.cs<br/>'
                '<b>Endpoint:</b> GET /api/joke/{id}/rating/current<br/>'
                '<b>Response:</b> { "jokeId": 5, "userRating": 4 }<br/>'
                '<b>Logic:</b> Resolve rating user key, fetch current user rating.<br/>'
                "<b>Acceptance:</b> Returns user's rating or null if not rated.<br/>"
                '<b>Effort:</b> 1 dev-hour</p>'
            ) }
        )
    },
    @{
        Id = 683
        Title = 'UI integration for joke rating component'
        Description = Join-Html @(
            '<p>As a user, I want to see and submit a 1-5 star rating on each joke so I can express my opinion, and '
            'see the updated community average immediately.</p>'
            '<p><b>Guidance:</b><br/>'
            '- Re-enable the previously-commented-out rating markup in JokeDisplayComponent.razor.<br/>'
            '- In JokeDisplayComponent.razor.cs, inject IJokeRepository (or call the API, depending on the chosen '
            "architecture), load the current user's rating and aggregate summary on init, and wire the submit handler "
            'to call the repository/API, update the display, and show a success/error notification.<br/>'
            '- UX requirements: user can select 1-5 stars; existing rating for the current user context is shown; '
            'updating a prior rating is supported; aggregate average and vote count refresh immediately after submit.</p>'
        )
        AcceptanceCriteria = Join-Html @(
            '- Star control renders without errors and accepts a 1-5 selection.<br/>'
            "- The current user's existing rating (if any) loads and displays correctly.<br/>"
            '- Submitting a rating updates the aggregate average and vote count on screen without a full page reload.<br/>'
            '- Errors from the API/repository are surfaced to the user (toast/snackbar) rather than silently failing.'
        )
        Tasks = @(
            @{ Id = 684; Title = 'Re-enable rating markup in JokeDisplayComponent.razor'; Description = Join-Html @(
                '<p><b>File(s):</b> src/web/Website/Components/JokeDisplayComponent.razor<br/>'
                '<b>Change:</b> Uncomment the rating block (currently commented out around line 38).<br/>'
                '<b>Acceptance:</b> Markup renders without errors.<br/>'
                '<b>Effort:</b> 0.5 dev-hour</p>'
            ) }
            @{ Id = 685; Title = 'Complete rating logic in JokeDisplayComponent.razor.cs'; Description = Join-Html @(
                '<p><b>File(s):</b> src/web/Website/Components/JokeDisplayComponent.razor.cs<br/>'
                '<b>Logic:</b> Inject IJokeRepository; on init, load current user rating and aggregate summary; '
                'OnSubmitRating calls the repository, updates the display, and handles errors; show a success snackbar '
                'or error toast.<br/>'
                '<b>Acceptance:</b> Rating UI loads, submit works, display updates.<br/>'
                '<b>Effort:</b> 3 dev-hours</p>'
            ) }
            @{ Id = 686; Title = 'Wire rating component to API (if API-first approach chosen)'; Description = Join-Html @(
                '<p><b>Consideration:</b> If using the API instead of a direct repository call from the Blazor component, '
                'add HttpClient calls to the endpoints from the API user story.<br/>'
                '<b>Acceptance:</b> Component integrates with the API endpoints.<br/>'
                '<b>Effort:</b> 1 dev-hour</p>'
            ) }
        )
    },
    @{
        Id = 687
        Title = 'Automated tests for joke rating feature'
        Description = Join-Html @(
            '<p>As a developer, I need unit, integration, and (optionally) UI tests covering the rating feature '
            'end-to-end so regressions are caught before release.</p>'
            '<p><b>Guidance:</b><br/>'
            '- Repository (SQL path): cover first insert, update of existing rating, duplicate-key rejection, rating '
            'range validation, aggregate calculation, and independent ratings from different keys.<br/>'
            '- RatingUserKeyResolver: cover authenticated key stability, anonymous IP key format/determinism, different '
            'IPs producing different keys, and safe fallback when forwarding headers are absent or untrusted.<br/>'
            '- API integration tests: anonymous and authenticated POST succeed; two anonymous IPs both succeed on the '
            'same joke; same key updates rather than duplicates; GET summary reflects aggregates; validation errors '
            'return 400/422.<br/>'
            '- Optional Playwright UI test: user can click stars and submit, aggregate updates after submit, error '
            'feedback displays.</p>'
        )
        AcceptanceCriteria = Join-Html @(
            '- All listed repository, resolver, and API scenarios have passing automated tests.<br/>'
            '- Test suite runs cleanly in CI with no flaky failures introduced by this feature.<br/>'
            '- (Optional) Playwright UI test validates the basic star-rating flow end-to-end.'
        )
        Tasks = @(
            @{ Id = 688; Title = 'Unit tests - repository (SQL path)'; Description = Join-Html @(
                '<p><b>File(s):</b> Create/update src/web/Tests/RepositoryTests/JokeRating_Repository_Tests.cs<br/>'
                '<b>Scenarios:</b> first rating insert; update existing rating same key; reject duplicate key (unique '
                'constraint); validate rating range (1-5); aggregate calculation (avg and vote count); different keys '
                'can rate same joke.<br/>'
                '<b>Acceptance:</b> All scenarios covered, tests pass.<br/>'
                '<b>Effort:</b> 3 dev-hours</p>'
            ) }
            @{ Id = 689; Title = 'Unit tests - RatingUserKeyResolver'; Description = Join-Html @(
                '<p><b>File(s):</b> Create src/web/Tests/Services/RatingUserKeyResolver_Tests.cs<br/>'
                '<b>Scenarios:</b> authenticated user returns consistent key; anonymous IP-derived key format correct; '
                'different IPs return different keys; IP hashing deterministic; proxy forwarding respected when '
                'configured; safe fallback on missing forwarding.<br/>'
                '<b>Acceptance:</b> All scenarios covered, tests pass.<br/>'
                '<b>Effort:</b> 2 dev-hours</p>'
            ) }
            @{ Id = 690; Title = 'Integration tests - rating API endpoints'; Description = Join-Html @(
                '<p><b>File(s):</b> Create src/web/Tests/API/JokeRating_API_Tests.cs<br/>'
                '<b>Scenarios:</b> POST rating succeeds for anonymous and authenticated; two anonymous IPs both succeed '
                'on same joke; same user key update overwrites prior rating; GET summary reflects aggregates; '
                'validation errors return 400/422.<br/>'
                '<b>Acceptance:</b> Integration test suite runs, all pass.<br/>'
                '<b>Effort:</b> 3 dev-hours</p>'
            ) }
            @{ Id = 691; Title = 'Playwright UI tests for rating flow (optional)'; Description = Join-Html @(
                '<p><b>File(s):</b> Create playwright/ui-tests/joke-rating.spec.ts<br/>'
                '<b>Scenarios:</b> user can click stars and submit rating; rating persists on reload (if applicable); '
                'aggregate updates after submit; error feedback displays.<br/>'
                '<b>Acceptance:</b> Playwright tests execute, basic UI flow validates.<br/>'
                '<b>Effort:</b> 2 dev-hours</p>'
            ) }
        )
    },
    @{
        Id = 692
        Title = 'Documentation and hardening for joke rating feature'
        Description = Join-Html @(
            '<p>As a developer/operator, I need the feature documented and reviewed for security, performance, and '
            'concurrency concerns before it ships, so it can be operated and maintained safely.</p>'
            '<p><b>Guidance:</b><br/>'
            '- Update README/Docs with a feature overview, configuration (proxy trust list, hash salt), known '
            'limitations (shared NAT/proxy can represent multiple anonymous users behind one IP), and deployment '
            'notes.<br/>'
            '- Code review focus areas: security (IP hashing, forwarding header validation), performance (aggregate '
            'recomputation efficiency), concurrency (race conditions on upsert), and error handling (missing joke, '
            'invalid rating).<br/>'
            '- Consider documenting a roadmap note: Phase 2 could combine the IP hash with a long-lived anonymous '
            'browser token cookie for better per-user distinction, and/or add anonymous rate-limiting per IP.</p>'
        )
        AcceptanceCriteria = Join-Html @(
            '- Documentation clearly explains the feature, its configuration, and its known limitations for future '
            'maintainers/operators.<br/>'
            '- Code review completed with no blocking issues; security, performance, and concurrency concerns '
            'explicitly addressed or accepted as documented tradeoffs.'
        )
        Tasks = @(
            @{ Id = 693; Title = 'Update README / documentation for rating feature'; Description = Join-Html @(
                '<p><b>File(s):</b> Update Docs/, README<br/>'
                '<b>Content:</b> Feature overview; configuration (proxy trust, salt); known limitations (NAT behind '
                'shared proxy); deployment notes.<br/>'
                '<b>Acceptance:</b> Clear docs for operators and future maintainers.<br/>'
                '<b>Effort:</b> 1 dev-hour</p>'
            ) }
            @{ Id = 694; Title = 'Code review and refinement'; Description = Join-Html @(
                '<p><b>Review focus:</b> Security (IP hashing, forwarding header validation); performance (aggregate '
                'recomputation efficiency); concurrency (race conditions on upsert); error handling (edge cases - '
                'missing joke, invalid rating).<br/>'
                '<b>Acceptance:</b> Code review approved, no blocking issues.<br/>'
                '<b>Effort:</b> 2 dev-hours</p>'
            ) }
        )
    }
)

$summary = New-Object System.Collections.Generic.List[object]

if ($Mode -eq 'Create') {
    $featureId = New-WorkItem -Title 'Joke Rating System - Persistent Multi-User Ratings' `
        -Type 'Feature' -Description $featureDescription -AcceptanceCriteria $featureAC
    $summary.Add([pscustomobject]@{ Id = $featureId; Type = 'Feature'; ParentId = $null; Title = 'Joke Rating System - Persistent Multi-User Ratings' })

    foreach ($story in $stories) {
        $storyId = New-WorkItem -Title $story.Title -Type 'User Story' `
            -Description $story.Description -AcceptanceCriteria $story.AcceptanceCriteria -ParentId $featureId
        $summary.Add([pscustomobject]@{ Id = $storyId; Type = 'User Story'; ParentId = $featureId; Title = $story.Title })

        foreach ($task in $story.Tasks) {
            $taskId = New-WorkItem -Title $task.Title -Type 'Task' -Description $task.Description -ParentId $storyId
            $summary.Add([pscustomobject]@{ Id = $taskId; Type = 'Task'; ParentId = $storyId; Title = $task.Title })
        }
    }
}
else {
    Set-WorkItem -Id $FeatureId -Description $featureDescription -AcceptanceCriteria $featureAC
    $summary.Add([pscustomobject]@{ Id = $FeatureId; Type = 'Feature'; ParentId = $null; Title = 'Joke Rating System - Persistent Multi-User Ratings' })

    foreach ($story in $stories) {
        Set-WorkItem -Id $story.Id -Description $story.Description -AcceptanceCriteria $story.AcceptanceCriteria
        $summary.Add([pscustomobject]@{ Id = $story.Id; Type = 'User Story'; ParentId = $FeatureId; Title = $story.Title })

        foreach ($task in $story.Tasks) {
            Set-WorkItem -Id $task.Id -Description $task.Description
            $summary.Add([pscustomobject]@{ Id = $task.Id; Type = 'Task'; ParentId = $story.Id; Title = $task.Title })
        }
    }
}

$summary | Format-Table -AutoSize
$summary | Export-Csv -Path (Join-Path $PSScriptRoot 'AzDO-WorkItems-Created.csv') -NoTypeInformation
Write-Host "`nWork item summary exported to AzDO-WorkItems-Created.csv"
