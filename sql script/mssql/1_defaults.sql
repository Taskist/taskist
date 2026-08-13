/* ==========================================================================================================
    Taskist - Default reference data

    Seeds the menus, roles, language, permissions and the initial administrator account.
    Run this after applying the EF Core migrations, and before 2_locale_resource.sql.

    The script is idempotent: rows are matched on their business key, so re-running it
    inserts only what is missing and never duplicates existing data.

    Identity values are assigned by the database rather than hard coded, so the script is
    safe to run against an instance that already holds data.
========================================================================================================== */

SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRANSACTION;

/* ---------------------------------------------------------------------------------------------------------
    Menu
--------------------------------------------------------------------------------------------------------- */

DECLARE @menu TABLE
(
    [Name]           NVARCHAR(50)   NOT NULL,
    [SystemName]     NVARCHAR(100)  NOT NULL,
    [Code]           INT            NOT NULL,
    [ActionName]     NVARCHAR(100)  NULL,
    [ControllerName] NVARCHAR(100)  NULL,
    [ParentCode]     INT            NOT NULL,
    [Icon]           NVARCHAR(100)  NULL,
    [DisplayOrder]   INT            NOT NULL,
    [Permission]     NVARCHAR(100)  NOT NULL,
    [Active]         BIT            NOT NULL
);

INSERT INTO @menu ([Name], [SystemName], [Code], [ActionName], [ControllerName], [ParentCode], [Icon], [DisplayOrder], [Permission], [Active])
VALUES
    (N'Work Items',     N'WorkItems',     100, NULL,     NULL,             0,   N'fas fa-check-double',  1, N'WorkItem.ManageWorkItems',           1),
    (N'Backlog',        N'Backlog',       101, N'Index', N'Backlog',       100, NULL,                    1, N'WorkItem.ManageBacklog',            1),
    (N'Board',          N'Board',         102, N'Index', N'Board',         100, NULL,                    2, N'WorkItem.ManageBoard',              1),
    (N'Sprints',        N'Sprints',       103, N'Index', N'Sprint',        100, NULL,                    3, N'WorkItem.ManageSprints',            1),
    (N'Configuration',  N'Configuration', 200, NULL,     NULL,             0,   N'fas fa-layer-group',   1, N'Configuration.ManageConfiguration', 1),
    (N'Users',          N'Users',         201, N'Index', N'User',          200, NULL,                    2, N'Configuration.ManageUser',          1),
    (N'Client',         N'Client',        202, N'Index', N'Client',        200, NULL,                    3, N'Configuration.ManageClient',        1),
    (N'Project',        N'Project',       203, N'Index', N'Project',       200, NULL,                    4, N'Configuration.ManageProject',       1),
    (N'Module',         N'Module',        204, N'Index', N'Module',        200, NULL,                    5, N'Configuration.ManageModule',        1),
    (N'Sub Module',     N'SubModule',     205, N'Index', N'SubModule',     200, NULL,                    6, N'Configuration.ManageSubModule',     1),
    (N'Severity',       N'Severity',      206, N'Index', N'Severity',      200, NULL,                    7, N'Configuration.ManageSeverity',      1),
    (N'Status',         N'Status',        207, N'Index', N'Status',        200, NULL,                    8, N'Configuration.ManageStatus',        1),
    (N'Task Type',      N'TaskType',      208, N'Index', N'TaskType',      200, NULL,                    9, N'Configuration.ManageTaskType',      1),
    (N'Reporter',       N'Reporter',      209, N'Index', N'Reporter',      200, NULL,                   10, N'Configuration.ManageReporter',      1),
    (N'User Role',      N'UserRole',      210, N'Index', N'UserRole',      200, NULL,                   11, N'Configuration.ManageUserRole',      1),
    (N'Email Account',  N'EmailAccount',  211, N'Index', N'EmailAccount',  200, NULL,                   15, N'Configuration.ManageEmailAccount',  1),
    (N'Email Template', N'EmailTemplate', 212, N'Index', N'EmailTemplate', 200, NULL,                   16, N'Configuration.ManageEmailTemplate', 1),
    (N'Language',       N'Language',      213, N'Index', N'Language',      200, NULL,                   17, N'Configuration.ManageLanguage',      1);

INSERT INTO [Menu] ([Name], [SystemName], [Code], [ActionName], [ControllerName], [ParentCode], [Icon], [DisplayOrder], [Permission], [Active])
SELECT s.[Name], s.[SystemName], s.[Code], s.[ActionName], s.[ControllerName], s.[ParentCode], s.[Icon], s.[DisplayOrder], s.[Permission], s.[Active]
FROM @menu AS s
WHERE NOT EXISTS (SELECT 1 FROM [Menu] AS t WHERE t.[SystemName] = s.[SystemName]);

/* ---------------------------------------------------------------------------------------------------------
    User roles
--------------------------------------------------------------------------------------------------------- */

DECLARE @userRole TABLE
(
    [Name]          NVARCHAR(100) NOT NULL,
    [SystemName]    NVARCHAR(100) NOT NULL,
    [Description]   NVARCHAR(255) NULL,
    [SystemDefined] BIT           NOT NULL,
    [Active]        BIT           NOT NULL
);

INSERT INTO @userRole ([Name], [SystemName], [Description], [SystemDefined], [Active])
VALUES
    (N'Registered',           N'Registered',          N'Default role for all users.', 1, 1),
    (N'System Administrator', N'SystemAdministrator', N'System administrator.',       1, 1),
    (N'Developer',            N'Developer',           N'Developers',                  1, 1),
    (N'Reporter',             N'Reporter',            N'Reporters',                   1, 1);

INSERT INTO [UserRole] ([Name], [SystemName], [Description], [SystemDefined], [Active])
SELECT s.[Name], s.[SystemName], s.[Description], s.[SystemDefined], s.[Active]
FROM @userRole AS s
WHERE NOT EXISTS (SELECT 1 FROM [UserRole] AS t WHERE t.[SystemName] = s.[SystemName]);

/* ---------------------------------------------------------------------------------------------------------
    Language
--------------------------------------------------------------------------------------------------------- */

INSERT INTO [Language] ([Name], [LanguageCulture], [DisplayName], [Rtl], [DisplayOrder], [Active])
SELECT s.[Name], s.[LanguageCulture], s.[DisplayName], s.[Rtl], s.[DisplayOrder], s.[Active]
FROM (VALUES
    (N'English (India)', N'en-IN', N'English (India)', CAST(0 AS BIT), 1, CAST(1 AS BIT))
) AS s ([Name], [LanguageCulture], [DisplayName], [Rtl], [DisplayOrder], [Active])
WHERE NOT EXISTS (SELECT 1 FROM [Language] AS t WHERE t.[LanguageCulture] = s.[LanguageCulture]);

/* ---------------------------------------------------------------------------------------------------------
    Permissions

    Every menu contributes a permission, and the role group is the segment before the dot
    (for example "Configuration.ManageUser" belongs to the "Configuration" group).
--------------------------------------------------------------------------------------------------------- */

INSERT INTO [UserRolePermission] ([Name], [SystemName], [RoleGroup], [SystemDefined])
SELECT s.[Name], s.[SystemName], s.[RoleGroup], s.[SystemDefined]
FROM (VALUES
    (N'Dashboard', N'General.ManageDashboard', N'General', CAST(1 AS BIT))
) AS s ([Name], [SystemName], [RoleGroup], [SystemDefined])
WHERE NOT EXISTS (SELECT 1 FROM [UserRolePermission] AS t WHERE t.[SystemName] = s.[SystemName]);

INSERT INTO [UserRolePermission] ([Name], [SystemName], [RoleGroup], [SystemDefined])
SELECT m.[Name], m.[Permission], LEFT(m.[Permission], CHARINDEX('.', m.[Permission]) - 1), 1
FROM [Menu] AS m
WHERE CHARINDEX('.', m.[Permission]) > 1
  AND NOT EXISTS (SELECT 1 FROM [UserRolePermission] AS t WHERE t.[SystemName] = m.[Permission]);

/* ---------------------------------------------------------------------------------------------------------
    Administrator account

    The stored hash is the legacy SHA1 format (HashFormat 0). Taskist verifies it on the
    first sign-in and transparently upgrades it to PBKDF2, so no reset is required.
--------------------------------------------------------------------------------------------------------- */

DECLARE @languageId INT = (SELECT TOP (1) [Id] FROM [Language] WHERE [LanguageCulture] = N'en-IN' ORDER BY [Id]);

IF @languageId IS NULL
BEGIN
    ROLLBACK TRANSACTION;
    THROW 50001, 'Language "en-IN" is missing. The default data could not be seeded.', 1;
END

IF NOT EXISTS (SELECT 1 FROM [User] WHERE [Email] = N'admin@taskist.org')
BEGIN
    INSERT INTO [User] ([Code], [FirstName], [LastName], [Email], [GenderId], [LanguageId], [SystemAccount],
                        [FailedLoginAttempts], [LastIPAddress], [LastLoginDate], [LastActivityDate],
                        [Locked], [Status], [Deleted])
    VALUES (N'f2e8296a-5b13-467d-9eb9-60a9f18ffeee', N'Admin', N'Taskist', N'admin@taskist.org', 1, @languageId, 1,
            0, N'0.0.0.0', NULL, CAST(N'2025-10-02T17:49:12.0317624' AS DATETIME2),
            0, 1, 0);
END

DECLARE @adminUserId INT = (SELECT TOP (1) [Id] FROM [User] WHERE [Email] = N'admin@taskist.org' ORDER BY [Id]);

-- Default password: Admin@12345  (SHA1 legacy format; upgraded to PBKDF2 on first sign-in).
-- CHANGE THIS IMMEDIATELY after the first login.
INSERT INTO [UserPassword] ([UserId], [Password], [PasswordSalt], [HashFormat], [CreatedOn])
SELECT @adminUserId, N'A8F174FF64311508687B4E00C5CB62B8AEB4D9B9', N'n1DRhPvrjpXMEA==', 0,
       CAST(N'2025-10-02T16:40:13.5020809' AS DATETIME2)
WHERE NOT EXISTS (SELECT 1 FROM [UserPassword] WHERE [UserId] = @adminUserId);

/* ---------------------------------------------------------------------------------------------------------
    Role to permission mapping

    Both halves of the composite key are compared, so a permission can be granted to
    several roles and re-running the script never duplicates a grant.
--------------------------------------------------------------------------------------------------------- */

-- every registered user can reach the dashboard
INSERT INTO [UserRolePermissionMap] ([PermissionId], [UserRoleId])
SELECT p.[Id], r.[Id]
FROM [UserRolePermission] AS p
CROSS JOIN [UserRole] AS r
WHERE p.[SystemName] = N'General.ManageDashboard'
  AND r.[SystemName] = N'Registered'
  AND NOT EXISTS (SELECT 1 FROM [UserRolePermissionMap] AS m
                  WHERE m.[PermissionId] = p.[Id] AND m.[UserRoleId] = r.[Id]);

-- the administrator receives everything else
INSERT INTO [UserRolePermissionMap] ([PermissionId], [UserRoleId])
SELECT p.[Id], r.[Id]
FROM [UserRolePermission] AS p
CROSS JOIN [UserRole] AS r
WHERE p.[SystemName] <> N'General.ManageDashboard'
  AND r.[SystemName] = N'SystemAdministrator'
  AND NOT EXISTS (SELECT 1 FROM [UserRolePermissionMap] AS m
                  WHERE m.[PermissionId] = p.[Id] AND m.[UserRoleId] = r.[Id]);

/* ---------------------------------------------------------------------------------------------------------
    User to role mapping
--------------------------------------------------------------------------------------------------------- */

INSERT INTO [UserRoleMap] ([UserId], [UserRoleId])
SELECT @adminUserId, r.[Id]
FROM [UserRole] AS r
WHERE r.[SystemName] IN (N'Registered', N'SystemAdministrator')
  AND @adminUserId IS NOT NULL
  AND NOT EXISTS (SELECT 1 FROM [UserRoleMap] AS m
                  WHERE m.[UserId] = @adminUserId AND m.[UserRoleId] = r.[Id]);

COMMIT TRANSACTION;

PRINT 'Default data seeded.';
