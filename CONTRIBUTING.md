# 🤝 Contributing to Dad-A-Base

> *"If your PR doesn't make at least one person groan, is it really worth it?"*

Thank you for contributing to this monument to dad jokes and DevOps best practices! This guide covers everything you need to get set up and contributing effectively.

---

## 🔧 Developer Setup

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Node.js 18+](https://nodejs.org/) (for Playwright tests and pre-commit hooks)
- [Git](https://git-scm.com/)

### Step 1: Clone and install dependencies

```bash
git clone https://github.com/lluppesms/dadabase.demo.git
cd dadabase.demo
npm install
```

The `npm install` command automatically activates the [Husky](https://typicode.github.io/husky/) pre-commit hooks via the `prepare` script.

### Step 2: Install gitleaks

The secret scanner must be installed separately on each machine:

| Platform | Command |
|----------|---------|
| 🪟 **Windows** | `winget install gitleaks` |
| 🍎 **macOS** | `brew install gitleaks` |
| 🐧 **Linux** | See [gitleaks releases](https://github.com/gitleaks/gitleaks/releases) |

> If `gitleaks` is not found, the hook will skip secret scanning with a warning but will **not** block your commit. Installing it is strongly recommended.

---

## 🪝 Pre-Commit Hooks

Every `git commit` automatically runs two checks:

### 1. 🔍 Secret Scanning (gitleaks)

Scans staged files for accidental secrets — API keys, connection strings, passwords, tokens. If secrets are detected, the commit is blocked. Review the output, remove the secrets, and commit again.

Configuration is in [`.gitleaks.toml`](./.gitleaks.toml). It uses the default gitleaks ruleset with allowlists for:
- `package-lock.json` (contains many hash-like strings)
- `*.lock` files
- `*.env.example` files (intentionally contain placeholder values)

### 2. 🎨 C# Code Formatting (dotnet format)

Automatically formats any staged `.cs` files to match the project code style using `dotnet format`. Formatted files are re-staged automatically so your commit always contains clean code.

This runs against all solution files:
- `src/web/dadabase.net10.web.sln`
- `src/function/dadabase.net10.function.sln`
- `src/mcp/DadJokeMCP.sln`
- `src/console/DadJoke.console.sln`
- `src/analyzer/DadJokeAnalyzer.sln`

> You can also run `dotnet format <solution>` manually at any time.

---

## 🚀 Running the App Locally

```bash
# Blazor web app
cd src/web/Website
dotnet run
```

---

## 🎭 Running Playwright Tests

```bash
# Install browsers (first time only)
npx playwright install

# Run all tests
npx playwright test

# Run with UI
npx playwright test --ui
```

---

## 🧪 Running Unit Tests

```bash
cd src/web
dotnet test
```

---

## 📐 Code Style

- Follow the conventions in [Docs/Coding_Standards.md](./Docs/Coding_Standards.md)
- C# code is auto-formatted by `dotnet format` on commit — no manual formatting needed
- Blazor components use scoped CSS (`.razor.css` files)
- Use `var` over explicit type declarations
- Prefer `async`/`await` over direct `Task` handling

---

## 📦 Pull Request Checklist

- [ ] Code builds without errors (`dotnet build`)
- [ ] Unit tests pass (`dotnet test`)
- [ ] No secrets in staged files (enforced by gitleaks hook)
- [ ] Code is formatted (enforced by `dotnet format` hook)
- [ ] PR description explains what changed and why

---

## 📜 License

By contributing, you agree your contributions will be licensed under the [MIT License](./LICENSE).