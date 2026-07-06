# DadABase Joke Rating Implementation Plan

Date: 2026-07-06
Status: Proposed for review

## Goal
Enable users to submit a 1-5 star rating for each joke, whether they are logged in or anonymous, with one rating per joke per user identity key.

## Key Requirement Clarification
A uniqueness rule must prevent duplicate ratings from the same user for the same joke, while still allowing multiple anonymous users to rate the same joke.

To satisfy this:
- Logged-in users will use their authenticated user key.
- Anonymous users will use an IP-based key (derived from request IP), so different anonymous users can each submit their own rating.

## Identity Key Strategy
Define a single RatingUserKey value used by uniqueness checks and upsert logic.

- Logged-in request:
  - RatingUserKey = authenticated user identifier (stable claim or identity name).
- Anonymous request:
  - RatingUserKey = derived from client IP address.

Recommended anonymous key format:
- Raw source: normalized client IP from trusted forwarding headers or remote endpoint.
- Stored key: hash of IP with app-level salt (not plain-text IP).
- Example concept: ANON_IP_SHA256(<ip> + <salt>)

Why hash the IP:
- Reduces storage of plain PII.
- Still provides deterministic uniqueness for anonymous requests.

## Database Changes
1. Add identity key column to rating table.
- Add column on Dad.JokeRating:
  - RatingUserKey nvarchar(255) not null

2. Add uniqueness constraint.
- Unique index on:
  - JokeId
  - RatingUserKey

This enforces one rating per joke per effective user key.

3. Keep existing rating validation.
- UserRating must remain between 1 and 5.

4. Add or update stored procedure for rating upsert.
- Procedure behavior:
  - Validate joke exists.
  - Validate star value range.
  - Upsert by JokeId + RatingUserKey.
  - Recompute Joke.Rating and Joke.VoteCount from Dad.JokeRating for that joke.
  - Return updated summary and whether insert or update occurred.

## Application Changes
### 1. Repository Contract
Update repository interface with explicit rating operations:
- SubmitOrUpdateRating(jokeId, userRating, ratingUserKey)
- GetUserRatingForJoke(jokeId, ratingUserKey)
- GetRatingSummaryForJoke(jokeId)

### 2. SQL Repository
Implement methods in the SQL repository using stored procedure execution.

### 3. JSON Repository Fallback
Mirror rating behavior in JSON mode so local and SQL modes stay functionally aligned.

### 4. Identity Resolution Service
Add a dedicated helper/service that builds RatingUserKey consistently across API and Blazor component paths.

Rules:
- Logged-in: use stable authenticated identifier.
- Anonymous: resolve client IP, normalize, hash with salt, prefix with ANON_IP_.

Important implementation detail:
- Respect forwarded headers only when proxy trust is configured.
- Avoid using untrusted client-provided headers directly.

### 5. API Endpoints
Add API endpoint(s) for rating submit/update and summary fetch:
- POST rating (AllowAnonymous)
- GET user + aggregate rating context

Response should include:
- JokeId
- UserRating
- AverageRating
- VoteCount
- WasUpdate

### 6. UI Integration
Re-enable rating UI in joke display component and wire to new repository/API methods.

UX requirements:
- User can select 1-5 stars.
- Existing rating is shown for current user context.
- Updating a prior rating is supported.
- Aggregate average and vote count refresh after submit.

## Testing Tasks
### Database
- Unique index blocks duplicate row for same JokeId + RatingUserKey.
- Different anonymous IP keys can rate the same joke independently.
- Upsert procedure handles insert and update paths correctly.

### Repository
- Submit first rating creates row.
- Submit second rating same key updates row, does not create duplicate.
- Invalid ratings are rejected.
- Average and vote count are correct after mixed updates.

### API
- Anonymous rating with IP-derived key succeeds.
- Logged-in rating succeeds.
- Two anonymous requests from different IPs both succeed on same joke.
- Same anonymous IP updates prior rating on same joke.

### UI/Component
- Star control renders and submits.
- Existing user rating loads and displays.
- Aggregate values update after submission.

## Security, Privacy, and Operations Considerations
1. IP as anonymous key tradeoffs.
- Pros: straightforward uniqueness for anonymous users.
- Cons: shared NAT/proxy can represent multiple users behind one IP.

2. Mitigation options (recommended roadmap).
- Phase 1: IP-derived hashed key (fastest to deliver).
- Phase 2: combine IP hash + long-lived anonymous browser token cookie for better per-user distinction.

3. Forwarded header trust.
- Configure known proxies/networks before honoring X-Forwarded-For.
- Fall back safely to remote IP when forwarding is unavailable.

4. Data retention.
- Document retention policy for rating records and hashed anonymous keys.

## Implementation Task Breakdown
1. SQL schema update for RatingUserKey and unique index.
2. SQL stored procedure for rating upsert and aggregate recompute.
3. Repository interface update.
4. SQL repository implementation.
5. JSON repository parity implementation.
6. Rating user key resolver (auth and anonymous IP handling).
7. API endpoints for submit and summary.
8. Joke display component rating UI activation and wiring.
9. Unit and integration tests.
10. Documentation updates and deployment notes.

## Acceptance Criteria
- A logged-in user can rate any joke and update their own rating.
- Anonymous users can rate any joke.
- Different anonymous users are not blocked by uniqueness (IP-derived keys differ).
- Same user key cannot create duplicate rows for the same joke.
- Joke.Rating and Joke.VoteCount remain accurate after inserts and updates.
- Feature works in both SQL and JSON data modes.

## Open Questions for Final Approval
1. Should anonymous key be IP hash only, or IP hash plus persistent anonymous cookie in phase 1?
2. Which authenticated claim should be canonical for logged-in user key in this app?
3. Is rating change history needed, or only current effective rating per user?
4. Should anonymous ratings be rate-limited per IP to reduce abuse?

## Recommended First Implementation Slice
To reduce delivery risk, implement in this order:
1. Database schema + upsert procedure.
2. Repository methods.
3. API endpoint.
4. UI component wiring.
5. Tests and hardening.

This sequence gives early validation at the data boundary before UI work.
