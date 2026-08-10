# Product

<!-- impeccable:product-schema 1 -->

## Platform

web

## Users

- People looking for, browsing, and sharing dad jokes, whether they need a quick laugh or a reliably groan-worthy punchline.
- Developers and technical teams evaluating a practical reference implementation for modern .NET, Azure, AI integration, infrastructure as code, CI/CD, and automated testing.

## Product Purpose

Dad-A-Base is a playful dad-joke application and an engineering demonstration repository. It lets visitors discover random jokes, search by text or category, and, when authorized, use AI-assisted features. It also makes the surrounding application architecture and delivery practices inspectable for developers. Success means a visitor can quickly find a joke while a technical evaluator can trace how the working product demonstrates its engineering practices.

## Positioning

Dad-A-Base gives a real, usable dad-joke experience equal footing with an open, end-to-end .NET and Azure implementation, so the product itself is the demonstration rather than a disposable sample.

## Operating Context

- Public visitors use the Blazor web app to fetch a random joke or search the joke collection by phrase and category.
- Authenticated administrators manage joke content and can access AI-assisted category and image-generation workflows.
- Developers use the repository and its running app to explore .NET 10, Azure Functions, Azure SQL or JSON fallback storage, Bicep, CI/CD, Playwright, and AI integrations.

## Capabilities and Constraints

- Anonymous browsing is supported; administrative management is restricted to authorized users.
- The web application is a .NET 10 Blazor Server app using MudBlazor.
- Joke data can use a JSON fallback or Azure SQL Database.
- AI-assisted image generation and category assignment depend on configured Azure AI services and authorized access.
- The repository includes Azure Functions, MCP servers, infrastructure as code, and automated tests alongside the web application.

## Brand Commitments

- Product name: Dad-A-Base.
- Voice: playful, knowingly corny, and welcoming without obscuring the real product behavior.
- Preserve the product's dual identity as a useful joke destination and a credible engineering demonstration.

## Evidence on Hand

- Seed joke data: `src/web/Website/Data/Jokes.json`.
- Public pages for random jokes, search, details, and product information, plus protected administration and editing surfaces.
- Repository documentation and runnable source demonstrate the architecture and delivery practices; future work must not fabricate customer claims, performance benchmarks, or AI capabilities beyond configured services.

## Product Principles

- Let visitors reach a joke quickly and without an account.
- Let the humor be deliberate while the product behavior remains clear.
- Demonstrate engineering practices through a working, inspectable product.
- Keep administrative and AI-assisted actions appropriately protected.
- Support local demonstration and cloud-backed operation through clear fallback paths.