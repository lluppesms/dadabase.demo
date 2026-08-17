# Joke Rating Feature – Implementation Task Queue

**Branch:** `feature/joke-rating-system`  
**Status:** Queued for implementation  
**Reference:** See [Joke-Rating-Implementation-Plan.md](Joke-Rating-Implementation-Plan.md)

---

## Phase 1: Database & Stored Procedure (Foundation)

### Task 1.1: Add RatingUserKey Column to JokeRating Table
- **File(s):** `src/sql.database/Dad/Tables/JokeRating.sql`
- **Change:** Add column `RatingUserKey nvarchar(255) not null`
- **Constraint:** Make CreateUserName nullable or deprecate if RatingUserKey becomes the canonical key
- **Acceptance:** Column present, default constraint if needed, can insert test rows with new column
- **Effort:** 1 dev-hour

### Task 1.2: Add Unique Constraint (JokeId + RatingUserKey)
- **File(s):** `src/sql.database/Dad/Tables/JokeRating.sql`
- **Change:** Add unique index/constraint on (JokeId, RatingUserKey)
- **Acceptance:** Constraint in place; attempts to insert duplicate fail with integrity error
- **Effort:** 0.5 dev-hour

### Task 1.3: Create/Update usp_Joke_Rate Stored Procedure
- **File(s):** Create `src/sql.database/Dad/Stored Procedures/usp_Joke_Rate.sql`
- **Parameters:**
  - @jokeId int
  - @userRating int (validate 1-5)
  - @ratingUserKey nvarchar(255)
- **Logic:**
  - Validate JokeId exists
  - Validate UserRating in [1,5]
  - Upsert: if (JokeId, RatingUserKey) exists, update; else insert
  - After upsert, recompute Joke.Rating and Joke.VoteCount from JokeRating aggregates
  - Return: JokeId, UserRating, AverageRating, VoteCount, @WasInsert bit
- **Acceptance:** Procedure callable, idempotent, aggregates stay in sync
- **Effort:** 3 dev-hours

### Task 1.4: Create Migration Patch Script
- **File(s):** Create `src/sql.database/Patch/Patch-20260706-add-rating-user-key.sql`
- **Scope:** For existing environments without RatingUserKey column
- **Logic:** Add column, create constraint, backfill existing rows with legacy CreateUserName or ANON_LEGACY
- **Acceptance:** Script runs idempotently on existing DB, no errors
- **Effort:** 1.5 dev-hours

---

## Phase 2: Repository & Data Access Layer

### Task 2.1: Update IJokeRepository Interface
- **File(s):** `src/web/Data/Repositories/IJokeRepository.cs`
- **Methods to add:**
  - `Task<(bool Success, int UserRating, decimal AverageRating, int VoteCount, bool WasInsert)> SubmitOrUpdateRating(int jokeId, int userRating, string ratingUserKey, string requestingUserName);`
  - `Task<int?> GetUserRatingForJoke(int jokeId, string ratingUserKey);`
  - `Task<(decimal AverageRating, int VoteCount)> GetRatingSummaryForJoke(int jokeId);`
- **Acceptance:** Interface compiles, methods are async, clear return types
- **Effort:** 1 dev-hour

### Task 2.2: Implement Methods in JokeSQLRepository
- **File(s):** `src/web/Data/Repositories/JokeSQLRepository.cs`
- **Implementation:**
  - Call stored procedure usp_Joke_Rate via _context.Database.ExecuteSqlInterpolated
  - Return result using SqlDataReader or EF parameter mapping
  - Handle transaction semantics if needed
- **Acceptance:** Unit tests pass for insert, update, duplicate-reject paths
- **Effort:** 3 dev-hours

### Task 2.3: Implement Fallback Methods in JokeJsonRepository
- **File(s):** `src/web/Data/Repositories/JokeJsonRepository.cs`
- **Implementation:**
  - In-memory dictionary: Dictionary<(int JokeId, string RatingUserKey), int UserRating>
  - Upsert logic mirrors SQL behavior
  - Aggregate computation from dictionary entries
  - Persist aggregates back to Joke objects
- **Acceptance:** Behavior parity with SQL; single anonymous key blocks duplicate, different keys don't
- **Effort:** 2.5 dev-hours

---

## Phase 3: Application Services

### Task 3.1: Create RatingUserKeyResolver Service
- **File(s):** Create `src/web/Website/Services/RatingUserKeyResolver.cs`
- **Logic:**
  - Input: HttpContext
  - If authenticated: extract stable claim/identity name
  - If anonymous: resolve client IP, hash with salt, return ANON_IP_<hash>
  - Respect X-Forwarded-For only if configured proxy list
  - Fall back safely to RemoteIpAddress when forwarding unavailable
- **Configuration:** Add appsettings option for proxy trust list and hash salt
- **Acceptance:** Unit tests for auth and anon paths, IP normalization, safe fallback
- **Effort:** 2 dev-hours

### Task 3.2: Register RatingUserKeyResolver in DI
- **File(s):** `src/web/Website/Program.cs`
- **Change:** Add `builder.Services.AddScoped<RatingUserKeyResolver>();`
- **Acceptance:** Service resolves at runtime without errors
- **Effort:** 0.5 dev-hour

---

## Phase 4: API Endpoints

### Task 4.1: Add Rating Submit/Update Endpoint
- **File(s):** `src/web/Website/API/JokeController.cs`
- **Endpoint:** POST `/api/joke/rate`
- **Request body:**
  ```json
  { "jokeId": 5, "userRating": 4 }
  ```
- **Response:**
  ```json
  {
    "jokeId": 5,
    "userRating": 4,
    "averageRating": 3.8,
    "voteCount": 42,
    "wasInsert": false
  }
  ```
- **Attributes:** [AllowAnonymous], [ApiKey]
- **Logic:**
  - Validate jokeId exists
  - Resolve rating user key
  - Call repository SubmitOrUpdateRating
  - Return payload
- **Acceptance:** Endpoint callable, returns correct structure, handles errors gracefully
- **Effort:** 2 dev-hours

### Task 4.2: Add Rating Summary Endpoint
- **File(s):** `src/web/Website/API/JokeController.cs`
- **Endpoint:** GET `/api/joke/{id}/rating/summary`
- **Response:**
  ```json
  { "jokeId": 5, "averageRating": 3.8, "voteCount": 42 }
  ```
- **Attributes:** [AllowAnonymous]
- **Logic:** Call repository GetRatingSummaryForJoke
- **Acceptance:** Endpoint returns current aggregates
- **Effort:** 1 dev-hour

### Task 4.3: Add User Rating Endpoint (Optional—for UI context)
- **File(s):** `src/web/Website/API/JokeController.cs`
- **Endpoint:** GET `/api/joke/{id}/rating/current`
- **Response:**
  ```json
  { "jokeId": 5, "userRating": 4 }
  ```
- **Logic:** Resolve rating user key, fetch current user rating
- **Acceptance:** Returns user's rating or null if not rated
- **Effort:** 1 dev-hour

---

## Phase 5: UI Component Integration

### Task 5.1: Re-enable Rating Markup in JokeDisplayComponent.razor
- **File(s):** `src/web/Website/Components/JokeDisplayComponent.razor`
- **Change:** Uncomment the rating block (currently commented out around line 38)
- **Acceptance:** Markup renders without errors
- **Effort:** 0.5 dev-hour

### Task 5.2: Complete Rating Logic in JokeDisplayComponent.razor.cs
- **File(s):** `src/web/Website/Components/JokeDisplayComponent.razor.cs`
- **Logic:**
  - Inject IJokeRepository
  - On init: load current user rating and aggregate summary
  - OnSubmitRating: call repository, update display, handle errors
  - Show success snackbar or error toast
- **Acceptance:** Rating UI loads, submit works, display updates
- **Effort:** 3 dev-hours

### Task 5.3: Wire Component to API
- **Consideration:** If using API instead of direct repository, add HttpClient calls
- **Acceptance:** Component integrates with API endpoints
- **Effort:** 1 dev-hour (if API-first approach)

---

## Phase 6: Testing

### Task 6.1: Unit Tests – Repository (SQL Path)
- **File(s):** Create/update `src/web/Tests/RepositoryTests/JokeRating_Repository_Tests.cs`
- **Scenarios:**
  - First rating insert
  - Update existing rating same key
  - Reject duplicate key (unique constraint)
  - Validate rating range (1-5)
  - Aggregate calculation (avg and vote count)
  - Different keys can rate same joke
- **Acceptance:** All scenarios covered, tests pass
- **Effort:** 3 dev-hours

### Task 6.2: Unit Tests – RatingUserKeyResolver
- **File(s):** Create `src/web/Tests/Services/RatingUserKeyResolver_Tests.cs`
- **Scenarios:**
  - Authenticated user returns consistent key
  - Anonymous IP-derived key format correct
  - Different IPs return different keys
  - IP hashing deterministic
  - Proxy forwarding respected when configured
  - Safe fallback on missing forwarding
- **Acceptance:** All scenarios covered, tests pass
- **Effort:** 2 dev-hours

### Task 6.3: Integration Tests – API Endpoints
- **File(s):** Create `src/web/Tests/API/JokeRating_API_Tests.cs`
- **Scenarios:**
  - POST rating succeeds for anonymous and authenticated
  - Two anonymous IPs both succeed on same joke
  - Same user key update overwrites prior rating
  - GET summary reflects aggregates
  - Validation errors return 400/422
- **Acceptance:** Integration test suite runs, all pass
- **Effort:** 3 dev-hours

### Task 6.4: Playwright UI Tests (Optional)
- **File(s):** Create `playwright/ui-tests/joke-rating.spec.ts`
- **Scenarios:**
  - User can click stars and submit rating
  - Rating persists on reload (if applicable)
  - Aggregate updates after submit
  - Error feedback displays
- **Acceptance:** Playwright tests execute, basic UI flow validates
- **Effort:** 2 dev-hours

---

## Phase 7: Documentation & Hardening

### Task 7.1: Update README / Documentation
- **File(s):** Update `Docs/`, README
- **Content:**
  - Feature overview
  - Configuration (proxy trust, salt)
  - Known limitations (NAT behind shared proxy)
  - Deployment notes
- **Acceptance:** Clear docs for operators and future maintainers
- **Effort:** 1 dev-hour

### Task 7.2: Code Review & Refinement
- **Review focus:**
  - Security: IP hashing, forwarding header validation
  - Performance: aggregate recomputation efficiency
  - Concurrency: race conditions on upsert
  - Error handling: edge cases (missing joke, invalid rating)
- **Acceptance:** Code review approved, no blocking issues
- **Effort:** 2 dev-hours

---

## Summary

| Phase | Task Count | Est. Effort | Notes |
|-------|------------|-------------|-------|
| 1: DB | 4 | 6.5 hrs | Foundation; unblock all layers |
| 2: Repository | 3 | 8.5 hrs | SQL + JSON fallback for feature parity |
| 3: Services | 2 | 2.5 hrs | Identity resolution, lightweight DI |
| 4: API | 3 | 4 hrs | RESTful endpoints, clear contracts |
| 5: UI | 3 | 4.5 hrs | Component wiring, UX integration |
| 6: Testing | 4 | 10 hrs | Unit, integration, optional E2E |
| 7: Hardening | 2 | 3 hrs | Docs, review, deployment readiness |
| **Total** | **21** | **~39 hours** | |

---

## Recommended Execution Order

1. **Database first** (Tasks 1.1 – 1.4): Unblocks all downstream work.
2. **Repository layer** (Tasks 2.1 – 2.3): Implement data access before UI.
3. **Services & API** (Tasks 3.1 – 4.3): Identity + endpoints ready for UI.
4. **UI integration** (Tasks 5.1 – 5.3): Wire components once API stable.
5. **Testing in parallel** (Tasks 6.1 – 6.4): Begin early, expand as code settles.
6. **Hardening** (Tasks 7.1 – 7.2): Final review before PR.

---

## Success Criteria

- [ ] Database schema updated and tested.
- [ ] Stored procedure upserts correctly and recomputes aggregates.
- [ ] Repository methods pass unit tests for both SQL and JSON modes.
- [ ] API endpoints are callable and return correct payloads.
- [ ] UI component loads and submits ratings.
- [ ] Different anonymous users (different IPs) can both rate the same joke.
- [ ] Same user key cannot create duplicate ratings for the same joke.
- [ ] Integration tests pass.
- [ ] Code review approved.
- [ ] Documentation updated.
