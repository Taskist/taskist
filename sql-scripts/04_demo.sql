/* ==========================================================================================================
    Taskist - Sample data (optional)

    One sample client, project, module and sub-module, so a fresh installation has something to explore.
    For evaluation and development only: skip it on a real deployment.

    Run after 02_defaults.sql. Idempotent: rows are matched on their name, so re-running inserts only what
    is missing.
========================================================================================================== */

SET NOCOUNT ON;
SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF NOT EXISTS (SELECT 1 FROM [Client] WHERE [Name] = N'Acme Corporation')
    INSERT INTO [Client] ([Name], [Description], [ContactPerson], [PhoneNumber], [Email], [Website], [Active], [Deleted])
    VALUES (N'Acme Corporation', N'Sample client for getting started.', N'John Doe', N'0000000000', N'contact@taskist.org', N'https://acme.test', 1, 0);

DECLARE @clientId INT = (SELECT TOP (1) [Id] FROM [Client] WHERE [Name] = N'Acme Corporation' ORDER BY [Id]);

IF NOT EXISTS (SELECT 1 FROM [Project] WHERE [Name] = N'Sample Project')
    INSERT INTO [Project] ([Name], [Description], [StartDate], [EndDate], [ClientId], [ProjectManagerId], [TeamLeadId], [GitProjectReference], [Active], [Deleted])
    VALUES (N'Sample Project', N'Sample project for getting started.', '2024-01-01', NULL, @clientId, NULL, NULL, NULL, 1, 0);

DECLARE @projectId INT = (SELECT TOP (1) [Id] FROM [Project] WHERE [Name] = N'Sample Project' ORDER BY [Id]);

IF NOT EXISTS (SELECT 1 FROM [Module] WHERE [Name] = N'General' AND [ProjectId] = @projectId)
    INSERT INTO [Module] ([Name], [Description], [ProjectId], [Active], [Deleted])
    VALUES (N'General', N'Default module.', @projectId, 1, 0);

DECLARE @moduleId INT = (SELECT TOP (1) [Id] FROM [Module] WHERE [Name] = N'General' AND [ProjectId] = @projectId ORDER BY [Id]);

IF NOT EXISTS (SELECT 1 FROM [SubModule] WHERE [Name] = N'General' AND [ModuleId] = @moduleId)
    INSERT INTO [SubModule] ([Name], [Description], [ModuleId], [OwnerId], [Active], [Deleted])
    VALUES (N'General', N'Default sub-module.', @moduleId, NULL, 1, 0);

COMMIT TRANSACTION;
PRINT 'Sample data seeded.';
