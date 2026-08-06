# AI Abstraction Planning (Issue #80)

This folder contains the research and implementation planning artifacts for migrating AI chat and image calls in `AIHelper` to provider-based abstractions.

## Contents

- `research-findings.md` – research notes comparing current Agent Framework usage with a Copilot SDK-based approach.
- `execution-plan.md` – detailed, phased implementation plan with acceptance criteria and rollback strategy.
- `rubber-duck-review.md` – independent critique of the plan and risk adjustments before implementation.

## Scope of this issue

Per issue #80, this work is planning-only. It does **not** implement code changes yet. The implementation should begin after the plan and acceptance criteria in these docs are approved.

## Copilot SDK validation status

The Copilot SDK BYOK + managed identity approach is considered validated from the referenced sample repository:

- `https://github.com/lluppesms/simple.ghcp.sdk.byok`
- `src/web/Services/GHCP_SDK_Service.cs`
