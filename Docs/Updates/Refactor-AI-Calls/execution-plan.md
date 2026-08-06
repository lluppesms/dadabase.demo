# Detailed Execution Plan (Issue #80)

## Objective

Keep `AIHelper` business logic the same while extracting model chat and image calls into services and enabling runtime chat provider switching between Agent Framework and Copilot SDK.

## Proposed design

### New abstractions

Create `IAiChatService` under `/src/web/Website/Services` with a minimal completion contract:

- `CompleteAsync(systemPrompt, userMessage, cancellationToken)` -> `string`

Create `IAiImageService` under `/src/web/Website/Services` to encapsulate image-generation operations currently implemented inside `AIHelper`.

### Implementations

- `AgentFrameworkChatService`
  - Uses existing Azure OpenAI + Agent Framework pattern.
  - Serves as behavior-preserving baseline.
- `CopilotSdkChatService`
  - Uses Copilot SDK provider configuration with managed identity token provider.
  - Follows the validated pattern from `lluppesms/simple.ghcp.sdk.byok` (`src/web/Services/GHCP_SDK_Service.cs`).
  - Uses Foundry/OpenAI-compatible model endpoint.
- `AiImageService`
  - Wraps existing image generation provider logic (MAI, Azure OpenAI image, OpenAI image) while preserving current output behavior.

### Selection strategy

Add config flag:

- `AppSettings:AiServiceProvider`
  - `AgentFramework` (default)
  - `CopilotSDK`

Wire provider selection in `Program.cs` via DI.

## Phased rollout plan

1. **Abstraction baseline (no behavior change)**
   - Add `IAiChatService`.
   - Add `IAiImageService`.
   - Add `AgentFrameworkChatService` and route `AIHelper` chat calls through it.
   - Add `AiImageService` and route `AIHelper` image generation through it.
   - Keep default provider as `AgentFramework`.
2. **Provider switching**
   - Add `AiServiceProvider` config and DI branch selection.
   - Add/adjust tests for provider selection and unchanged `AIHelper` parsing behavior.
3. **Copilot SDK provider**
   - Implement `CopilotSdkChatService` using the validated sample pattern.
   - Keep feature flag default on `AgentFramework` for safe rollout.
4. **Validation and cleanup**
   - Validate both chat providers for all three chat methods.
   - Validate `IAiImageService` parity for existing image generation behavior.
   - Remove obsolete agent-init code in `AIHelper` only after parity criteria are met.

## Test strategy

- Unit tests for provider selection from config.
- Unit tests for `AIHelper` parsing/business logic using mocked `IAiChatService` responses.
- Unit tests for `AIHelper` image orchestration using mocked `IAiImageService` responses.
- Targeted integration checks for each chat provider (non-empty, parseable outputs).
- Targeted integration checks for image generation parity through `IAiImageService`.

## Acceptance criteria

- `AIHelper` business outputs remain equivalent for:
  - `GetJokeSceneDescription`
  - `SuggestCategories`
  - `AnalyzeJoke`
- `AIHelper` image outputs remain equivalent for:
  - `GenerateAnImage`
  - `SaveBase64ImageToBlob`
- Provider can be selected by `AppSettings:AiServiceProvider` without code changes.
- Default behavior remains backward compatible (`AgentFramework`).
- Failure modes are clear and non-breaking (graceful error handling retained).

## Rollback

If Copilot SDK provider fails in runtime testing, keep flag on `AgentFramework` and defer switch while retaining abstraction baseline.
