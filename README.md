# 🤣 The Dad-A-Base

> *Where does a geeky Dad store all of his Dad jokes? In a dad-a-base, of course!*

![Dad Joke Level: Expert](https://img.shields.io/badge/Dad%20Joke%20Level-Expert-gold?style=for-the-badge&logo=laughing)
![Groan Factor](https://img.shields.io/badge/Groan%20Factor-Maximum-purple?style=for-the-badge)
![.NET 10](https://img.shields.io/badge/.NET-10-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)

---

## 🎯 What Is This Masterpiece?

This isn't just a repository. This is a **monument to dad jokes** and a shrine to DevOps best practices, all wrapped in one glorious package. It's the kind of project that makes you say, *"I didn't know I needed this, but now I can't live without it."*

Want to see every cutting-edge development and automation practice demonstrated through the lens of corny humor? **You're in the right place.**

![Architecture](https://img.shields.io/badge/Architecture-🏗️%20Over--Engineered%20Perfection-blue?style=flat-square)

---

## 🤔 Why Does This Exist?

Because when you combine:
- 🎯 A passion for clean code
- 😄 An unhealthy collection of dad jokes
- 🚀 A need to demonstrate DevOps best practices
- 🤖 Agents begging for a place to call home

...you get this magnificent repository.

**Perfect for:**
- 📚 Learning modern .NET development
- 🏗️ Understanding Infrastructure as Code
- 🔄 Studying CI/CD pipeline patterns
- 🗃️ Learning how to SQL schema and data with code
- 🤖 Seeing how you can integrate AI into an existing app
- 😂 Telling terrible jokes at work

---

## 🚀 Things This Repo Demonstrates

| Technology | Description | Status |
|------------|-------------|--------|
| 🔥 **.NET 10 Blazor App** | A beautiful, interactive web app that serves dad jokes with style | ![Production Ready](https://img.shields.io/badge/-Production%20Ready-success) |
| ⚡ **Azure Function** | Serverless dad joke API - because jokes should be scalable | ![Flex Consumption](https://img.shields.io/badge/-Flex%20Consumption-blue) |
| 💻 **Console App** | For when you need jokes in your terminal (we don't judge) | ![CLI Jokes](https://img.shields.io/badge/-CLI%20Jokes-yellow) |
| 🗃️ **SQL Automation** | Schema + seed data deployed via SQL DACPAC because jokes need a a parent before they are fully groan | ![Schema Migration](https://img.shields.io/badge/-Schema%20Migration-lightgrey) |
| 🤖 **AI Agent Integration** | Seeing how you can integrate AI into an existing app | ![Hallucination%20Free](https://img.shields.io/badge/-Hallucination%20Free-brightgreen) |
| 🏗️ **Bicep IaC** | Full Azure resource deployment - infrastructure so clean it sparkles | ![100% Declarative](https://img.shields.io/badge/-100%25%20Declarative-informational) |
| 🔄 **Azure DevOps Pipelines** | Full CI/CD pipelines built with reusable templates | ![Modular](https://img.shields.io/badge/-Modular%20Templates-orange) |
| 🐙 **GitHub Actions** | Because we support *all* the CI/CD platforms | ![Multi-Platform](https://img.shields.io/badge/-Multi--Platform-blueviolet) |
| 🔍 **Code Scanning** | Security scanning to keep the jokes safe from hackers | ![Secure](https://img.shields.io/badge/-Secure-red) |
| 🎭 **Playwright Testing** | Automated smoke tests that actually click buttons | ![End-to-End](https://img.shields.io/badge/-End--to--End-9cf) |
| ✅ **Unit Testing** | With code coverage, because untested jokes aren't funny | ![High Coverage](https://img.shields.io/badge/-High%20Coverage-brightgreen) |
| 🪝 **Pre-Commit Hooks** | Auto-format C# and scan for secrets before every commit | ![Developer Experience](https://img.shields.io/badge/-Developer%20Experience-teal) |

---

##  Features That'll Make You Smile

### 🌐 The Blazor Web App
- 🎲 **Random Joke Generator** - Never run out of material at parties
- 🔍 **Search API** - Find the perfect joke for any occasion  
- 📂 **Category Browser** - Dad jokes, organized *scientifically*
- 🤖 **AI Integration** - Generate joke categories and images with GenAI magic

### ⚡ The Azure Function
- 🚀 Serverless dad jokes that scale to infinity
- 📊 OpenAPI/Swagger support for the API purists
- 💪 Built on .NET 10 Isolated Worker

### 🏗️ Infrastructure as Code
- 🎯 **Bicep templates** that deploy entire environments with one command
- 🔐 **Managed Identity** support - no passwords in config files!
- 📊 **Application Insights** - because we need to monitor joke performance
- 🗄️ **Azure SQL** - enterprise-grade joke storage

### 🗃️ SQL Database as Code
- 📝 **Complete schema defined in code** - tables, views, stored procedures, all versioned
- 📦 **DACPAC deployment** - database changes deployed via automated pipelines
- 🌱 **Seed data included** - pre-loaded with quality dad jokes for immediate use
- 🔄 **CI/CD integration** - schema changes flow through the same pipeline as application code
- ✅ **Schema validation** - ensures database integrity before deployment

### 🎭 Playwright Automated Testing
- 🤖 **End-to-end UI testing** - automated browser tests validate the entire user experience
- 🔄 **Pipeline integration** - tests run automatically after deployment
- 🌐 **Multi-browser support** - verify functionality across different browsers
- 📊 **API testing included** - validate both UI and backend endpoints
- ✅ **Post-deployment validation** - ensure jokes are actually being delivered correctly

---

## 🎬 Quick Start (local test run)

```bash
# Clone the repo
git clone https://github.com/lluppesms/dadabase.demo.git

# Install npm dependencies (also activates pre-commit hooks)
npm install

# Navigate to the web project
cd src/web/Website

# Run the Blazor app
dotnet run

# Open browser and enjoy the dad jokes!
```

---

## 🛠️ Developer Setup (Pre-Commit Hooks)

This repo uses [Husky](https://typicode.github.io/husky/) to run pre-commit checks automatically before every `git commit`:

- 🔍 **Secret scanning** via [gitleaks](https://github.com/gitleaks/gitleaks) — blocks commits containing API keys, tokens, or credentials
- 🎨 **Auto-formatting** via `dotnet format` — fixes C# whitespace/style violations and re-stages the corrected files

### One-time setup

1. **Install gitleaks** (required for secret scanning):
   - Windows: `winget install gitleaks`
   - macOS: `brew install gitleaks`

2. **Run `npm install`** — this activates the Husky hooks automatically via the `prepare` script.

That's it! The hooks run on every commit from then on. See [CONTRIBUTING.md](./CONTRIBUTING.md) for full details.

---

## 🔧 Developer Setup

This repo uses [Husky](https://typicode.github.io/husky/) pre-commit hooks to automatically format C# code and scan for secrets before every commit. Two one-time setup steps are required:

### 1. Install gitleaks

The secret scanner must be installed separately on each developer machine:

| Platform | Command |
|----------|---------|
| 🪟 **Windows** | `winget install gitleaks` |
| 🍎 **macOS** | `brew install gitleaks` |
| 🐧 **Linux** | See [gitleaks releases](https://github.com/gitleaks/gitleaks/releases) |

> If `gitleaks` is not found, the hook will skip secret scanning with a warning — it won't block your commit.

### 2. Activate Husky hooks

```bash
npm install
```

That's it! The `prepare` script in `package.json` activates the hooks automatically. From this point on, every `git commit` will:

1. 🔍 **Scan for secrets** using gitleaks (skipped gracefully if not installed)
2. 🎨 **Auto-format staged `.cs` files** using `dotnet format` across all solution files
3. ✅ **Re-stage any auto-formatted files** so they're included in your commit

See [CONTRIBUTING.md](./CONTRIBUTING.md) for full contributor guidelines.

---

## 🤖 GenAI-Powered Features

This application has been supercharged with **Generative AI capabilities** to bring your dad jokes to life:

### 🎨 AI Image Generation
When viewing a joke, the app uses AI to:
1. 📝 **Analyze the joke content** and understand the humor
2. 🖼️ **Generate a visual scene description** that captures the essence of the joke
3. 🎨 **Create an AI-generated image** that illustrates the joke visually (*using the brand new MAI-Image-2 model!*)

Transform text-based dad jokes into visual masterpieces! Each joke can have its own unique, AI-generated illustration that brings the punchline to life.

### 🏷️ Automatic Category Assignment
When creating a **new joke**, the AI assistant works behind the scenes to:
- 🧠 **Analyze the joke content** to understand themes, topics, and humor style
- 🔍 **Intelligently assign relevant categories** automatically
- ⚡ **Save you time** - no manual categorization needed!

Just write your joke, and the AI figures out whether it's a pun, a knock-knock joke, animal humor, food-related, or any other category. It's like having a comedy curator in your pocket!

> **Powered by Azure Foundry** - Because even dad jokes deserve enterprise-grade AI

---

## 🤖 GitHub Copilot Agents and Skills

All of the GitHub Copilot Agents and Skills that used to live in this repo have relocated to a dedicated repository for better maintenance and discoverability. You could say they... moved to a better *repo-hood*. 🏘️

Check out the [my.copilot.skills](https://github.com/lluppesms/my.copilot.skills) repo to see the full collection of AI agents and skills that can be used in this project.

### Loading in VS Code

Load the [VS Code Workspace](./dadabase.demo.gh.code-workspace) to automatically make all shared skills available to GitHub Copilot Chat. The workspace file includes both this repo and the `my.copilot.skills` repo as workspace folders, so Copilot Chat discovers the skills from `.github/skills/` in both repositories.

### Loading in GitHub Copilot CLI

The CLI is a little different and does not support the VS Code Workspace. For details on loading skills into the GitHub Copilot CLI (and what to do about gents, and instructions), see the [Copilot CLI Skills Guide](./Docs/Copilot_CLI_Skills.md).

---

## 🚀 Deployment Options

Choose your adventure:

| Method | Documentation | Difficulty |
|--------|---------------|------------|
| 🔄 **Azure DevOps** | [Pipeline Guide](./.azdo/pipelines/readme.md) | ⭐⭐⭐ |
| 🐙 **GitHub Actions** | [Actions Guide](./.github/workflows-readme.md) | ⭐⭐⭐ |
| ⌨️ **AZD CLI** | [AZD Guide](./.azure/readme.md) | ⭐⭐ |

[![azd Compatible](/Docs/images/AZD_Compatible.png)](/.azure/readme.md)

---

## 🔄 CI/CD Pipelines and Actions Showcase

### Azure DevOps Pipelines
Our Azure DevOps pipelines are like a well-oiled machine... if that machine told puns:

| Pipeline | Purpose |
|----------|---------|
| 🏗️ `deploy-bicep` | Create all Azure resources |
| 🌐 `build-deploy-webapp` | Build, test, and deploy the Blazor app to standard Azure App Service |
| 📦 `build-deploy-containerapp` | Build, test, and deploy the Blazor app to Azure Container Apps |
| ⚡ `build-deploy-function` | Ship the serverless jokes |
| 🗃️ `build-deploy-dacpac` | Deploy SQL schema and seed data |
| 🔍 `scan-code` | Security scanning (serious stuff) |
| 🎭 `smoke-test-webapp` | Make sure the jokes are actually funny (automated) |

### GitHub Actions
Same great taste, GitHub flavor:

| Workflow | Badges |
|----------|--------|
| Deploy Infrastructure | [![deploy-bicep](https://github.com/lluppesms/dadabase.demo/actions/workflows/1-deploy-bicep.yml/badge.svg)](https://github.com/lluppesms/dadabase.demo/actions/workflows/1-deploy-bicep.yml) |
| Build & Deploy Web App (App Service) | [![bicep-build-deploy-webapp](https://github.com/lluppesms/dadabase.demo/actions/workflows/2.1-bicep-build-deploy-webapp.yml/badge.svg)](https://github.com/lluppesms/dadabase.demo/actions/workflows/2.1-bicep-build-deploy-webapp.yml) |
| Build & Deploy Web App (Container App) | [![bicep-build-deploy-containerapp](https://github.com/lluppesms/dadabase.demo/actions/workflows/2.2-bicep-build-deploy-containerapp.yml/badge.svg)](https://github.com/lluppesms/dadabase.demo/actions/workflows/2.2-bicep-build-deploy-containerapp.yml) |
| Deploy DACPAC | [![build-deploy-dacpac](https://github.com/lluppesms/dadabase.demo/actions/workflows/4-build-deploy-dacpac.yml/badge.svg)](https://github.com/lluppesms/dadabase.demo/actions/workflows/4-build-deploy-dacpac.yml) |
| Code Scanning | [![scan-code](https://github.com/lluppesms/dadabase.demo/actions/workflows/7-scan-code.yml/badge.svg)](https://github.com/lluppesms/dadabase.demo/actions/workflows/7-scan-code.yml) |

---

## 🏛️ The Grand Architecture

```
📁 Dad-A-Base Repository
├── 🌐 src/web/           → .NET 10 Blazor App (the star of the show)
├── ⚡ src/function/       → Azure Function (serverless joke delivery)
├── 💻 src/console/        → Console App (for joke connoisseurs)
├── 📊 src/sql.database/   → SQL Database Project (DACPAC central)
├── 🏗️ infra/Bicep/        → Infrastructure as Code (Bicep flexing)
├── 🔄 .azdo/pipelines/    → Azure DevOps CI/CD (YAML wizardry)
├── 🐙 .github/workflows/  → GitHub Actions (also YAML wizardry)
└── 🎭 playwright/         → Automated testing (robot comedy critics)
```

---

## 🧪 Testing Philosophy

> *"A dad joke without tests is just a dad statement."* - Ancient DevOps Proverb

- ✅ **Unit Tests** with MSTest and Coverlet for code coverage
- 🎭 **Playwright Tests** for end-to-end UI validation
- 📊 **Test results and Code coverage** integrated directly into CI/CD pipelines - because metrics matter

---

## 📚 Documentation

| Topic | Link |
|-------|------|
| 📖 Coding Standards | [Coding_Standards.md](./Docs/Coding_Standards.md) |
| 🏗️ Infrastructure as Code | [Infra_As_Code.md](./Docs/Infra_As_Code.md) |
| 🗃️ SQL DACPAC Deployment | [SQL-DacPac.md](./Docs/SQL-DacPac.md) |
| 🔄 Azure DevOps Pipelines | [YML_AzDO.md](./Docs/YML_AzDO.md) |
| 🐙 GitHub Actions | [YML_GitHub.md](./Docs/YML_GitHub.md) |
| 🤝 Contributing & Dev Setup | [CONTRIBUTING.md](./CONTRIBUTING.md) |
| 🤝 Contributing Guide | [CONTRIBUTING.md](./CONTRIBUTING.md) |

---

## 🤝 Contributing

Found a bug? Want to add a feature? Have an even worse dad joke?

Pull requests are welcome! Check out [CONTRIBUTING.md](./CONTRIBUTING.md) for developer setup (including pre-commit hooks), code style, and PR guidelines. Just remember: if your PR doesn't make at least one person groan, is it really worth it?

See [CONTRIBUTING.md](./CONTRIBUTING.md) for developer setup and contribution guidelines.

---

## 📜 License

[MIT](./LICENSE) - Because dad jokes should be free for everyone.

---

<div align="center">

[![Open in vscode.dev](https://img.shields.io/badge/Open%20in-vscode.dev-blue?style=for-the-badge)][1]

[1]: https://vscode.dev/github/lluppesms/dadabase.demo/

---

*Made with 💚 and an excessive amount of groaning*

**Remember: Good code and good jokes both require timing.**

</div>
