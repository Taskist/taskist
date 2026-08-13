# Security Policy

## Reporting a Vulnerability

**Please do not report security vulnerabilities through public GitHub issues, Zulip, or pull requests.**

Taskist stores user credentials and project data, so we ask that suspected vulnerabilities are reported privately to give us time to publish a fix before details become public.

Report a vulnerability in either of these ways:

- **Email:** [hello@taskist.org](mailto:hello@taskist.org) with the subject line `SECURITY`
- **GitHub:** [Open a private security advisory](https://github.com/Taskist/taskist/security/advisories/new)

Please include as much of the following as you can:

- The type of issue (for example: privilege escalation, injection, authentication bypass)
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
- Default configuration shipped in this repository
- The official Docker images and compose files

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

Taskist is self-hosted, so several security properties depend on how you deploy it. Please make sure you have covered the following.

### Required configuration

The application **will not start** until these are set. This is deliberate — earlier versions shipped with insecure defaults.

| Setting | Environment variable | Notes |
| ------- | -------------------- | ----- |
| `ConnectionStrings:AppContext` | `TASKIST_ConnectionStrings__AppContext` | Use a least-privilege SQL account, not `sa` |
| `Security:EncryptionKey` | `TASKIST_Security__EncryptionKey` | Minimum 32 characters, unique per deployment |

Generate a key with:

```bash
openssl rand -base64 32
```

> [!IMPORTANT]
> Releases prior to the security hardening release contained a hard-coded encryption key in the source. Because it was published in this public repository, it offered no protection — anyone could forge account activation tokens against any deployment using it. The application now refuses to start if that key is detected. If you are upgrading, you **must** generate a new key.

### Recommended practice

- **Always terminate TLS.** Session and authentication cookies are marked `Secure`, so sign-in will not work over plain HTTP outside local development.
- **Never commit real secrets.** `appsettings.Development.json` and `appsettings.Production.json` are git-ignored. Prefer environment variables or [user-secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets) in development.
- **Restrict the Hangfire dashboard.** `/hangfire` is limited to the `SystemAdministrator` role; consider blocking it at your reverse proxy as well.
- **Back up the data protection keys** in `App_Data/DataProtectionKeys`. Losing them invalidates every active session.
- **Keep dependencies current.** Watch this repository for releases.

### Password storage

Passwords are hashed with PBKDF2-HMAC-SHA256 (210,000 iterations, per-user random salt).

Deployments upgrading from an earlier version keep working without a forced password reset: legacy SHA1 hashes are verified against the old algorithm on sign-in and then transparently re-hashed to PBKDF2. The legacy hashes disappear as users sign in. If you would rather not wait, you can require all users to reset their passwords.
