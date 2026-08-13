# 📝 Open-Source Task & Bug Tracking - Designed for Simplicity.

A web-based **Task, Change Request (CR), and Bug Management System** built with **ASP.NET Core MVC (.NET 8)**, using **Microsoft SQL Server** as the database.

This system helps teams manage tasks, track bugs, handle change requests, and organize projects efficiently.

[![CI](https://github.com/Taskist/taskist/actions/workflows/ci.yml/badge.svg)](https://github.com/Taskist/taskist/actions/workflows/ci.yml)
![License](https://img.shields.io/github/license/taskist/taskist)
![Issues](https://img.shields.io/github/issues/taskist/taskist)
![Stars](https://img.shields.io/github/stars/taskist/taskist)
![.NET](https://img.shields.io/badge/.NET-8.0-512BD4)

## 🚀 Live Demo

Experience Taskist in action: 👉 [**demo.taskist.org**](https://demo.taskist.org)

## 📚 Documentation

Looking for detailed setup guides, module explanations, and screenshots?  
Check out the complete **Taskist Documentation Site** below 👇  

🔗 **Visit:** 👉 [**docs.taskist.org**](https://docs.taskist.org)

## 💬 Join the Discussion on Zulip

Stay connected with the Taskist community on **[Zulip](https://taskist.zulipchat.com)** — our hub for collaboration, support, and ideas.

[![Chat on Zulip](https://img.shields.io/badge/chat-on%20Zulip-3C7EBB?logo=zulip&logoColor=white)](https://taskist.zulipchat.com)

| Channel            | Purpose                                                           | Link                                                                          |
| ------------------ | ----------------------------------------------------------------- | ----------------------------------------------------------------------------- |
| 📢 **General**     | General discussions, announcements, and community updates.        | [Join #general](https://taskist.zulipchat.com/#narrow/channel/539615-general) |
| 💻 **Development** | Development discussions, code reviews, and architecture planning. | [Join #dev](https://taskist.zulipchat.com/#narrow/channel/539614-dev)         |
| 🛠️ **Support**     | Get help setting up Taskist, report bugs, or troubleshoot issues. | [Join #support](https://taskist.zulipchat.com/#narrow/channel/539613-support) |

📝 **Tip:**  
You can join directly with your GitHub or email account — no setup required.  
All discussions are public and searchable to help new contributors learn quickly.

## 📑 Table of Contents

1. [✨ Features](#-features)
2. [🏛 Master Modules](#-master-modules)
3. [⚡ Transaction Modules](#-transaction-modules)
4. [🗂 Module Hierarchy](#-module-hierarchy)
5. [🛠 Technology Stack](#-technology-stack)
6. [🐳 Quick Start with Docker](#-quick-start-with-docker)
7. [🚀 Local Development Setup](#-local-development-setup)
8. [⚙️ Configuration](#-configuration)
9. [💾 Database Migrations](#-database-migrations)
10. [🔐 Security](#-security)
11. [🐛 GitHub Issues & Contribution](#-github-issues--contribution)
12. [📄 License](#-license)

## ✨ Features

- 👤 User authentication and authorization with roles & permissions.
- 🏢 Client and project management.
- 🐞 Task, Change Request, and Bug tracking with status, severity, and reporter.
- 📆 Sprint and backlog management.
- 🧩 Modular system design with configurable menus and settings.
- 📊 Full audit and tracking for project activities.

## 🏛 Master Modules

| Module                      | Description                                                         |
| --------------------------- | ------------------------------------------------------------------- |
| 👤 User                     | System users who can create or manage tasks.                        |
| 🔑 User Roles & Permissions | Define roles (Admin, Manager, Developer, Tester) and access rights. |
| 🏢 Client                   | Organizations or clients associated with projects.                  |
| 📁 Project                  | Projects under a client.                                            |
| 🧩 Module                   | Main functional modules of a project.                               |
| 🔹 SubModule                | Sub-divisions under each module.                                    |
| 📝 Reporter                 | Person reporting a task, bug, or CR.                                |
| ⚠️ Severity                 | Priority/impact of tasks/bugs (High, Medium, Low).                  |
| 🔄 Status                   | Current status of a task (Open, In Progress, Closed, etc.).         |
| 🗂 TaskType                  | Type of work (Task, Bug, CR).                                       |
| 📜 Menu                     | Configurable navigation menu items.                                 |
| ⚙️ Setting                  | Application or system-wide settings.                                |

## ⚡ Transaction Modules

| Module     | Description                          |
| ---------- | ------------------------------------ |
| 📋 Backlog | Manage pending tasks, CRs, and bugs. |
| 🏃 Sprint  | Plan, track, and close sprints.      |

## 🗂 Module Hierarchy

```
Master Modules
├─ User
├─ User Roles & Permissions
├─ Client
├─ Project
├─ Module
│  └─ SubModule
├─ Reporter
├─ Severity
├─ Status
├─ TaskType
├─ Menu
└─ Setting

Transaction Modules
├─ Backlog
└─ Sprint
```

💡 **Note:** Master modules define core entities. Transaction modules handle activities/records based on master data.

## 🛠 Technology Stack

- **Backend:** ASP.NET Core MVC (.NET 8)
- **Frontend:** Razor Views, Bootstrap
- **Database:** Microsoft SQL Server 2019+
- **ORM:** Entity Framework Core 8
- **Background jobs:** Hangfire
- **Validation:** FluentValidation
- **Containers:** Docker & Docker Compose

## 🐳 Quick Start with Docker

The fastest way to try Taskist. This starts SQL Server, applies migrations, seeds reference data and runs the app.

**Prerequisites:** [Docker Desktop](https://www.docker.com/products/docker-desktop/) (or Docker Engine with Compose v2).

```bash
git clone https://github.com/Taskist/taskist.git
cd taskist

# 1. Create your environment file
cp .env.example .env

# 2. Generate an encryption key and paste it into .env
openssl rand -base64 32

# 3. Start everything
docker compose up -d
```

Open **http://localhost:8080** and sign in:

| Email               | Password      |
| ------------------- | ------------- |
| `admin@taskist.org` | `Admin@12345` |

> [!WARNING]
> Change this password immediately after the first sign-in, and never expose the seeded account on a reachable network. Taskist upgrades the stored hash to PBKDF2 automatically the first time this account signs in.

Useful commands:

```bash
docker compose logs -f web     # follow application logs
docker compose down            # stop, keeping data
docker compose down -v         # stop and delete all data
```

Want sample projects and tasks to explore? Set `SEED_DUMMY_DATA=true` in `.env` before the first start.

## 🚀 Local Development Setup

**Prerequisites:** [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) and SQL Server (Express or LocalDB is fine).

1. Clone and restore:

```powershell
git clone https://github.com/Taskist/taskist.git
cd taskist
dotnet restore src/Taskist.sln
```

2. Configure your connection string and encryption key. `appsettings.Development.json` is git-ignored, so it is safe for local values:

```json
{
  "ConnectionStrings": {
    "AppContext": "Server=.\\SQLEXPRESS;Database=Taskist;Integrated Security=True;TrustServerCertificate=True;"
  },
  "Security": {
    "EncryptionKey": "a-local-development-key-at-least-32-chars"
  }
}
```

3. Create the schema:

```powershell
cd src/Libraries/Taskist.Data
dotnet ef database update --startup-project ../../Presentation/Taskist.Web
```

4. Seed reference data by running the scripts in [`sql script/mssql/`](sql%20script/mssql/) **in order**:

| Script | Required | Contents |
| ------ | -------- | -------- |
| `1_defaults.sql` | ✅ | Menus, roles, permissions, language and the administrator account |
| `2_locale_resource.sql` | ✅ | English (India) translations |
| `3_dummy_data.sql` | Optional | Sample clients, projects and tasks for evaluation |

Each script runs in a transaction and is safe to re-run — rows are matched on their business key, so nothing is duplicated.

5. Run:

```powershell
cd src/Presentation/Taskist.Web
dotnet run
```

Browse to the URL shown in the console (typically `https://localhost:7169`).

## ⚙️ Configuration

Taskist reads configuration from `appsettings.json`, environment variables prefixed with `TASKIST_`, and user-secrets. Environment variables win, so they are the preferred way to supply secrets.

Nested keys use a double underscore: `Security:EncryptionKey` becomes `TASKIST_Security__EncryptionKey`.

### Required

The application **will not start** without these.

| Setting | Environment variable | Description |
| ------- | -------------------- | ----------- |
| `ConnectionStrings:AppContext` | `TASKIST_ConnectionStrings__AppContext` | SQL Server connection string |
| `Security:EncryptionKey` | `TASKIST_Security__EncryptionKey` | Minimum 32 characters, unique per deployment. Generate with `openssl rand -base64 32` |

### Optional

| Setting | Default | Description |
| ------- | ------- | ----------- |
| `Security:MaxFailedAccessAttempts` | `5` | Failed sign-ins before an account locks |
| `Security:LockoutMinutes` | `15` | How long the lockout lasts |
| `Security:RequireHttpsCookies` | `true` | Restrict cookies to HTTPS. Set `false` only when deliberately serving plain HTTP |
| `Sentry:Enabled` | `false` | Enable Sentry error reporting |

> [!IMPORTANT]
> `Security:RequireHttpsCookies` defaults to `true`, so **sign-in will not work over plain HTTP**. The Docker Compose stack sets it to `false` because it serves HTTP on localhost. Set it back to `true` once you are behind TLS.

## 💾 Database Migrations

Run EF Core commands from the `src/Libraries/Taskist.Data` folder:

```powershell
cd src/Libraries/Taskist.Data

# apply pending migrations
dotnet ef database update --startup-project ../../Presentation/Taskist.Web

# add a migration after changing an entity
dotnet ef migrations add YourMigrationName --startup-project ../../Presentation/Taskist.Web

# remove the last migration (only if not yet applied)
dotnet ef migrations remove --startup-project ../../Presentation/Taskist.Web
```

Upgrading an existing install? See **[UPGRADING.md](UPGRADING.md)** — the security hardening release requires new configuration and a migration.

## 🔐 Security

To report a vulnerability, please follow **[SECURITY.md](SECURITY.md)** rather than opening a public issue.

Deployment checklist:

- Serve over HTTPS and keep `Security:RequireHttpsCookies` set to `true`
- Set a unique `Security:EncryptionKey`
- Change the default `admin@taskist.org` password (`Admin@12345`)
- Use a least-privilege SQL account rather than `sa`
- Back up `App_Data/DataProtectionKeys` — losing it invalidates every session

## 🐛 GitHub Issues & Contribution

New contributors are very welcome. **[CONTRIBUTING.md](CONTRIBUTING.md)** covers environment setup, coding conventions and what CI expects.

### Raising an Issue

Open a [new issue](https://github.com/Taskist/taskist/issues/new/choose) and pick a template. Bug reports ask for reproduction steps, your version and how you deploy Taskist — that detail is usually what decides whether a bug can be fixed.

Found a security vulnerability? Please report it privately following **[SECURITY.md](SECURITY.md)** instead of opening an issue.

Setup questions are usually answered faster on [Zulip](https://taskist.zulipchat.com/#narrow/channel/539613-support).

### Submitting a Pull Request

For anything beyond a small fix, please discuss it first via an issue or on [Zulip](https://taskist.zulipchat.com/#narrow/channel/539614-dev), so your time is not spent on something that does not fit the project's direction.

```bash
# 1. Fork, then branch
git checkout -b feature/YourFeatureName

# 2. Make your changes, then verify what CI will check
dotnet build src/Taskist.sln
dotnet test src/Taskist.sln

# 3. Push and open a pull request against main
git push origin feature/YourFeatureName
```

**Code guidelines**

- Follow C# naming conventions and match the structure of the file you are editing
- Keep methods short and modular
- Use EF Core migrations for schema changes — CI fails if the model changes without one
- Add tests for authentication, permissions, data access or file upload changes

By participating, you agree to uphold our [Code of Conduct](CODE_OF_CONDUCT.md).

## 📄 License

This project is licensed under the **MIT License** – see the [LICENSE](LICENSE) file for details.

## 💖 Open Source Sponsors & Partners

We gratefully acknowledge the generous support of the following providers who offer free licenses or services to our open-source project:

<table>
<tbody>
<tr>
<td>
    <a href="https://sentry.io/for/open-source/" target="_blank" title="Sentry – Free error tracking for open-source projects">
        <img src="assets/sentry.png" alt="Sentry" />
    </a>
</td>
<td>
    <a href="https://www.atlassian.com/software/confluence" target="_blank" title="Atlassian Confluence">
        <img src="assets/confluence.png" alt="Atlassian Confluence" />
    </a>
 </td>
 <td>
    <a href="https://monsterasp.net" target="_blank" title="MonsterASP.Net">
        <img src="assets/monsterasp.net.png" alt="MonsterASP.Net" />
    </a>
 </td>
 <td>
    <a href="https://zulip.com" target="_blank" title="Zulip - Organized team chat app">
        <img src="assets/zulip.png" alt="Zulip - Organized team chat app" />
    </a>
 </td>
 <td>
    <a href="https://gitbook.com" target="_blank" title="Create and publish beautiful documentation">
        <img src="assets/gitbook.png" alt="Create and publish beautiful documentation" />
    </a>
 </td>
</tr>
</tbody>
</table>

## 🌟 Support the Project

If you find **Taskist** helpful, please consider supporting it! ❤️
Your support helps keep the project growing and maintained.

### 🪙 Ways to Support

- ⭐ **Star this repository** on GitHub to show appreciation
- 🪙 **Share it** with other developers or teams
- ☕ **Buy Me a Coffee** to support ongoing development
<p>
  <a href="https://www.buymeacoffee.com/somaraj" target="_blank"><img src="https://cdn.buymeacoffee.com/buttons/v2/default-yellow.png" alt="Buy Me A Coffee" style="height: 60px !important;width: 217px !important;" ></a>
</p>
