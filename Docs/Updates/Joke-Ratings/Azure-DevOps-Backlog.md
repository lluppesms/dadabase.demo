# Joke Rating Feature – Azure DevOps Backlog

Date created: 2026-08-17
Organization: `https://dev.azure.com/lyleluppes`
Project: `GitHubDevOps`

## Summary

The Joke Rating feature described in [Joke-Rating-Implementation-Plan.md](Joke-Rating-Implementation-Plan.md)
and [Joke-Rating-Implementation-Tasks.md](Joke-Rating-Implementation-Tasks.md) was broken down into a
Feature → User Story → Task hierarchy and created in Azure DevOps Boards using the Azure CLI
(`az boards work-item create` / `az boards work-item relation add`).

The creation script is [Create-AzDO-WorkItems.ps1](Create-AzDO-WorkItems.ps1). It is data-driven (one
`$stories` array with an embedded `Tasks` array per story) so the same script doubles as documentation
of exactly what was created and why. **Re-running it will create duplicate work items** — it was written
as a one-time setup script, not an idempotent sync tool.

Each User Story description includes the developer-facing guidance (files to touch, logic, and design
notes) pulled from the implementation docs, plus a dedicated Acceptance Criteria field capturing the
Definition of Done for that story. Each Task description includes the specific file(s), implementation
notes, its own acceptance check, and the original effort estimate from the task queue doc.

## Work Item Hierarchy Created

| Id | Type | Parent | Title |
|----|------|--------|-------|
| 666 | Feature | — | Joke Rating System - Persistent Multi-User Ratings |
| 667 | User Story | 666 | Database schema and stored procedure for joke ratings |
| 668 | Task | 667 | Add RatingUserKey column to JokeRating table |
| 669 | Task | 667 | Add unique constraint (JokeId + RatingUserKey) |
| 670 | Task | 667 | Create/update usp_Joke_Rate stored procedure |
| 671 | Task | 667 | Create migration patch script for RatingUserKey |
| 672 | User Story | 666 | Repository and data access layer for joke ratings |
| 673 | Task | 672 | Update IJokeRepository interface for rating operations |
| 674 | Task | 672 | Implement rating methods in JokeSQLRepository |
| 675 | Task | 672 | Implement fallback rating methods in JokeJsonRepository |
| 676 | User Story | 666 | Rating user key resolution service (auth + anonymous IP) |
| 677 | Task | 676 | Create RatingUserKeyResolver service |
| 678 | Task | 676 | Register RatingUserKeyResolver in DI |
| 679 | User Story | 666 | API endpoints for rating submit and summary |
| 680 | Task | 679 | Add rating submit/update endpoint (POST /api/joke/rate) |
| 681 | Task | 679 | Add rating summary endpoint (GET /api/joke/{id}/rating/summary) |
| 682 | Task | 679 | Add current user rating endpoint (GET /api/joke/{id}/rating/current) |
| 683 | User Story | 666 | UI integration for joke rating component |
| 684 | Task | 683 | Re-enable rating markup in JokeDisplayComponent.razor |
| 685 | Task | 683 | Complete rating logic in JokeDisplayComponent.razor.cs |
| 686 | Task | 683 | Wire rating component to API (if API-first approach chosen) |
| 687 | User Story | 666 | Automated tests for joke rating feature |
| 688 | Task | 687 | Unit tests - repository (SQL path) |
| 689 | Task | 687 | Unit tests - RatingUserKeyResolver |
| 690 | Task | 687 | Integration tests - rating API endpoints |
| 691 | Task | 687 | Playwright UI tests for rating flow (optional) |
| 692 | User Story | 666 | Documentation and hardening for joke rating feature |
| 693 | Task | 692 | Update README / documentation for rating feature |
| 694 | Task | 692 | Code review and refinement |

A machine-readable copy of this table is saved alongside the script as
[AzDO-WorkItems-Created.csv](AzDO-WorkItems-Created.csv).

## Known Issue: Truncated Descriptions (Fixed 2026-08-17)

The initial creation run produced Descriptions that were cut off mid-sentence and an empty
Acceptance Criteria field on every User Story. Root cause: `az` on Windows is a shim (`az.cmd`)
executed through `cmd.exe`, and any CLI argument containing a **raw newline** gets silently
truncated by cmd.exe's argument parser — text after the first newline is dropped, and a
`--fields "Field=Value"` argument with an embedded newline can be dropped entirely. The original
script built Description/AcceptanceCriteria text with PowerShell here-strings, which contain real
newline characters between lines.

**Fix:** [Create-AzDO-WorkItems.ps1](Create-AzDO-WorkItems.ps1) was rewritten so every
Description/AcceptanceCriteria value is built as a single-line HTML string (via a `Join-Html`
helper that joins an array of string fragments with no separator), using `<br/>` for line breaks
instead of real newlines. Task descriptions were also expanded from a single file path to the
full guidance (file(s), logic/parameters, acceptance check, effort estimate) in the same
`<b>Label:</b> ...<br/>` style as the stories.

The script now supports a `-Mode` parameter:
- `-Mode Create` (default) — creates new work items (used for the original 666-694 run).
- `-Mode Update` — updates the existing work items in place (ids hard-coded to match the original
  run) with corrected content. This was run once to repair items 666-694 and is safe to re-run.

## How It Was Created

1. Confirmed Azure CLI + `azure-devops` extension were installed and authenticated.
2. Set CLI defaults to the target org/project:
   ```powershell
   az devops configure --defaults organization=https://dev.azure.com/lyleluppes project=GitHubDevOps
   ```
3. Ran [Create-AzDO-WorkItems.ps1](Create-AzDO-WorkItems.ps1), which:
   - Creates the Feature.
   - Loops through 7 User Stories (one per implementation phase from the task queue doc), creating each
     with a `parent` relation to the Feature.
   - Loops through each User Story's Tasks, creating each with a `parent` relation to its User Story.
   - Exports the full id/type/parent/title table to `AzDO-WorkItems-Created.csv`.

## Notes / Follow-ups

- Work items were created unassigned and without an iteration/area path — assign owners and sprints in
  Azure Boards before starting work.
- The `Task 3` under "UI integration" (Wire rating component to API) is conditional on choosing an
  API-first architecture for the Blazor component vs. calling the repository directly — resolve that
  design decision before starting the UI story.
- Process template for `GitHubDevOps` is **Agile**, so the hierarchy uses `Feature` → `User Story` → `Task`
  work item types.
