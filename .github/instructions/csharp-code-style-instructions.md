---
applyTo: "**/*.cs,**/*.razor"
---

# C# Code Style Instructions

## General Code Style

- Prefer `async`/`await` over direct `Task` handling
- Use nullable reference types
- Use `var` over explicit type declarations
- Always implement `IDisposable` when dealing with event handlers or subscriptions
- Use latest C# features (e.g., records, pattern matching, expression-bodied members)
- Use consistent naming conventions: `PascalCase` for public members, `camelCase` for private members
- Use meaningful names for variables, methods, and classes
- Use dependency injection for services and components
- Use interfaces for service contracts and put them in a unique file
- Use file-scoped namespaces in C# — namespaces are PascalCased
- Prefer explicit access modifiers (`public`, `private`, etc.) for all members

## Using Directives

- Place `using` directives at the top of files, outside namespaces
- If a `using` directive is used in more than one file, move it to a `globalUsings.cs` file in the project root
- Organize `using` directives:
  - Put `System` namespaces first
  - Put `Microsoft` namespaces second
  - Put application namespaces last
  - Remove unused `using` directives
  - Sort `using` directives alphabetically within each group

## Namespace & Folder Structure

- Organize code into clear namespaces reflecting folder structure (e.g., `DadABase.Web`, `DadABase.Tests`)
- Group related files into folders: `API`, `Components`, `Data`, `Helpers`, `Models`, `Pages`, `Repositories`, `Shared`
- Keep test code in dedicated test projects/folders using clear naming (e.g., `Category_API_Tests.cs`)
