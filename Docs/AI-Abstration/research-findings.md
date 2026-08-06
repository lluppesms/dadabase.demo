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

Image generation paths (Azure OpenAI image client, MAI REST, OpenAI REST) are separate and should remain out of chat abstraction scope.

## Target direction

Introduce a chat abstraction in `/src/web/Website/Services` with two implementations:

1. **Agent Framework service** (wrapper over existing behavior)
2. **Copilot SDK service** (managed identity, Foundry model endpoint)

Then have `AIHelper` select which service is used based on a configuration flag.

## Key technical findings

1. A minimal abstraction should model one turn of completion:
   - input: system prompt + user prompt
   - output: response text
2. This shape cleanly supports all three existing `AIHelper` chat methods while preserving current parsing/business behavior.
3. Copilot SDK path (from reference sample) uses managed identity token acquisition and a provider config against Foundry/OpenAI-compatible endpoint.
4. `AIHelper` can remain responsible for:
   - composing prompt payloads
   - parsing categories/scene
   - image generation and blob storage

## Important uncertainty discovered

Before implementation, validate Copilot SDK viability in the target hosting model with a small spike:

- Confirm BYOK + managed identity token flow against the intended Foundry endpoint.
- Confirm runtime requirements for Copilot SDK process/session handling in this app environment.

This is now a formal gate in the execution plan.
