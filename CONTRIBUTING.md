# Contributing to Taskist

We welcome and appreciate all contributions to the **Taskist** project! Whether you're fixing bugs, adding features, or helping the community, your efforts are vital to our success.

## 💡 How to Contribute

### Discuss Changes First

**When contributing to this repository, please first discuss the change you wish to make via [issue](https://www.google.com/url?sa=E&source=gmail&q=https://github.com/Taskist/Taskist/issues), [Zulip](https://taskist.zulipchat.com/#narrow/channel/539614-dev) or [email](mailto:hello@taskist.org) with the owners of this repository before making a change.** This helps ensure that your work aligns with the project's direction and minimizes rework.

### General Contribution Opportunities

You are more than welcome to contribute to the success of Taskist by:

* Helping out in the realization of the project's source code.

* Participating in the forums or community discussions.

* Spreading the word about Taskist! If you have a site, write about us or place a hyperlink!

We are committed to nurturing the growth of the Taskist community!

### More Information on Specific Contributions

* Working with source code and contributions: [more info](https://taskist.org)

* Sharing your plugins, themes, and language packs: [more info](https://taskist.org)

* Contributing to the documentation: [more info](https://taskist.org)

## 🛠 Setting Up Your Development Environment

### Prerequisites

* [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
* SQL Server (Express, LocalDB or the Docker image is fine)
* Optionally [Docker Desktop](https://www.docker.com/products/docker-desktop/), which is the quickest way to get a database running

### Getting Started

```bash
git clone https://github.com/Taskist/taskist.git
cd taskist
dotnet restore src/Taskist.sln
```

Taskist requires a connection string and an encryption key, and **will not start without them**. This is deliberate — it prevents insecure defaults reaching production.

Create `src/Presentation/Taskist.Web/appsettings.Development.json` (git-ignored, so it is safe for local values):

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

Then create the schema and seed reference data:

```bash
cd src/Libraries/Taskist.Data
dotnet ef database update --startup-project ../../Presentation/Taskist.Web
```

Run `1_defaults.sql` then `2_locale_resource.sql` from [`sql script/mssql/`](sql%20script/mssql/). `3_dummy_data.sql` adds optional sample content for local testing.

The seed scripts are transactional and idempotent — they match on business keys rather than identity values, so re-running them is safe. If you add seed data, follow the same `INSERT ... SELECT ... WHERE NOT EXISTS` pattern and never use `SET IDENTITY_INSERT`.

Finally:

```bash
cd src/Presentation/Taskist.Web
dotnet run
```

The full setup, including the Docker route, is documented in the [README](README.md).

## ✅ Before Opening a Pull Request

Please make sure the following pass locally, since CI runs the same checks:

```bash
# build
dotnet build src/Taskist.sln

# tests
dotnet test src/Taskist.sln
```

If you changed an entity or anything under `Taskist.Data`, add a migration:

```bash
cd src/Libraries/Taskist.Data
dotnet ef migrations add YourMigrationName --startup-project ../../Presentation/Taskist.Web
```

CI fails if the model changes without a matching migration.

## 📐 Code Guidelines

* Follow standard C# naming conventions
* Match the structure of the file you are editing — the codebase groups members with `#region` blocks (`Fields`, `Ctor`, `Utilities`, `Methods`) and uses `protected readonly` for injected dependencies
* Keep methods short and focused
* Put database changes in an EF Core migration, never in a hand-edited script
* Add tests for anything touching authentication, permissions, data access or file uploads

### Security-Sensitive Changes

Take extra care with code that touches authentication, authorization, file uploads or query building, and call it out in your pull request description.

Two rules matter in particular:

* **Never interpolate user input into a query expression.** Sorting and grouping go through `OrderBySafe`, which validates the column against the entity's real properties. Bypassing it reintroduces an injection vector.
* **Always scope data access to the caller.** Loading a record by ID is not sufficient — confirm the signed-in user may reach it (see `CanAccessAsync` in `BacklogItemService`).

Found a vulnerability? Please **do not** open a public issue or pull request. Follow [SECURITY.md](SECURITY.md) to report it privately.
