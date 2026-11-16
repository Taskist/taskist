# Git integration (GitHub and GitLab)

Taskist links projects to repositories on GitHub (github.com or GitHub Enterprise Server) and GitLab
(gitlab.com or self-managed). It shows branches and merge/pull requests on each task, lets developers create
branches and merge/pull requests, runs code reviews from the Reviews page, moves task status from webhook
events and can sync issues both ways.

## 1. Add a Git connection (administrator)

**Integrations → Git Connections** (permission *Manage Git Connections*).

| Field | GitHub | GitLab |
|---|---|---|
| Server URL | `https://github.com` or your GHES URL | `https://gitlab.com` or your server |
| API URL | leave empty | leave empty |
| Service authentication | **GitHub App** (recommended) or a fine-grained token | Project/group access token (bot) |

### GitHub App (recommended)

1. GitHub → Settings → Developer settings → GitHub Apps → New GitHub App.
2. Permissions: Contents *read*, Metadata *read*, Pull requests *read & write*, Issues *read & write*.
   Leave the app's own webhook inactive (Taskist uses repository webhooks).
3. Generate a private key, paste the PEM file contents and the App id into the connection.
4. **Install App** on the account or organization that owns the repositories, and select only the
   repositories Taskist needs. Creating the app is not enough: until it is installed, Taskist cannot list or
   read any repository.

### GitLab service token

Create a project or group access token (it acts as a bot user) with `read_api`. Use `api` if you want
issue sync or automatic reviewer assignment.

### Optional: one-click "Connect" for users (OAuth)

- GitHub: register an OAuth App (or use the GitHub App's user authorization). Callback URL: the one shown in
  the connection form (`…/profile/gitoauthcallback`).
- GitLab: Admin/User settings → Applications, scope `api`, same callback URL.
Paste the client id and secret into the connection. Users can still use personal access tokens.

## 2. Link repositories to a project

**Portfolio → Project** → edit → **Repositories** tab → Add. Pick the connection and search the repository (or
type `owner/repo` / `group/project`). Mark one repository as primary. Then open **Webhook**:

- **Create webhook** (recommended) uses your own connected account (GitHub repository admin, GitLab
  Maintainer) and stores a fresh secret without showing it.
- Or copy the payload URL, generate a secret (shown once) and add the webhook by hand: content type JSON;
  GitHub events *push, pull request, pull request review, issue comment* (+ *issues* for issue sync); GitLab
  triggers *push, merge request, comments* (+ *issues*).

## 3. Users connect their own account

My profile → **Git connections** → Connect. With a token: GitHub classic token with `repo` (or a fine-grained
token with Contents + Pull requests read & write), GitLab token with `api`. Branches, merge/pull requests,
approvals and merges are always done in the user's own name.

## 4. Issue sync (optional, per repository)

| Mode | What happens |
|---|---|
| Off | default |
| Import | issues with the import label become tasks; later issue changes update the task |
| Export | tasks are published as issues (button on the task, or automatically for chosen task types) |
| Two-way | both; when both sides changed the same field, the newer change wins and a note is added |

Synced fields: title, description (Markdown ↔ HTML), open/closed (mapped to the statuses you choose),
labels ↔ tags, assignee (only when the user has connected their account). Comments are synced when enabled;
outgoing comments only when the author ticks **Post to issue**. Webhooks give near-real-time sync; a
background job also checks every 5 minutes.

## Troubleshooting

| Symptom | Cause and fix |
|---|---|
| Repository picker is empty, or "The GitHub App is not installed…" | The app exists but is not installed. GitHub → the app's settings → **Install App** on the repositories' owner. |
| "GitHub rejected the GitHub App credentials (401)" | Wrong App ID or private key. Paste the App ID and the full PEM file again. |
| "Connect your … account" when creating a branch or merge request | These actions use the user's own account. My profile → **Git connections** → Connect. |
| Webhook deliveries fail with 401/403 | The secret in GitHub/GitLab doesn't match. Regenerate it on the project's **Webhook** dialog and update the hook (or use **Create webhook**). |
| Tasks don't move when a merge request opens or merges | Check the webhook's recent deliveries on GitHub/GitLab, and that the merge request title or branch names the task (`#123`). |

## Upgrading from the GitLab-only version

On startup the old Integration settings (GitLab URL, token, webhook secret) become a default GitLab
connection, each project's GitLab project becomes its primary repository, and the old values are cleared.
Users' saved GitLab tokens move to the new store the first time they are used. Existing webhooks keep working
through the old URL; switch them to the new per-repository URL when convenient.

## Running the database changes

```
dotnet ef database update --project src/Libraries/Taskist.Data --startup-project src/Libraries/Taskist.Data
```
or run `sql-scripts/01_schema.sql`, then `02_defaults.sql` (new permission) and `03_locale.sql` (new texts).

Webhook secrets, service tokens and users' tokens are encrypted with ASP.NET Core Data Protection. Back up the
key folder (see [SECURITY.md](../SECURITY.md#encryption-keys)); without it they must be entered again.
