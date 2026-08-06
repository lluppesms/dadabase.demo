# Detailed Execution Plan (Issue #80)

## Objective

Keep `AIHelper` business logic the same while extracting model chat calls into services and enabling runtime provider switching between Agent Framework and Copilot SDK.

## Proposed design

### New abstraction

Create `IAiChatService` under `/src/web/Website/Services` with a minimal completion contract:

- `CompleteAsync(systemPrompt, userMessage, cancellationToken)` -> `string`

### Implementations

- `AgentFrameworkChatService`
  - Uses existing Azure OpenAI + Agent Framework pattern.
  - Serves as behavior-preserving baseline.
- `CopilotSdkChatService`
  - Uses Copilot SDK provider configuration with managed identity token provider.
  - Uses Foundry/OpenAI-compatible model endpoint.

### Selection strategy

Add config flag:

- `AppSettings:AiServiceProvider`
  - `AgentFramework` (default)
  - `CopilotSDK`

Wire provider selection in `Program.cs` via DI.

## Phased rollout plan

1. **Spike (required gate)**
   - Build a minimal viability spike proving Copilot SDK + managed identity works for target endpoint and environment.
   - Document results in this folder before code refactor begins.
2. **Abstraction baseline (no behavior change)**
   - Add `IAiChatService`.
   - Add `AgentFrameworkChatService` and route `AIHelper` chat calls through it.
   - Keep default provider as `AgentFramework`.
3. **Provider switching**
   - Add `AiServiceProvider` config and DI branch selection.
   - Add/adjust tests for provider selection and unchanged `AIHelper` parsing behavior.
4. **Copilot SDK provider**
   - Implement `CopilotSdkChatService` using validated spike findings.
   - Keep feature flag default on `AgentFramework` for safe rollout.
5. **Validation and cleanup**
   - Validate both providers for all three chat methods.
   - Remove obsolete agent-init code in `AIHelper` only after parity criteria are met.

## Test strategy

- Unit tests for provider selection from config.
- Unit tests for `AIHelper` parsing/business logic using mocked `IAiChatService` responses.
- Targeted integration checks for each provider (non-empty, parseable outputs).
- Keep image generation tests out of scope unless impacted by chat abstraction changes.

## Acceptance criteria

- `AIHelper` business outputs remain equivalent for:
  - `GetJokeSceneDescription`
  - `SuggestCategories`
  - `AnalyzeJoke`
- Provider can be selected by `AppSettings:AiServiceProvider` without code changes.
- Default behavior remains backward compatible (`AgentFramework`).
- Copilot SDK viability spike is complete and documented.
- Failure modes are clear and non-breaking (graceful error handling retained).

## Rollback

If Copilot SDK provider fails in runtime testing, keep flag on `AgentFramework` and defer switch while retaining abstraction baseline.
