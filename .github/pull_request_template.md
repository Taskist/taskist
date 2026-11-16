## What and why

<!-- What does this change, and why? Link the issue or discussion: "Closes #123". -->

## How it was tested

<!-- Steps you followed, and any new tests. -->

## Checklist

- [ ] `dotnet build src/Taskist.sln` and `dotnet test src/Taskist.sln` pass
- [ ] Entity changes come with an EF migration, and the migration is additive (no dropped columns, no rewritten data)
- [ ] New UI text is added to `sql-scripts/03_locale.sql`
- [ ] New menu entries or permissions are added to `sql-scripts/02_defaults.sql`
- [ ] No inline scripts or styles, no CDN links, no `ViewBag`/`ViewData`, no comments in `.cs` files
- [ ] New dependencies are MIT, Apache or BSD licensed
- [ ] This touches authentication, authorization, file uploads, outbound HTTP or query building (please describe the impact above)
