# Changelog

All notable changes to Taskist are documented here. The format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and versions follow [Semantic Versioning](https://semver.org/).

Each release lists **Upgrade notes** when an install needs action beyond the steps in [UPGRADING.md](UPGRADING.md).

## [Unreleased]

### Added

- **GitHub support** alongside GitLab: multiple Git connections, several repositories per project, webhooks per repository, user accounts connected by token or OAuth, and two-way issue sync. See [docs/git-integration.md](docs/git-integration.md).
- **Merge request reviews** (Work Items → Reviews): approve, comment, reject with a reason, rebase and merge under the reviewer's own account.
- **Chat notifications** to Microsoft Teams (channel or Power Automate flow), Slack, Google Chat, Discord, Mattermost or any JSON webhook (optionally HMAC-signed), chosen per project and event.
- **Release version auto-generation** from a per-project format, with "patch of" a chosen release and live hints for manual names.
- **Closing checklist**: statuses can require root cause, resolution details or a closing merge request.
- **Appearance**: light, dark or system theme and an accent colour per user.
- **Per-user export column layouts** on top of the administrator's default.
- **Tags** for the backlog, with inline create and merge.
- `Security:RequireHttpsCookies` and `Security:ForwardedHeaders:*` settings for plain-HTTP and reverse-proxy deployments.
- Startup checks: the Data Protection key folder must be writable; Production logs warnings for plain-HTTP cookies and an unprotected key ring.
- Test project (`src/Tests/Taskist.Tests`) and GitHub Actions CI.

### Changed

- Upgraded to **.NET 10** and EF Core 10.
- Passwords are hashed with PBKDF2-HMAC-SHA512 (210,000 iterations).
- All stored secrets are protected with ASP.NET Core Data Protection.
- Renamed *Employee* → *User* and *BacklogTask* → *Backlog* throughout.
- Backlog filters use select2; Kendo UI was removed.
- Excel export uses ClosedXML instead of EPPlus; object mapping uses Mapperly.
- Seed data moved from the install wizard to four idempotent scripts in `sql-scripts/`: schema, defaults, translations and optional sample data. A fresh install now also gets the email templates, using an inactive placeholder email account.
- Docker images target .NET 10; the migrate container runs `sql-scripts/02`–`04`.
- E-mails fill `%company%` from **Configuration → Settings → Company name** (falls back to "Taskist").

### Security

- Task attachments are access-checked on download, upload and delete, uploads require the antiforgery token, and files are served as downloads with `nosniff`.
- Uploads are limited to an allow-list of file types and a size cap; wiki images and profile photos accept raster images only.
- Files uploaded before a task is saved are linked through a signed, user-bound token instead of a raw document ID.
- The seeded administrator is only created in an empty user table, so re-running the seed scripts can never restore the default account.

### Fixed

- "Open tasks by project" on the dashboard no longer counts closed or deleted tasks.
- Missing "Reporter" label on the task form.
- Branch names from accented titles no longer split words ("déjà" → `deja`).

### Removed

- HR module (leave, work from home, attendance, holidays, approvals), the desktop quick-capture app, and the organisation structure (departments, designations, org chart, country/state masters).
- Issue Tracker module.
- Developer notes and QA notes fields.
- Reporting fields on tasks (reported via / on / by / to) and the generic Lookup master.
- Dev, Test and Release status fields on tasks; release tracking uses the Release Plan.
- The Client User role; signing in requires the Registered role.
- The unused "Form Name" column in the release export.
- The built-in encryption key and the legacy `enc:` secret format.

### Upgrade notes

- The schema was re-baselined into a single `InitialCreate` migration. Databases from earlier releases cannot be upgraded in place — see [UPGRADING.md](UPGRADING.md).
- Environment variables are no longer prefixed: use `ConnectionStrings__AppContext`, not `TASKIST_ConnectionStrings__AppContext`.
- The Docker `uploads` volume is no longer used; attachments are stored in the database.
