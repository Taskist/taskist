# Upgrading Taskist

## Upgrading to the security hardening release

This release fixes several security defects that affect all earlier versions. **Please read this before upgrading** — it requires configuration that did not exist previously, and the application will not start without it.

### 1. Back up your database

```powershell
sqlcmd -S <server> -Q "BACKUP DATABASE Taskist TO DISK='C:\backup\taskist-pre-upgrade.bak'"
```

### 2. Generate an encryption key

Earlier versions used a key that was hard-coded in the public source. Generate your own:

```bash
openssl rand -base64 32
```

Set it, along with your connection string, as environment variables:

```bash
TASKIST_Security__EncryptionKey=<your generated key>
TASKIST_ConnectionStrings__AppContext=<your connection string>
```

Both are also settable in `appsettings.json`, but environment variables are preferred so secrets stay out of source control.

> The application throws on startup if the key is missing, shorter than 32 characters, or still set to the old published value. This is intentional.

### 3. Apply the database migration

```powershell
cd src/Libraries/Taskist.Data
dotnet ef database update --startup-project ../../Presentation/Taskist.Web
```

This adds two columns:

| Table | Column | Purpose |
| ----- | ------ | ------- |
| `UserPassword` | `HashFormat` | Records which algorithm produced the stored hash. Existing rows default to `0` (legacy SHA1). |
| `User` | `LockoutEndDate` | Supports the new failed-sign-in lockout. |

### 4. Serve the application over HTTPS

Session and authentication cookies are now marked `Secure`. **Sign-in will silently fail over plain HTTP** outside local development. If you terminate TLS at a reverse proxy, make sure it forwards `X-Forwarded-Proto`.

### 5. Start the application

Existing users sign in with their current passwords — no reset required.

---

## What changed

### Passwords upgrade themselves

Password hashing moved from unsalted-style SHA1 to PBKDF2-HMAC-SHA256 (210,000 iterations).

Stored hashes cannot be converted without the plaintext, so the upgrade is transparent: a legacy hash is verified with SHA1 on sign-in, and immediately re-hashed to PBKDF2 on success. No user action is needed, and legacy hashes disappear as people sign in.

To force the migration instead, require all users to reset their passwords.

### Accounts now lock after repeated failures

After 5 consecutive failed attempts (default), an account locks for 15 minutes. Tune this in configuration:

```json
{
  "Security": {
    "MaxFailedAccessAttempts": 5,
    "LockoutMinutes": 15
  }
}
```

Sign-in endpoints are also rate limited to 10 requests per minute per IP address.

Note that failed sign-ins no longer distinguish "user doesn't exist" from "wrong password" — both return the same message, so the form cannot be used to discover which email addresses have accounts.

### Attachments are access-checked and restricted

Attachment endpoints previously accepted any document ID from any signed-in user. They now verify the caller is a member of the project the item belongs to. The same check was added to comments and history.

Uploads are now limited to an allowlist of extensions and 10 MB, and are always served as downloads rather than with their stored content type — an HTML attachment previously executed in the browser.

If your users legitimately upload a type that is now rejected, extend `AllowedUploadExtensions` in `ServiceConstant`. Be careful adding types the browser will execute.

### Diagnostics are development-only

`EnableSensitiveDataLogging` (which logs query parameter values, including credentials) and MiniProfiler now only run in the Development environment. Set `ASPNETCORE_ENVIRONMENT=Production` in production.

### Everyone is signed out once

Legacy `wc.bl.*` cookie names were renamed to `taskist.*`, and the data protection application name changed from `Backlog` to `Taskist`.

Existing sessions are not carried over, so **all users are signed out when you upgrade**. They can sign straight back in with their existing passwords. Stale `wc.bl.*` cookies are harmless and expire on their own.

### Failed writes now surface

The repository previously caught and discarded exceptions on save, so a failed write reported success to the caller. Exceptions now propagate to the global error handler.

**This may surface pre-existing errors that were previously invisible.** If something starts reporting failures after upgrading, it was most likely failing silently before — check the log table for detail.

---

## Rolling back

The migration is reversible:

```powershell
dotnet ef database update AvatarVersion --startup-project ../../Presentation/Taskist.Web
```

Be aware that any password re-hashed to PBKDF2 will **not** verify against older application versions. Restore your pre-upgrade backup if you need a complete rollback.
