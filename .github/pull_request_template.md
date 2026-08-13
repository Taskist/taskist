<!--
Thanks for contributing to Taskist!

For anything beyond a small fix, please discuss the change first via an issue or on
Zulip (https://taskist.zulipchat.com/#narrow/channel/539614-dev) so effort is not
spent on something that does not fit the project's direction.
-->

## What does this change?

<!-- A short description of the change and why it is needed. -->

## Related issue

<!-- e.g. Closes #123 -->

## Type of change

- [ ] Bug fix
- [ ] New feature
- [ ] Breaking change
- [ ] Documentation
- [ ] Refactoring / code quality
- [ ] Build, CI or tooling

## How was this tested?

<!-- Describe what you ran, and what a reviewer should try. -->

- [ ] `dotnet build src/Taskist.sln` succeeds
- [ ] `dotnet test src/Taskist.sln` passes
- [ ] Verified manually in a browser

## Database changes

- [ ] This change does not touch entities or the schema
- [ ] I added an EF Core migration (`dotnet ef migrations add ...`)
- [ ] I verified the migration applies cleanly to an existing database

## Checklist

- [ ] The code follows the conventions of the surrounding files
- [ ] I have not committed secrets, connection strings or credentials
- [ ] I updated the documentation where behaviour changed
- [ ] Configuration I added has a sensible and secure default

## Security considerations

<!--
Call out anything touching authentication, permissions, uploads, or data access,
even if it seems minor. Write "None" if this does not apply.
-->
