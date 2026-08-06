# Research Findings: Agent Framework to Copilot SDK

## Repository context reviewed

- Current logic: `/home/runner/work/dadabase.demo/dadabase.demo/src/web/Website/Repositories/AIHelper.cs`
- DI registration: `/home/runner/work/dadabase.demo/dadabase.demo/src/web/Website/Program.cs`
- Web project packages: `/home/runner/work/dadabase.demo/dadabase.demo/src/web/Website/DadABase.Web.csproj`
- Reference sample: `lluppesms/simple.ghcp.sdk.byok`

## Current state in this app

`AIHelper` currently performs chat-model calls using Microsoft Agent Framework and Azure OpenAI:

- Builds chat client from Azure OpenAI endpoint/deployment settings.
- Creates `AIAgent` instances with prompt instructions for:
  - joke scene description
  - category classification
  - combined analyzer output
- Calls `RunAsync(...)` and then parses string output.

Image generation paths (Azure OpenAI image client, MAI REST, OpenAI REST) should be moved behind a separate image abstraction so `AIHelper` can delegate generation responsibilities similarly to chat.

## Target direction

Introduce service abstractions in `/src/web/Website/Services`:

1. **`IAiChatService`**
   - **Agent Framework service** (wrapper over existing behavior)
   - **Copilot SDK service** (managed identity, Foundry model endpoint; based on validated sample)
2. **`IAiImageService`**
   - Wrapper over existing image generation flows (Azure OpenAI image client, MAI REST, OpenAI REST)

Then have `AIHelper` select the chat provider by configuration flag and route image generation through `IAiImageService`.

## Key technical findings

1. A minimal abstraction should model one turn of completion:
   - input: system prompt + user prompt
   - output: response text
2. This shape cleanly supports all three existing `AIHelper` chat methods while preserving current parsing/business behavior.
3. Copilot SDK path (from reference sample `src/web/Services/GHCP_SDK_Service.cs`) uses managed identity token acquisition and a provider config against Foundry/OpenAI-compatible endpoint.
4. `AIHelper` can remain responsible for:
   - composing prompt payloads
   - parsing categories/scene
   - orchestration and parsing/business outputs
5. `IAiImageService` can own image generation details while preserving existing blob-storage behavior and return shape currently expected by `AIHelper`.

## Validation status and implementation implication

Copilot SDK viability is treated as validated from the referenced sample repository and service implementation:

- `https://github.com/lluppesms/simple.ghcp.sdk.byok`
- `src/web/Services/GHCP_SDK_Service.cs`

Implementation can proceed directly to abstraction and provider integration work without a separate pre-implementation spike gate.
