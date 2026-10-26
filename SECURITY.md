# Security Policy

## Reporting a Vulnerability

**Please do not report security vulnerabilities through public GitHub issues, Discussions, or pull requests.**

Taskist stores user credentials, project data and access tokens for Git providers, so we ask that suspected vulnerabilities are reported privately to give us time to publish a fix before details become public.

Report a vulnerability in either of these ways:

- **GitHub:** [Open a private security advisory](https://github.com/Taskist/taskist/security/advisories/new) (preferred)
- **Email:** [taskist@outlook.in](mailto:taskist@outlook.in) with the subject line `SECURITY`

Please include as much of the following as you can:

- The type of issue (for example: privilege escalation, injection, authentication bypass, SSRF)
- The affected file, endpoint, or component
- Steps to reproduce, or a proof of concept
- The version or commit you tested against
- What an attacker could achieve with it

### What to Expect

| Stage | Target |
| ----- | ------ |
| Acknowledgement of your report | Within 3 working days |
| Initial assessment and severity | Within 7 working days |
| Fix released for confirmed issues | As soon as practical, prioritised by severity |

We will keep you updated as we work on the issue, and we are happy to credit you in the release notes and advisory unless you would prefer to remain anonymous.

Taskist is a volunteer-maintained open-source project. We do not currently operate a paid bug bounty, but we genuinely appreciate the time researchers spend on it.

### Scope

In scope:

- The Taskist application source code in this repository
- Default configuration and seed scripts shipped in this repository
- The Dockerfiles and compose file

Out of scope:

- The demo site at `demo.taskist.org` — please do not run automated scanners or destructive tests against it
- Vulnerabilities in third-party dependencies that already have a public advisory (please report those upstream, though we welcome a heads-up)
- Issues that require an already-compromised host or physical access
- Missing hardening headers with no demonstrated impact

## Supported Versions

Security fixes are applied to the latest release. We recommend always running the most recent version.

| Version | Supported |
| ------- | --------- |
| Latest release | ✅ |
| Older releases | ❌ |

## Deploying Taskist Securely

Taskist is self-hosted, so several security properties depend on how you deploy it.

### Transport and cookies

- **Terminate TLS.** With the default `Security:RequireHttpsCookies=true`, the sign-in cookie is marked `Secure`, so sign-in only works over HTTPS. Set it to `false` only for a deliberately plain-HTTP setup such as the local Docker sample; Taskist logs a warning at startup when it is off outside Development.
- **Behind a reverse proxy**, enable `Security:ForwardedHeaders:Enabled` and list the proxy in `KnownProxies` / `KnownNetworks`. Only listed proxies (and loopback) are trusted, so clients cannot spoof their IP address or scheme. Invalid entries stop the application from starting.

### Encryption keys

Taskist has no encryption key setting of its own. All secrets — Git access tokens, OAuth client secrets, webhook secrets, notification channel URLs and signing secrets, password-reset tokens and authentication cookies — are protected with **ASP.NET Core Data Protection**.

- The key ring lives in `App_Data/DataProtectionKeys` (or `DataProtection:KeysPath`). The application refuses to start if that folder is not writable.
- **Back it up.** Losing it signs everyone out and makes every stored secret unreadable; they then have to be re-entered.
- **Protect it at rest** with `DataProtection:CertificatePath` (+ `CertificatePassword`), `DataProtection:CertificateThumbprint`, or `DataProtection:ProtectWithDpapi` on Windows. Taskist logs a startup warning in Production, and shows one on the Integration settings tab, while the key ring is unprotected.
- In Docker, the key ring is the `taskist-keys` volume. `docker compose down -v` deletes it.

Secrets are write-only in the UI and are never written to the activity log.

### Passwords and sign-in

- Passwords are hashed with **PBKDF2-HMAC-SHA512, 210,000 iterations** and a per-user random salt, stored in a self-describing format. Hashes made with fewer iterations are upgraded transparently on the next successful sign-in.
- Password length, failed-attempt limit, session length, reset-token lifetime and password expiry are set under **Configuration → Settings → Security**. When the failed-attempt limit is reached the account is **locked until an administrator unlocks it**.
- The forgot-password endpoint is rate limited per IP address.
- Change the seeded `admin@taskist.org` password immediately after installation.

### Wiki access

- A project's wiki — page content, tree, history, linked tasks and Word export — is visible only to members of that project who also hold the *Wiki* permission. Requests for pages in other projects get an empty or "not found" response that does not reveal whether the page exists.
- Editing (create, save, rename, move, delete, restore) additionally requires the member's *Can add wiki* right.
- Pages and folders can only be moved within their own project; a folder cannot be moved into itself or one of its sub-folders; a version can only be restored onto the page it belongs to.
- **Wiki images** are recorded against the page's project when they are uploaded (uploading requires edit rights on that page) and are served only to the uploader and to people who can view that project. Pasting another project's image URL into your own page does not grant access, even before the original page is saved. Images uploaded before this record existed fall back to the first page that saved a reference to them. The image endpoint never serves profile photos or task attachments, and the Word export embeds only images the reader may view.

### Attachments and images

- Task attachments can only be downloaded by users who can see the task, and only added or removed by users who can edit it.
- Uploads are limited to an allow-list of file types and a size cap (`UploadPolicy` in `Taskist.Core`). Wiki images and profile photos accept raster images only (PNG, JPEG, GIF, WebP, BMP); SVG is not accepted.
- Files are served as downloads with `X-Content-Type-Options: nosniff`; only raster images are shown inline, with a content type derived from the file extension rather than from the uploader.
- Files uploaded before a task is saved are linked through a signed, user-bound token that expires after a day.

### Integrations

- **Git webhooks** must be configured with a secret; deliveries without a valid signature or token are rejected. The webhook endpoint is rate limited.
- **Outbound requests** to Git providers and chat services go through hardened HTTP clients that use HTTPS only, do not follow redirects, and refuse loopback, link-local and cloud-metadata addresses. Notification channels are also restricted to the expected host for their type.
- Branch creation, merge/pull requests, approvals and merges always use the **acting user's own** Git token, so the provider's own permission rules apply.

### Error tracking (optional)

Taskist can send unhandled exceptions and errors logged by the application to [Sentry](https://sentry.io). It is **off by default** and starts only when `Sentry:Enabled` is `true` and `Sentry:Dsn` is a valid DSN; otherwise the SDK is never initialised, and a misconfiguration is logged as a startup warning.

When enabled, every event is filtered before it leaves the server:

- **Not sent:** request bodies, cookies, query strings (they can carry password-reset tokens) — also stripped from URLs inside log breadcrumbs and exception messages — and the `Authorization`, `Cookie`, antiforgery and webhook-signature headers.
- **Users** are identified by their numeric Taskist user ID only — no name, e-mail address or IP address (`SendDefaultPii` is always off).
- Performance tracing is off unless `Sentry:TracesSampleRate` is set above `0`.

Exception messages and stack traces are sent as they are, so treat your Sentry project as holding operational data and restrict who can see it.

### Operations

- **Restrict the Hangfire dashboard.** `/hangfire` is limited to administrators and users with the *Manage Jobs* permission; consider also blocking it at your reverse proxy.
- **Use a least-privilege SQL login**, not `sa`. The Docker sample uses `sa` for convenience only.
- **Never commit real secrets.** `appsettings.Development.json`, `appsettings.Production.json` and `.env` are git-ignored. Prefer environment variables or [user-secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets).
- Run with `ASPNETCORE_ENVIRONMENT=Production`. Detailed errors, sensitive-data logging and MiniProfiler are enabled only in Development.
- **Keep current.** Watch this repository for releases.
