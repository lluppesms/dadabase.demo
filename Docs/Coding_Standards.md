# DadABase Coding Standards

This document outlines the coding standards and conventions used in the DadABase project. It serves as a reference for maintaining consistency across the codebase.

## Table of Contents
1. [Project Structure](#project-structure)
2. [Naming Conventions](#naming-conventions)
3. [Code Organization](#code-organization)
4. [Documentation](#documentation)
5. [Error Handling](#error-handling)
6. [Testing](#testing)
7. [Infrastructure as Code](#infrastructure-as-code)
8. [GitHub Actions](#github-actions)

## Project Structure

### Solution Organization
- **DadABase.Web** (`src/web/Website/`): Main web application project containing Blazor components, pages, and API controllers.
- **DadABase.Data** (`src/web/Data/`): Class library project for data access, domain models, and repository interfaces/implementations.
- **DadABase.Tests** (`src/web/Tests/`): Test project containing unit and integration tests.

### DadABase.Web Folder Structure
- **API/**: Contains API controllers with RESTful endpoints.
- **Components/**: Reusable Blazor UI components (each with matching `.razor.cs` and `.razor.css` files).
- **Data/**: Static data files (e.g., `Jokes.json`).
- **Helpers/**: Utility and helper classes.
- **Models/**: Application-level models and view models.
  - **AIModels/**: Models for AI/chat completions.
  - **Application/**: Application-level models like `AppSettings`, `BuildInfo`, and `Constants`.
  - **DbContext/**: EF Core `ApplicationDbContext` and identity entities.
  - **ViewModels/**: View-specific models used in pages and components.
- **Pages/**: Blazor pages (each with matching `.razor.cs` and `.razor.css` files).
- **Repositories/**: Service implementations and their interfaces (interfaces co-located with implementations, not in a subfolder).
- **Shared/**: Shared layout components and navigation.
- **wwwroot/**: Static assets (CSS, JS, images).

### DadABase.Data Folder Structure
- **Helpers/**: Shared utility classes.
- **Models/**: Domain entity classes.
- **Repositories/**: Repository interfaces and implementations co-located (e.g., `IJokeRepository.cs` alongside `JokeSQLRepository.cs`).

## Naming Conventions

### General
- Use **PascalCase** for:
  - Class names
  - Method names
  - Property names
  - Enum values
  - Constant names
- Use **camelCase** for:
  - Local variables
  - Method parameters
  - Private fields
- Use **UPPER_CASE** for:
  - Rarely-changed, app-wide constants

### File Naming
- Name files according to their primary class.
- **Razor Components:** `ComponentName.razor` with code-behind as `ComponentName.razor.cs`.
- **Tests:** Use descriptive names that indicate the tested component and type of test (e.g., `Joke_API_Tests.cs`).

### Interfaces
- Prefix interfaces with "I" (e.g., `IJokeRepository`).

### Type Suffixes
- Controllers: `JokeController`
- Repositories: `JokeRepository`
- Repository Interfaces: `IJokeRepository`
- Tests: `Joke_API_Tests`, `JokeRepository_Tests`

## Code Organization

### File Structure
- Begin each file with a copyright header comment.
- Include a summary comment explaining the file's purpose.
- Keep files focused on a single responsibility.

### Classes
- Organize class members in the following order:
  1. Fields
  2. Properties
  3. Constructors
  4. Public methods
  5. Private methods
- Use partial classes for code-behind files (e.g., Blazor components).
- Use Dependency Injection for accessing services.

### Regions (used sparingly)
- Use regions to organize large classes (e.g., `#region Initialization`).
- Don't overuse regions to hide poor code organization.

## Documentation

### Comments
- Use XML documentation comments for public APIs and classes.
- Use `<summary>` tags for method and class descriptions.
- Use `<param>` tags for parameters.
- Use `<returns>` tags for return values.

### Example
```csharp
/// <summary>
/// Retrieves a joke by its unique identifier.
/// </summary>
/// <param name="id">The unique identifier of the joke.</param>
/// <returns>The joke if found; otherwise, null.</returns>
public Joke GetJokeById(int id)
{
    // Implementation
}
```

## Error Handling

### Exceptions
- Use specific exception types rather than generic ones.
- Handle exceptions at the appropriate level of abstraction.
- Log exceptions properly with appropriate logging levels.
- Use custom exceptions where appropriate.

### Validation
- Validate inputs early in the process.
- Return appropriate status codes from API controllers.

## Testing

### Test Folder Structure
- **APITests/**: Tests for API controller endpoints (e.g., `Joke_API_Tests.cs`, `Category_API_Tests.cs`).
- **ModelTests/**: Tests for domain model logic (e.g., `Model_Tests.cs`).
- **RepositoryTests/**: Tests for repository implementations (e.g., `JokeRepository_Tests.cs`).
- **SampleData/**: Test data and data management classes.

### Test Organization
- Organize tests by component type into the appropriate subfolder.
- Use clear naming conventions:
  - Category-based: `Category_API_Tests.cs`, `JokeRepository_Tests.cs`
  - Or method-based: `ClassName_MethodName_ExpectedBehavior`

### Test Base Classes
- Use `BaseTest` for common test setup, data initialization, and mock HTTP context.
- Separate test data from test logic using the `SampleData/` folder.

### Test Data
- Use the `TestingData` partial class strategy: a `TestingDataManager.cs` base with separate per-entity files (e.g., `TestingData_Joke.cs`, `TestingData_JokeCategory.cs`).
- Use `TestingDataModelCoverage.cs` and `TestingDataStoredProcDatasets.cs` for coverage and stored proc tests.
- Keep all sample data separated from test implementation in the `SampleData/` folder.

## Conclusion

These standards are designed to ensure code consistency, readability, and maintainability across the DadABase project. Following these guidelines will help maintain code quality and make collaboration more effective.

---

*This document was generated based on the code analysis of the DadABase project as of March 31, 2026.*
