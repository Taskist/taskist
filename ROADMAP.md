# Taskist roadmap

What we plan to build next and why. The order is a plan, not a promise. It changes with feedback, so tell us what blocks you in [Discussions → Ideas](https://github.com/Taskist/taskist/discussions/categories/ideas) or give a 👍 to the issue you care about.

Each release has a [milestone](https://github.com/Taskist/taskist/milestones). Issues labelled `good first issue` are a good place to start contributing; see [CONTRIBUTING.md](CONTRIBUTING.md).

## At a glance

| Version | Theme | Highlights |
| --- | --- | --- |
| **1.1** | Sign in with your company account | Single sign-on (OpenID Connect), two-factor sign-in, keyboard shortcuts |
| **1.2** | Better collaboration | @mentions, watchers, task templates, task links |
| **1.3** | Bring your history with you | Import wizard for CSV/Excel, Jira, Redmine, Trello and Azure DevOps |
| **1.4** | Build on Taskist | REST API with personal access tokens, due-date reminders |
| **1.5** | Plan sprints | Estimates and time logging, sprints and burndown (per project) |
| **2.0** | Runs on PostgreSQL | PostgreSQL as an alternative to SQL Server |

## What every release follows

- **Upgrades in place.** Schema changes only add things, and seed scripts only add missing rows. Anything you need to do is listed under **Upgrade notes** in the [changelog](CHANGELOG.md).
- **Nothing new switches itself on.** Features that send messages, call outside services or change what users see are enabled in settings. Every setting can also be set as an environment variable for Docker.
- **Free licences only.** Bundled libraries are MIT, Apache or BSD, vendored locally with no CDN, and listed in [THIRD-PARTY-NOTICES.md](THIRD-PARTY-NOTICES.md).
- **Translatable.** Every label is a translation key.
- **Security first.** Anything touching sign-in, tokens or outside URLs is documented in [SECURITY.md](SECURITY.md) and covered by tests.

## 1.1: Sign in with your company account

- **Single sign-on (OpenID Connect).** Works with Keycloak, Authentik, Microsoft Entra ID, Google, Okta and GitLab/GitHub.
  - By default, only existing users can sign in this way, matched by verified e-mail. Creating accounts on first sign-in is optional; new accounts get the *Registered* role only.
  - Optional mapping from identity-provider groups to Taskist roles.
  - The local administrator login always stays available, so a broken identity provider can't lock you out.
- **Two-factor sign-in.** Authenticator-app codes and recovery codes for local accounts; administrators can be required to use it.
- **Keyboard shortcuts and quick-add.** Create a task, search and move between pages without the mouse. Press `?` for the list.

## 1.2: Better collaboration

- **@mentions** in comments notify the person straight away. Only project members can be mentioned.
- **Watchers.** Follow any task to hear about status changes, comments and reassignment. You follow tasks you create, comment on or are mentioned in automatically.
- **Task templates.** Bug, change-request and to-do templates (steps to reproduce, expected vs actual, environment) fill the description on create. Admins can add templates, shared or per project.
- **Task links.** *Blocks*, *blocked by*, *duplicates* and *relates to*, with a badge on the board and grid for blocked tasks.

## 1.3: Bring your history with you

- **Import wizard.** Upload a CSV or Excel file, map its columns to Taskist fields, preview any errors, then import in the background. Importing the same file again doesn't create duplicates.
- **Ready-made mappings** for Jira, Redmine, Trello and Azure DevOps exports. New mappings are data files, so they're easy to contribute.
- GitHub and GitLab issues can already be imported with [issue sync](docs/git-integration.md).

## 1.4: Build on Taskist

- **REST API v1** for tasks, comments, attachments, projects and releases, with an OpenAPI description and rate limiting. It applies the same permission and project-membership rules as the web UI.
- **Personal access tokens** with scopes and expiry dates, created under *My profile*.
- **Due-date reminders.** An optional daily summary of tasks due soon or overdue, plus an overdue badge and dashboard card.

## 1.5: Plan sprints

Both features are turned on per project, so teams that don't use them never see them.

- **Estimates and time logging.** Estimate and remaining hours, a work log on each task, and a time report with Excel export.
- **Sprints.** Plan a sprint from the backlog, show the active sprint on the board, follow progress on a burndown chart, and move unfinished work to the next sprint.

## 2.0: Runs on PostgreSQL

- **PostgreSQL** as an alternative to SQL Server, with its own migrations and a Docker Compose setup based on PostgreSQL.
- Seed data moves from SQL scripts into the application, so it works on both databases.

## Later

- **Recurring tasks** for routine work such as monthly patching or backup checks.
- Whatever the community asks for most. Requests with the most 👍 move up.
