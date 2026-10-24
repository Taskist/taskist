# Upgrading Taskist

## Before every upgrade

1. **Back up the database.**

   ```powershell
   sqlcmd -S <server> -Q "BACKUP DATABASE Taskist TO DISK='C:\backup\taskist-pre-upgrade.bak'"
   ```

2. **Back up the Data Protection key folder** (`App_Data/DataProtectionKeys`, `DataProtection:KeysPath`, or the `taskist-keys` Docker volume). Without it, stored Git tokens, webhook secrets and notification channel URLs cannot be decrypted.

3. Read the [changelog](CHANGELOG.md) for the versions you are skipping. Anything that needs action is listed there under **Upgrade notes**.

## Applying an upgrade

### Docker

```bash
git pull
docker compose up -d --build
```

The `migrate` service applies new migrations and re-runs the seed scripts before the web container starts. The scripts are idempotent, so existing data is left alone and only missing rows (new permissions, menu items, translations) are added.

### Manual install

1. Stop the application.
2. Deploy the new build.
3. Apply migrations:

   ```powershell
   dotnet ef database update --project src/Libraries/Taskist.Data --startup-project src/Libraries/Taskist.Data --connection "<connection string>"
   ```

   Or, without the .NET SDK on the server, run `sql-scripts/01_schema.sql`. It is idempotent and only applies migrations that are missing.

4. Re-run the seed scripts `02_defaults.sql` and `03_locale.sql` (with `sqlcmd`, pass `-f 65001`). New permissions, menu entries and translations only appear after this step.
5. Start the application and check the log for startup warnings.

Migrations are additive: they add tables and nullable columns and do not rewrite existing values.

## Rolling back

Restore both backups from before the upgrade. Rolling back migrations alone is not supported, because later versions may already have written data that older code does not understand.

## Upgrading from releases before the current baseline

The database schema was re-baselined into a single `InitialCreate` migration for the current release line. Databases created by **earlier releases cannot be upgraded in place**: their migration history does not match.

To move data from an older install, set up a fresh database with the current release and copy the data across, or ask in [GitHub Discussions](https://github.com/Taskist/taskist/discussions/categories/q-a) before you start.
