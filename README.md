# 🤣 The Dad-A-Base

> *Where does a geeky Dad store all of his Dad jokes? In a dad-a-base, of course!*

![Dad Joke Level: Expert](https://img.shields.io/badge/Dad%20Joke%20Level-Expert-gold?style=for-the-badge&logo=laughing)
![Groan Factor](https://img.shields.io/badge/Groan%20Factor-Maximum-purple?style=for-the-badge)
![.NET 10](https://img.shields.io/badge/.NET-10-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)

---

## 🎯 What Is This Masterpiece?

This isn't just a repository. This is a **monument to dad jokes** and a shrine to DevOps best practices, all wrapped in one glorious package. It's the kind of project that makes you say, *"I didn't know I needed this, but now I can't live without it."*

Want to see every cutting-edge development practice demonstrated through the lens of corny humor? **You're in the right place.**

![Architecture](https://img.shields.io/badge/Architecture-🏗️%20Over--Engineered%20Perfection-blue?style=flat-square)

---

## 🚀 What This Repo Demonstrates

| Technology | Description | Status |
|------------|-------------|--------|
| 🔥 **.NET 10 Blazor App** | A beautiful, interactive web app that serves dad jokes with style | ![Production Ready](https://img.shields.io/badge/-Production%20Ready-success) |
| ⚡ **Azure Function** | Serverless dad joke API - because jokes should be scalable | ![Flex Consumption](https://img.shields.io/badge/-Flex%20Consumption-blue) |
| 💻 **Console App** | For when you need jokes in your terminal (we don't judge) | ![CLI Jokes](https://img.shields.io/badge/-CLI%20Jokes-yellow) |
| 🏗️ **Bicep IaC** | Full Azure resource deployment - infrastructure so clean it sparkles | ![100% Declarative](https://img.shields.io/badge/-100%25%20Declarative-informational) |
| ✅ **Unit Testing** | With code coverage, because untested jokes aren't funny | ![High Coverage](https://img.shields.io/badge/-High%20Coverage-brightgreen) |
| 🔄 **Azure DevOps Pipelines** | Full CI/CD pipelines built with reusable templates | ![Modular](https://img.shields.io/badge/-Modular%20Templates-orange) |
| 🐙 **GitHub Actions** | Because we support *all* the CI/CD platforms | ![Multi-Platform](https://img.shields.io/badge/-Multi--Platform-blueviolet) |
| 🔍 **Code Scanning** | Security scanning to keep the jokes safe from hackers | ![Secure](https://img.shields.io/badge/-Secure-red) |
| 🎭 **Playwright Testing** | Automated smoke tests that actually click buttons | ![End-to-End](https://img.shields.io/badge/-End--to--End-9cf) |
| 🗃️ **SQL DACPAC Deploy** | Schema + seed data deployment because jokes need a home | ![Schema Migration](https://img.shields.io/badge/-Schema%20Migration-lightgrey) |

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

## 🎪 Features That'll Make You Smile

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

---

## 🔄 CI/CD Pipeline Showcase

### Azure DevOps Pipelines
Our Azure DevOps pipelines are like a well-oiled machine... if that machine told puns:

| Pipeline | Purpose |
|----------|---------|
| 🏗️ `deploy-bicep` | Create all Azure resources |
| 🌐 `build-deploy-webapp` | Build, test, and deploy the Blazor app |
| ⚡ `build-deploy-function` | Ship the serverless jokes |
| 🗃️ `build-deploy-dacpac` | Deploy SQL schema and seed data |
| 🔍 `scan-code` | Security scanning (serious stuff) |
| 🎭 `smoke-test-webapp` | Make sure the jokes are actually funny (automated) |

### GitHub Actions
Same great taste, GitHub flavor:

| Workflow | Badges |
|----------|--------|
| Deploy Infrastructure | [![deploy-bicep](https://github.com/lluppesms/dadabase.demo/actions/workflows/1-deploy-bicep.yml/badge.svg)](https://github.com/lluppesms/dadabase.demo/actions/workflows/1-deploy-bicep.yml) |
| Build & Deploy Web App | [![bicep-build-deploy-webapp](https://github.com/lluppesms/dadabase.demo/actions/workflows/3-bicep-build-deploy-webapp.yml/badge.svg)](https://github.com/lluppesms/dadabase.demo/actions/workflows/3-bicep-build-deploy-webapp.yml) |
| Deploy DACPAC | [![build-deploy-dacpac](https://github.com/lluppesms/dadabase.demo/actions/workflows/4-build-deploy-dacpac.yml/badge.svg)](https://github.com/lluppesms/dadabase.demo/actions/workflows/4-build-deploy-dacpac.yml) |
| Code Scanning | [![scan-code](https://github.com/lluppesms/dadabase.demo/actions/workflows/7-scan-code.yml/badge.svg)](https://github.com/lluppesms/dadabase.demo/actions/workflows/7-scan-code.yml) |

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

## 🧪 Testing Philosophy

> *"A dad joke without tests is just a dad statement."* - Ancient DevOps Proverb

- ✅ **Unit Tests** with MSTest and Coverlet for code coverage
- 🎭 **Playwright Tests** for end-to-end UI validation
- 📊 **Test results** integrated directly into CI/CD pipelines
- 🔍 **Code coverage reports** because metrics matter

---

## 📚 Documentation

| Topic | Link |
|-------|------|
| 📖 Coding Standards | [Coding_Standards.md](./Docs/Coding_Standards.md) |
| 🏗️ Infrastructure as Code | [Infra_As_Code.md](./Docs/Infra_As_Code.md) |
| 🗃️ SQL DACPAC Deployment | [SQL-DacPac.md](./Docs/SQL-DacPac.md) |
| 🔄 Azure DevOps Pipelines | [YML_AzDO.md](./Docs/YML_AzDO.md) |
| 🐙 GitHub Actions | [YML_GitHub.md](./Docs/YML_GitHub.md) |

---

## 🤔 Why Does This Exist?

Because when you combine:
- 🎯 A passion for clean code
- 😄 An unhealthy collection of dad jokes
- 🚀 A need to demonstrate DevOps best practices

...you get this magnificent repository.

**Perfect for:**
- 📚 Learning modern .NET development
- 🏗️ Understanding Infrastructure as Code
- 🔄 Studying CI/CD pipeline patterns
- 😂 Telling terrible jokes at work

---

## 🎬 Quick Start

```bash
# Clone the repo
git clone https://github.com/lluppesms/dadabase.demo.git

# Navigate to the web project
cd src/web/Website

# Run the Blazor app
dotnet run

# Open browser and enjoy the dad jokes!
```

---

## 🤝 Contributing

Found a bug? Want to add a feature? Have an even worse dad joke?

Pull requests are welcome! Just remember: if your PR doesn't make at least one person groan, is it really worth it?

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
