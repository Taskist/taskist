# Contributing to Taskist

We welcome and appreciate all contributions to **Taskist** — bug fixes, features, documentation, translations, and help for other users in the community.

## 💡 Before You Start

**For anything beyond a small fix, please discuss the change first** in an [issue](https://github.com/Taskist/taskist/issues), in [GitHub Discussions](https://github.com/Taskist/taskist/discussions/categories/ideas), or by [email](mailto:taskist@outlook.in). It saves your time if the idea doesn't fit the project's direction.

Other ways to help:

- Answer questions in [GitHub Discussions](https://github.com/Taskist/taskist/discussions/categories/q-a)
- Improve the [documentation](https://docs.taskist.org)
- Spread the word — write about Taskist or link to it

Found a vulnerability? Please **do not** open a public issue or pull request. Follow [SECURITY.md](SECURITY.md) to report it privately.

## 🛠 Setting Up Your Development Environment

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- SQL Server (Express, LocalDB or the Docker image)
- EF Core tools: `dotnet tool install --global dotnet-ef`
- Optional: [Docker Desktop](https://www.docker.com/products/docker-desktop/) for the full stack

### Getting started

```bash
git clone https://github.com/Taskist/taskist.git
cd taskist
dotnet restore src/Taskist.sln
```

Create `src/Presentation/Taskist.Web/appsettings.Development.json` (git-ignored):

```json
{
  "ConnectionStrings": {
    "AppContext": "Server=.\\SQLEXPRESS;Database=Taskist;Integrated Security=True;TrustServerCertificate=True;"
  }
}
```

Create the schema, then run `sql-scripts/02_defaults.sql` and `03_locale.sql` (`04_demo.sql` adds optional sample data):

```bash
dotnet ef database update --project src/Libraries/Taskist.Data --startup-project src/Libraries/Taskist.Data
```

Run the app and sign in with `admin@taskist.org` / `Admin@123`:

```bash
dotnet run --project src/Presentation/Taskist.Web
```

### Solution layout

| Project | Contents |
| ------- | -------- |
| `src/Libraries/Taskist.Core` | Entities, enums, DTOs, settings, shared helpers |
| `src/Libraries/Taskist.Data` | EF Core context, configurations, migrations, repositories |
| `src/Libraries/Taskist.Service` | Business services (registered automatically by assembly scan) |
| `src/Presentation/Taskist.Web` | MVC controllers, views, view models, validators, `wwwroot` |
| `src/Tests/Taskist.Tests` | xUnit tests (no database needed) |
| `sql-scripts/` | Idempotent seed scripts, run in numeric order |
| `tools/theme/` | Dark-theme generator (`generate-dark-css.py`) |

## ✅ Before Opening a Pull Request

CI runs these on every pull request, so please run them locally first:

```bash
dotnet build src/Taskist.sln
dotnet test src/Taskist.sln

# fails if the model changed without a migration
dotnet ef migrations has-pending-model-changes --project src/Libraries/Taskist.Data --startup-project src/Libraries/Taskist.Data
```

If you changed an entity, add a migration:

```bash
dotnet ef migrations add YourMigrationName --project src/Libraries/Taskist.Data --startup-project src/Libraries/Taskist.Data
```

Rebuild first; a stale build can produce an empty migration.

## 📐 Code Guidelines

Please match the style of the file you are editing. In particular:

**C#**

- **No comments in `.cs` files.** Group members with `#region` blocks instead (`Fields`, `Ctor`, `Methods`, `Utilities`, …).
- **One top-level type per file**, named after the type. Nested types stay with their parent.
- Inject dependencies through the constructor into `private readonly` fields.

**Views and front end**

- **No inline `<script>` or `<style>` blocks.** Put JavaScript in `wwwroot/js` and CSS in `wwwroot/css`.
- **No CDNs.** Vendor libraries locally under `wwwroot/lib`.
- **No `ViewBag` / `ViewData`** for passing data (the layout title is the only exception). Every view has a typed `@model`; put UI-only flags on a view model in `Taskist.Web/Models`.
- Empty, setup and error states use the `<wp-empty-state>` tag helper.
- After changing a colour in `wwwroot/css`, regenerate the dark theme with `python tools/theme/generate-dark-css.py` (Python 3.8+, no packages) and commit the result; CI fails if it is stale. Never edit `theme-dark.generated.css` by hand; dark fixes the generator can't infer go in `theme-dark.css`.

**Text and translations**

- UI text comes from the database. Add new keys to `sql-scripts/03_locale.sql`; labels don't render until the script runs.
- Seed scripts containing non-ASCII characters must stay **UTF-8 with BOM**.

**Database**

- Schema changes go in an EF Core migration only; never hand-written DDL.
- Migrations must be **additive and non-destructive**: add nullable columns or relax constraints. Don't drop columns holding data or rewrite existing values. Installs run in production with live data.
- Seed data follows the existing `INSERT … SELECT … WHERE NOT EXISTS` pattern, matched on business keys. Never use `SET IDENTITY_INSERT`.
- Edit forms must always include the record's current value in drop-downs, so saving can't silently clear a foreign key.

**Dependencies**

- Only permissively licensed (MIT, Apache 2.0, BSD) packages. No commercial, dual-licensed or "non-commercial" packages — for example FluentAssertions 8+ or EPPlus 5+.

**Tests**

- New pure logic (parsers, rules, calculations) should come with tests in `src/Tests/Taskist.Tests`. Use plain xUnit `Assert`.

### Adding a master (CRUD screen)

Entity (`BaseEntity` + `ISoftDeletedEntity` + `IAuditableEntity`) → EF configuration → service interface and implementation → model (`[LocalizedDisplayName]`) → controller (`[CheckPermission]`) → views (index DataTable, `_Form` create/edit) → FluentValidation validator → Mapperly mapping → permission in `PermissionProvider` and `02_defaults.sql` → menu entry in `02_defaults.sql` → labels in `03_locale.sql`.

## 🔐 Security-Sensitive Changes

Take extra care with code that touches authentication, authorization, file uploads, outbound HTTP or query building, and call it out in your pull request description.

- **Never interpolate user input into a query expression.** Grid sorting goes through `ApplySafeSort`, which only accepts real property names. Don't bypass it.
- **Always scope data access to the caller.** Loading a record by ID is not enough; check that the signed-in user may reach it (project membership, `CanEditTaskAsync`, …).
- **Outbound requests** to user-configured URLs must use the hardened named `HttpClient`s, which block loopback, link-local and metadata addresses.
- **Secrets** are stored through `[Encrypted]` properties or `ISecretProtector`, never in plain text, and never logged.

By participating, you agree to uphold our [Code of Conduct](CODE_OF_CONDUCT.md).
