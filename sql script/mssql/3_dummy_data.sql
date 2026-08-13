/* ==========================================================================================================
    Taskist - Sample data (optional)

    Creates fictional clients, projects, modules, reporters and the shared status, task type
    and severity lists, so a fresh installation has something to explore.

    This file is OPTIONAL and is intended for evaluation and development. Do not run it
    against a production database.

    Run it after 1_defaults.sql and 2_locale_resource.sql.

    The script is idempotent: rows are matched on their business key, so re-running it
    inserts only what is missing and never duplicates existing data.

    ----------------------------------------------------------------------------------------------------------
    DISCLAIMER

    The sample content in this file was generated with the assistance of AI.

    - All names, contact details, company titles and URLs are fictional and used purely for demonstration.
    - No real person, organization or entity is represented or referenced.
    - The script is provided "AS IS" without warranty of any kind, including but not limited to
      correctness, completeness, or fitness for a particular purpose.
    - You are free to modify, redistribute or use this script for educational, testing or
      open-source purposes in compliance with applicable laws and licences.
    - The authors assume no legal liability for misuse or misrepresentation of the generated content.

    © 2025 Taskist - https://taskist.org - https://github.com/Taskist/taskist
========================================================================================================== */

SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRANSACTION;

/* ---------------------------------------------------------------------------------------------------------
    Clients
--------------------------------------------------------------------------------------------------------- */

DECLARE @client TABLE
(
    [Name]          NVARCHAR(100) NOT NULL PRIMARY KEY,
    [Description]   NVARCHAR(255) NOT NULL,
    [ContactPerson] NVARCHAR(100) NOT NULL,
    [PhoneNumber]   NVARCHAR(20)  NOT NULL,
    [Email]         NVARCHAR(100) NOT NULL,
    [Website]       NVARCHAR(100) NOT NULL
);

INSERT INTO @client ([Name], [Description], [ContactPerson], [PhoneNumber], [Email], [Website])
VALUES
    (N'TechNova Solutions',      N'Enterprise software provider',            N'Rajesh Kumar', N'0000000000', N'info@taskist.org',    N'https://taskist.org'),
    (N'CodeSphere Technologies', N'Custom web and mobile app development',   N'Anita Patel',  N'0000000000', N'contact@taskist.org', N'https://taskist.org'),
    (N'Orbit Systems',           N'Cloud infrastructure and DevOps solutions', N'Rahul Sharma', N'0000000000', N'support@taskist.org', N'https://taskist.org'),
    (N'Edge Labs',               N'Creative agency specializing in UX/UI',   N'Priya Menon',  N'0000000000', N'hello@taskist.org',   N'https://taskist.org'),
    (N'Core Pvt Ltd',            N'AI-driven software solutions',            N'Amit Verma',   N'0000000000', N'sales@taskist.org',   N'https://taskist.org');

INSERT INTO [Client] ([Name], [Description], [ContactPerson], [PhoneNumber], [Email], [Website], [Active], [Deleted])
SELECT s.[Name], s.[Description], s.[ContactPerson], s.[PhoneNumber], s.[Email], s.[Website], 1, 0
FROM @client AS s
WHERE NOT EXISTS (SELECT 1 FROM [Client] AS t WHERE t.[Name] = s.[Name]);

/* ---------------------------------------------------------------------------------------------------------
    Projects - one per client
--------------------------------------------------------------------------------------------------------- */

DECLARE @project TABLE
(
    [Name]        NVARCHAR(100) NOT NULL PRIMARY KEY,
    [Description] NVARCHAR(255) NOT NULL,
    [StartDate]   DATE          NOT NULL,
    [EndDate]     DATE          NOT NULL,
    [ClientName]  NVARCHAR(100) NOT NULL
);

INSERT INTO @project ([Name], [Description], [StartDate], [EndDate], [ClientName])
VALUES
    (N'Project Atlas',   N'Enterprise workflow automation system',  '2025-01-01', '2025-06-30', N'TechNova Solutions'),
    (N'Project Horizon', N'Mobile-first CRM platform',              '2025-02-01', '2025-07-30', N'CodeSphere Technologies'),
    (N'Project Nimbus',  N'Cloud analytics and monitoring suite',    '2025-03-01', '2025-08-30', N'Orbit Systems'),
    (N'Project Vertex',  N'Design collaboration and feedback tool',  '2025-04-01', '2025-09-30', N'Edge Labs'),
    (N'Project Helix',   N'AI-powered customer insight platform',    '2025-05-01', '2025-10-30', N'Core Pvt Ltd');

INSERT INTO [Project] ([Name], [Description], [StartDate], [EndDate], [ClientId], [Active], [Deleted])
SELECT s.[Name], s.[Description], s.[StartDate], s.[EndDate], c.[Id], 1, 0
FROM @project AS s
INNER JOIN [Client] AS c ON c.[Name] = s.[ClientName]
WHERE NOT EXISTS (SELECT 1 FROM [Project] AS t WHERE t.[Name] = s.[Name]);

/* ---------------------------------------------------------------------------------------------------------
    Modules - the same set for every project
--------------------------------------------------------------------------------------------------------- */

DECLARE @moduleName TABLE ([Name] NVARCHAR(255) NOT NULL PRIMARY KEY);

INSERT INTO @moduleName ([Name])
VALUES (N'Authentication Module'), (N'Reporting Module'), (N'User Management'), (N'Notifications'), (N'Integration Layer');

INSERT INTO [Module] ([Name], [Description], [ProjectId], [Active], [Deleted])
SELECT m.[Name], CONCAT(m.[Name], N' for ', p.[Name]), p.[Id], 1, 0
FROM [Project] AS p
CROSS JOIN @moduleName AS m
WHERE p.[Active] = 1 AND p.[Deleted] = 0
  AND NOT EXISTS (SELECT 1 FROM [Module] AS t WHERE t.[Name] = m.[Name] AND t.[ProjectId] = p.[Id]);

/* ---------------------------------------------------------------------------------------------------------
    Sub modules - the same set for every module
--------------------------------------------------------------------------------------------------------- */

DECLARE @subModuleName TABLE ([Name] NVARCHAR(255) NOT NULL PRIMARY KEY);

INSERT INTO @subModuleName ([Name])
VALUES (N'Setup'), (N'Configuration'), (N'API Endpoints'), (N'UI Components'), (N'Testing');

INSERT INTO [SubModule] ([Name], [Description], [ModuleId], [Active], [Deleted])
SELECT s.[Name], CONCAT(s.[Name], N' for ', m.[Name]), m.[Id], 1, 0
FROM [Module] AS m
CROSS JOIN @subModuleName AS s
WHERE m.[Active] = 1 AND m.[Deleted] = 0
  AND NOT EXISTS (SELECT 1 FROM [SubModule] AS t WHERE t.[Name] = s.[Name] AND t.[ModuleId] = m.[Id]);

/* ---------------------------------------------------------------------------------------------------------
    Status
--------------------------------------------------------------------------------------------------------- */

INSERT INTO [Status] ([Name], [Description], [TextColor], [BackgroundColor], [IconClass], [GroupId], [SystemDefined], [Active], [Deleted])
SELECT s.[Name], s.[Description], s.[TextColor], s.[BackgroundColor], s.[IconClass], s.[GroupId], 1, 1, 0
FROM (VALUES
    (N'New',         N'Issue has been reported but not yet reviewed or assigned.',        N'#FFFFFF', N'#2196F3', N'fa-regular fa-file',          1),
    (N'Assigned',    N'Issue assigned to a developer or team for resolution.',            N'#FFFFFF', N'#3F51B5', N'fa-solid fa-user-check',      2),
    (N'In Progress', N'Work has started on resolving this issue.',                        N'#FFFFFF', N'#FFC107', N'fa-solid fa-spinner fa-spin', 2),
    (N'Resolved',    N'Developer has fixed the issue; pending verification.',             N'#FFFFFF', N'#4CAF50', N'fa-solid fa-check',           5),
    (N'Reopened',    N'Issue was marked resolved but reoccurred or was not fixed properly.', N'#FFFFFF', N'#FF5722', N'fa-solid fa-redo',        4),
    (N'Closed',      N'Issue has been verified and marked as completed.',                 N'#FFFFFF', N'#009688', N'fa-solid fa-lock',            5),
    (N'Deferred',    N'Issue postponed for future releases.',                             N'#FFFFFF', N'#9E9E9E', N'fa-solid fa-clock',           3),
    (N'Duplicate',   N'Issue is a duplicate of another ticket.',                          N'#000000', N'#E0E0E0', N'fa-solid fa-copy',            3),
    (N'Rejected',    N'Issue is invalid or will not be fixed.',                           N'#FFFFFF', N'#9C27B0', N'fa-solid fa-ban',             3)
) AS s ([Name], [Description], [TextColor], [BackgroundColor], [IconClass], [GroupId])
WHERE NOT EXISTS (SELECT 1 FROM [Status] AS t WHERE t.[Name] = s.[Name]);

/* ---------------------------------------------------------------------------------------------------------
    Task types
--------------------------------------------------------------------------------------------------------- */

INSERT INTO [TaskType] ([Name], [Description], [TextColor], [BackgroundColor], [IconClass], [GroupId], [Active], [Deleted])
SELECT s.[Name], s.[Description], s.[TextColor], s.[BackgroundColor], s.[IconClass], s.[GroupId], 1, 0
FROM (VALUES
    (N'To Do',          N'General work items or non-development tasks.',            N'#000000', N'#2196F3', N'fa-regular fa-list-check', 1),
    (N'Change Request', N'Feature modification or improvement request.',            N'#FFFFFF', N'#009688', N'fa-solid fa-wrench',       2),
    (N'Bug',            N'Defect in functionality or unexpected system behavior.',  N'#FFFFFF', N'#F44336', N'fa-solid fa-bug',          3)
) AS s ([Name], [Description], [TextColor], [BackgroundColor], [IconClass], [GroupId])
WHERE NOT EXISTS (SELECT 1 FROM [TaskType] AS t WHERE t.[Name] = s.[Name]);

/* ---------------------------------------------------------------------------------------------------------
    Severity
--------------------------------------------------------------------------------------------------------- */

INSERT INTO [Severity] ([Name], [Description], [TextColor], [BackgroundColor], [IconClass], [GroupId], [Active], [Deleted])
SELECT s.[Name], s.[Description], s.[TextColor], s.[BackgroundColor], s.[IconClass], s.[GroupId], 1, 0
FROM (VALUES
    (N'Trivial',      N'Very low impact or cosmetic issue, does not affect functionality.',            N'#000000', N'#8BC34A', N'fa-regular fa-lightbulb',           1),
    (N'Minor',        N'Low impact issue with minimal user disruption.',                               N'#000000', N'#FFEB3B', N'fa-regular fa-circle-exclamation',  1),
    (N'Major',        N'Significant issue affecting major functionality with a workaround available.', N'#FFFFFF', N'#FF9800', N'fa-solid fa-exclamation-triangle',  2),
    (N'Critical',     N'Severe issue causing data loss or system instability requiring urgent fix.',   N'#FFFFFF', N'#F44336', N'fa-solid fa-bug',                   3),
    (N'Show Stopper', N'Complete system or module failure blocking critical operations.',              N'#FFFFFF', N'#B71C1C', N'fa-solid fa-skull-crossbones',      4)
) AS s ([Name], [Description], [TextColor], [BackgroundColor], [IconClass], [GroupId])
WHERE NOT EXISTS (SELECT 1 FROM [Severity] AS t WHERE t.[Name] = s.[Name]);

/* ---------------------------------------------------------------------------------------------------------
    Reporters - a different set per project
--------------------------------------------------------------------------------------------------------- */

DECLARE @reporter TABLE
(
    [ProjectName] NVARCHAR(100) NOT NULL,
    [Name]        NVARCHAR(100) NOT NULL,
    [Description] NVARCHAR(255) NOT NULL,
    PRIMARY KEY ([ProjectName], [Name])
);

INSERT INTO @reporter ([ProjectName], [Name], [Description])
VALUES
    (N'Project Atlas',   N'IT',         N'Handles internal systems and project infrastructure'),
    (N'Project Atlas',   N'QA',         N'Responsible for testing modules and submodules'),
    (N'Project Atlas',   N'DevOps',     N'Manages CI/CD and deployment pipelines'),
    (N'Project Atlas',   N'Support',    N'Handles client-reported bugs and tickets'),
    (N'Project Atlas',   N'Admin',      N'Manages user permissions and data policies'),

    (N'Project Horizon', N'Accounts',   N'Manages project billing and transactions'),
    (N'Project Horizon', N'Finance',    N'Tracks budgets and expenses'),
    (N'Project Horizon', N'HR',         N'Handles staffing and project resources'),
    (N'Project Horizon', N'IT',         N'Handles internal systems and infrastructure'),
    (N'Project Horizon', N'QA',         N'Performs regression and integration testing'),

    (N'Project Nimbus',  N'R&D',        N'Responsible for new feature prototyping'),
    (N'Project Nimbus',  N'Product',    N'Manages product requirements and backlog'),
    (N'Project Nimbus',  N'Design',     N'Creates and reviews UI/UX designs'),
    (N'Project Nimbus',  N'IT',         N'Handles system configurations and builds'),
    (N'Project Nimbus',  N'QA',         N'Performs test automation and release validation'),

    (N'Project Vertex',  N'Analytics',  N'Collects and interprets usage metrics'),
    (N'Project Vertex',  N'Security',   N'Ensures app and data security compliance'),
    (N'Project Vertex',  N'DevOps',     N'Oversees release pipelines and infrastructure'),
    (N'Project Vertex',  N'Support',    N'Resolves reported user issues'),
    (N'Project Vertex',  N'Finance',    N'Monitors financial operations'),

    (N'Project Helix',   N'Operations', N'Manages internal task assignments'),
    (N'Project Helix',   N'Support',    N'Handles technical and client support'),
    (N'Project Helix',   N'Marketing',  N'Manages campaigns and outreach'),
    (N'Project Helix',   N'QA',         N'Ensures build and release quality'),
    (N'Project Helix',   N'Admin',      N'Controls permissions and workflow settings');

INSERT INTO [Reporter] ([Name], [Description], [ProjectId], [Active], [Deleted])
SELECT s.[Name], s.[Description], p.[Id], 1, 0
FROM @reporter AS s
INNER JOIN [Project] AS p ON p.[Name] = s.[ProjectName]
WHERE NOT EXISTS (SELECT 1 FROM [Reporter] AS t WHERE t.[Name] = s.[Name] AND t.[ProjectId] = p.[Id]);

COMMIT TRANSACTION;

PRINT 'Sample data seeded.';
