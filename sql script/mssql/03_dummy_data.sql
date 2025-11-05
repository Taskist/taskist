/* ==========================================================================================================
    DISCLAIMER
    ----------------------------------------------------------------------------------------------------------
    This SQL script was **AI-generated** using OpenAI's GPT technology to create dummy seed data 
    for an open-source, public project. 

    Important Notes:

    - All names, contact details, company titles, and URLs are **fictional** and used purely for demonstration.  
    - No real person, organization, or entity is represented or referenced.
    - The script is provided **"AS IS"** without any warranty or guarantee of any kind, 
      including but not limited to correctness, completeness, or fitness for a particular purpose.
    - Users are free to modify, redistribute, or use this script for educational, testing, or open-source purposes 
      in compliance with applicable laws and licenses.
    - The author(s) and AI provider assume **no legal liability** for misuse or misrepresentation of the generated content.

    © 2025 — Taskist, https://taskist.org https://github.com/Taskist/taskist
========================================================================================================== */

-- Client
DECLARE @Client1 VARCHAR(100) = 'TechNova Solutions';
DECLARE @Client2 VARCHAR(100) = 'CodeSphere Technologies';
DECLARE @Client3 VARCHAR(100) = 'Orbit Systems';
DECLARE @Client4 VARCHAR(100) = 'Edge Labs';
DECLARE @Client5 VARCHAR(100) = 'Core Pvt Ltd';

IF NOT EXISTS (SELECT 1 FROM Client WHERE Name = @Client1)
    INSERT INTO Client (Name, Description, ContactPerson, PhoneNumber, Email, Website, Active, Deleted)
    VALUES (@Client1, 'Enterprise software provider', 'Rajesh Kumar', '0000000000', 'info@taskist.org', 'https://taskist.org', 1, 0);

IF NOT EXISTS (SELECT 1 FROM Client WHERE Name = @Client2)
    INSERT INTO Client (Name, Description, ContactPerson, PhoneNumber, Email, Website, Active, Deleted)
    VALUES (@Client2, 'Custom web and mobile app development', 'Anita Patel', '0000000000', 'contact@taskist.org', 'https://taskist.org', 1, 0);

IF NOT EXISTS (SELECT 1 FROM Client WHERE Name = @Client3)
    INSERT INTO Client (Name, Description, ContactPerson, PhoneNumber, Email, Website, Active, Deleted)
    VALUES (@Client3, 'Cloud infrastructure and DevOps solutions', 'Rahul Sharma', '0000000000', 'support@taskist.org', 'https://taskist.org', 1, 0);

IF NOT EXISTS (SELECT 1 FROM Client WHERE Name = @Client4)
    INSERT INTO Client (Name, Description, ContactPerson, PhoneNumber, Email, Website, Active, Deleted)
    VALUES (@Client4, 'Creative agency specializing in UX/UI', 'Priya Menon', '0000000000', 'hello@taskist.org', 'https://taskist.org', 1, 0);

IF NOT EXISTS (SELECT 1 FROM Client WHERE Name = @Client5)
    INSERT INTO Client (Name, Description, ContactPerson, PhoneNumber, Email, Website, Active, Deleted)
    VALUES (@Client5, 'AI-driven software solutions', 'Amit Verma', '0000000000', 'sales@taskist.org', 'https://taskist.org', 1, 0);

-- Project (Each client gets one)
DECLARE @ClientId1 INT = (SELECT TOP 1 Id FROM Client WHERE Name = @Client1);
DECLARE @ClientId2 INT = (SELECT TOP 1 Id FROM Client WHERE Name = @Client2);
DECLARE @ClientId3 INT = (SELECT TOP 1 Id FROM Client WHERE Name = @Client3);
DECLARE @ClientId4 INT = (SELECT TOP 1 Id FROM Client WHERE Name = @Client4);
DECLARE @ClientId5 INT = (SELECT TOP 1 Id FROM Client WHERE Name = @Client5);

IF NOT EXISTS (SELECT 1 FROM Project WHERE Name = 'Project Atlas')
    INSERT INTO Project (Name, Description, StartDate, EndDate, ClientId, Active, Deleted)
    VALUES ('Project Atlas', 'Enterprise workflow automation system', '2025-01-01', '2025-06-30', @ClientId1, 1, 0);

IF NOT EXISTS (SELECT 1 FROM Project WHERE Name = 'Project Horizon')
    INSERT INTO Project (Name, Description, StartDate, EndDate, ClientId, Active, Deleted)
    VALUES ('Project Horizon', 'Mobile-first CRM platform', '2025-02-01', '2025-07-30', @ClientId2, 1, 0);

IF NOT EXISTS (SELECT 1 FROM Project WHERE Name = 'Project Nimbus')
    INSERT INTO Project (Name, Description, StartDate, EndDate, ClientId, Active, Deleted)
    VALUES ('Project Nimbus', 'Cloud analytics and monitoring suite', '2025-03-01', '2025-08-30', @ClientId3, 1, 0);

IF NOT EXISTS (SELECT 1 FROM Project WHERE Name = 'Project Vertex')
    INSERT INTO Project (Name, Description, StartDate, EndDate, ClientId, Active, Deleted)
    VALUES ('Project Vertex', 'Design collaboration and feedback tool', '2025-04-01', '2025-09-30', @ClientId4, 1, 0);

IF NOT EXISTS (SELECT 1 FROM Project WHERE Name = 'Project Helix')
    INSERT INTO Project (Name, Description, StartDate, EndDate, ClientId, Active, Deleted)
    VALUES ('Project Helix', 'AI-powered customer insight platform', '2025-05-01', '2025-10-30', @ClientId5, 1, 0);
GO

-- Module (5 per project)
DECLARE @Project TABLE (Id INT, Name NVARCHAR(255));
INSERT INTO @Project (Id, Name)
SELECT Id, Name FROM Project WHERE Active = 1 AND Deleted = 0;

DECLARE @ModuleNames TABLE (ModuleName NVARCHAR(255));
INSERT INTO @ModuleNames (ModuleName)
VALUES ('Authentication Module'), ('Reporting Module'), ('User Management'), ('Notifications'), ('Integration Layer');

DECLARE @ProjId INT, @ProjName NVARCHAR(255), @ModName NVARCHAR(255);

DECLARE proj_cursor CURSOR FOR SELECT Id, Name FROM @Project;
OPEN proj_cursor;
FETCH NEXT FROM proj_cursor INTO @ProjId, @ProjName;

WHILE @@FETCH_STATUS = 0
BEGIN
    DECLARE mod_cursor CURSOR FOR SELECT ModuleName FROM @ModuleNames;
    OPEN mod_cursor;
    FETCH NEXT FROM mod_cursor INTO @ModName;

    WHILE @@FETCH_STATUS = 0
    BEGIN
        IF NOT EXISTS (SELECT 1 FROM Module WHERE Name = @ModName AND ProjectId = @ProjId)
            INSERT INTO Module (Name, Description, ProjectId, Active, Deleted)
            VALUES (@ModName, CONCAT(@ModName, ' for ', @ProjName), @ProjId, 1, 0);

        FETCH NEXT FROM mod_cursor INTO @ModName;
    END

    CLOSE mod_cursor;
    DEALLOCATE mod_cursor;

    FETCH NEXT FROM proj_cursor INTO @ProjId, @ProjName;
END

CLOSE proj_cursor;
DEALLOCATE proj_cursor;
GO

-- SubModules (5 per module)
DECLARE @Module TABLE (Id INT, Name NVARCHAR(255));
INSERT INTO @Module (Id, Name)
SELECT Id, Name FROM Module WHERE Active = 1 AND Deleted = 0;

DECLARE @SubNames TABLE (SubName NVARCHAR(255));
INSERT INTO @SubNames (SubName)
VALUES ('Setup'), ('Configuration'), ('API Endpoints'), ('UI Components'), ('Testing');

DECLARE @ModuleId INT, @ModuleName NVARCHAR(255), @SubName NVARCHAR(255);

DECLARE mod_cursor2 CURSOR FOR SELECT Id, Name FROM @Module;
OPEN mod_cursor2;
FETCH NEXT FROM mod_cursor2 INTO @ModuleId, @ModuleName;

WHILE @@FETCH_STATUS = 0
BEGIN
    DECLARE sub_cursor CURSOR FOR SELECT SubName FROM @SubNames;
    OPEN sub_cursor;
    FETCH NEXT FROM sub_cursor INTO @SubName;

    WHILE @@FETCH_STATUS = 0
    BEGIN
        IF NOT EXISTS (SELECT 1 FROM SubModule WHERE Name = @SubName AND ModuleId = @ModuleId)
            INSERT INTO SubModule (Name, Description, ModuleId, Active, Deleted)
            VALUES (@SubName, CONCAT(@SubName, ' part of ', @ModuleName), @ModuleId, 1, 0);

        FETCH NEXT FROM sub_cursor INTO @SubName;
    END

    CLOSE sub_cursor;
    DEALLOCATE sub_cursor;

    FETCH NEXT FROM mod_cursor2 INTO @ModuleId, @ModuleName;
END

CLOSE mod_cursor2;
DEALLOCATE mod_cursor2;
GO
-- Status
IF NOT EXISTS (SELECT 1 FROM Status WHERE Name = 'New')
INSERT INTO Status (Name, Description, TextColor, BackgroundColor, IconClass, GroupId, SystemDefined, Active, Deleted)
VALUES ('New', 'Issue has been reported but not yet reviewed or assigned.', '#FFFFFF', '#2196F3', 'fa-regular fa-file', 1, 1, 1, 0);

IF NOT EXISTS (SELECT 1 FROM Status WHERE Name = 'Assigned')
INSERT INTO Status (Name, Description, TextColor, BackgroundColor, IconClass, GroupId, SystemDefined, Active, Deleted)
VALUES ('Assigned', 'Issue assigned to a developer or team for resolution.', '#FFFFFF', '#3F51B5', 'fa-solid fa-user-check', 2, 1, 1, 0);

IF NOT EXISTS (SELECT 1 FROM Status WHERE Name = 'In Progress')
INSERT INTO Status (Name, Description, TextColor, BackgroundColor, IconClass, GroupId, SystemDefined, Active, Deleted)
VALUES ('In Progress', 'Work has started on resolving this issue.', '#FFFFFF', '#FFC107', 'fa-solid fa-spinner fa-spin', 2, 1, 1, 0);

IF NOT EXISTS (SELECT 1 FROM Status WHERE Name = 'Resolved')
INSERT INTO Status (Name, Description, TextColor, BackgroundColor, IconClass, GroupId, SystemDefined, Active, Deleted)
VALUES ('Resolved', 'Developer has fixed the issue; pending verification.', '#FFFFFF', '#4CAF50', 'fa-solid fa-check', 5, 1, 1, 0);

IF NOT EXISTS (SELECT 1 FROM Status WHERE Name = 'Reopened')
INSERT INTO Status (Name, Description, TextColor, BackgroundColor, IconClass, GroupId, SystemDefined, Active, Deleted)
VALUES ('Reopened', 'Issue was marked resolved but reoccurred or wasn’t fixed properly.', '#FFFFFF', '#FF5722', 'fa-solid fa-redo', 4, 1, 1, 0);

IF NOT EXISTS (SELECT 1 FROM Status WHERE Name = 'Closed')
INSERT INTO Status (Name, Description, TextColor, BackgroundColor, IconClass, GroupId, SystemDefined, Active, Deleted)
VALUES ('Closed', 'Issue has been verified and marked as completed.', '#FFFFFF', '#009688', 'fa-solid fa-lock', 5, 1, 1, 0);

IF NOT EXISTS (SELECT 1 FROM Status WHERE Name = 'Deferred')
INSERT INTO Status (Name, Description, TextColor, BackgroundColor, IconClass, GroupId, SystemDefined, Active, Deleted)
VALUES ('Deferred', 'Issue postponed for future releases.', '#FFFFFF', '#9E9E9E', 'fa-solid fa-clock', 3, 1, 1, 0);

IF NOT EXISTS (SELECT 1 FROM Status WHERE Name = 'Duplicate')
INSERT INTO Status (Name, Description, TextColor, BackgroundColor, IconClass, GroupId, SystemDefined, Active, Deleted)
VALUES ('Duplicate', 'Issue is a duplicate of another ticket.', '#000000', '#E0E0E0', 'fa-solid fa-copy', 3, 1, 1, 0);

IF NOT EXISTS (SELECT 1 FROM Status WHERE Name = 'Rejected')
INSERT INTO Status (Name, Description, TextColor, BackgroundColor, IconClass, GroupId, SystemDefined, Active, Deleted)
VALUES ('Rejected', 'Issue is invalid or won’t be fixed.', '#FFFFFF', '#9C27B0', 'fa-solid fa-ban', 3, 1, 1, 0);
GO
-- TaskType 
IF NOT EXISTS (SELECT 1 FROM TaskType WHERE Name = 'To Do')
INSERT INTO TaskType (Name, Description, TextColor, BackgroundColor, IconClass, GroupId, Active, Deleted)
VALUES ('To Do', 'General work items or non-development tasks.', '#000000', '#2196F3', 'fa-regular fa-list-check', 1, 1, 0);

IF NOT EXISTS (SELECT 1 FROM TaskType WHERE Name = 'Change Request')
INSERT INTO TaskType (Name, Description, TextColor, BackgroundColor, IconClass, GroupId, Active, Deleted)
VALUES ('Change Request', 'Feature modification or improvement request.', '#FFFFFF', '#009688', 'fa-solid fa-wrench', 2, 1, 0);

IF NOT EXISTS (SELECT 1 FROM TaskType WHERE Name = 'Bug')
INSERT INTO TaskType (Name, Description, TextColor, BackgroundColor, IconClass, GroupId, Active, Deleted)
VALUES ('Bug', 'Defect in functionality or unexpected system behavior.', '#FFFFFF', '#F44336', 'fa-solid fa-bug', 3, 1, 0);
GO
-- Severity
IF NOT EXISTS (SELECT 1 FROM Severity WHERE Name = 'Trivial')
INSERT INTO Severity (Name, Description, TextColor, BackgroundColor, IconClass, GroupId, Active, Deleted)
VALUES ('Trivial', 'Very low impact or cosmetic issue, does not affect functionality.', '#000000', '#8BC34A', 'fa-regular fa-lightbulb', 1, 1, 0);

IF NOT EXISTS (SELECT 1 FROM Severity WHERE Name = 'Minor')
INSERT INTO Severity (Name, Description, TextColor, BackgroundColor, IconClass, GroupId, Active, Deleted)
VALUES ('Minor', 'Low impact issue with minimal user disruption.', '#000000', '#FFEB3B', 'fa-regular fa-circle-exclamation', 1, 1, 0);

IF NOT EXISTS (SELECT 1 FROM Severity WHERE Name = 'Major')
INSERT INTO Severity (Name, Description, TextColor, BackgroundColor, IconClass, GroupId, Active, Deleted)
VALUES ('Major', 'Significant issue affecting major functionality with a workaround available.', '#FFFFFF', '#FF9800', 'fa-solid fa-exclamation-triangle', 2, 1, 0);

IF NOT EXISTS (SELECT 1 FROM Severity WHERE Name = 'Critical')
INSERT INTO Severity (Name, Description, TextColor, BackgroundColor, IconClass, GroupId, Active, Deleted)
VALUES ('Critical', 'Severe issue causing data loss or system instability requiring urgent fix.', '#FFFFFF', '#F44336', 'fa-solid fa-bug', 3, 1, 0);

IF NOT EXISTS (SELECT 1 FROM Severity WHERE Name = 'Show Stopper')
INSERT INTO Severity (Name, Description, TextColor, BackgroundColor, IconClass, GroupId, Active, Deleted)
VALUES ('Show Stopper', 'Complete system or module failure blocking critical operations.', '#FFFFFF', '#B71C1C', 'fa-solid fa-skull-crossbones', 4, 1, 0);
GO

-- Reporter
DECLARE @ProjectId INT;

--------------------------------------------
-- Project Atlas
--------------------------------------------
SELECT @ProjectId = Id FROM Project WHERE Name = 'Project Atlas';
IF @ProjectId IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM Reporter WHERE Name = 'IT' AND ProjectId = @ProjectId)
        INSERT INTO Reporter ([Name], [Description], [ProjectId], [Active], [Deleted])
        VALUES ('IT', 'Handles internal systems and project infrastructure', @ProjectId, 1, 0);

    IF NOT EXISTS (SELECT 1 FROM Reporter WHERE Name = 'QA' AND ProjectId = @ProjectId)
        INSERT INTO Reporter VALUES ('QA', 'Responsible for testing modules and submodules', @ProjectId, 1, 0);

    IF NOT EXISTS (SELECT 1 FROM Reporter WHERE Name = 'DevOps' AND ProjectId = @ProjectId)
        INSERT INTO Reporter VALUES ('DevOps', 'Manages CI/CD and deployment pipelines', @ProjectId, 1, 0);

    IF NOT EXISTS (SELECT 1 FROM Reporter WHERE Name = 'Support' AND ProjectId = @ProjectId)
        INSERT INTO Reporter VALUES ('Support', 'Handles client-reported bugs and tickets', @ProjectId, 1, 0);

    IF NOT EXISTS (SELECT 1 FROM Reporter WHERE Name = 'Admin' AND ProjectId = @ProjectId)
        INSERT INTO Reporter VALUES ('Admin', 'Manages user permissions and data policies', @ProjectId, 1, 0);
END;

--------------------------------------------
-- Project Horizon
--------------------------------------------
SELECT @ProjectId = Id FROM Project WHERE Name = 'Project Horizon';
IF @ProjectId IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM Reporter WHERE Name = 'Accounts' AND ProjectId = @ProjectId)
        INSERT INTO Reporter VALUES ('Accounts', 'Manages project billing and transactions', @ProjectId, 1, 0);

    IF NOT EXISTS (SELECT 1 FROM Reporter WHERE Name = 'Finance' AND ProjectId = @ProjectId)
        INSERT INTO Reporter VALUES ('Finance', 'Tracks budgets and expenses', @ProjectId, 1, 0);

    IF NOT EXISTS (SELECT 1 FROM Reporter WHERE Name = 'HR' AND ProjectId = @ProjectId)
        INSERT INTO Reporter VALUES ('HR', 'Handles staffing and project resources', @ProjectId, 1, 0);

    IF NOT EXISTS (SELECT 1 FROM Reporter WHERE Name = 'IT' AND ProjectId = @ProjectId)
        INSERT INTO Reporter VALUES ('IT', 'Handles internal systems and infrastructure', @ProjectId, 1, 0);

    IF NOT EXISTS (SELECT 1 FROM Reporter WHERE Name = 'QA' AND ProjectId = @ProjectId)
        INSERT INTO Reporter VALUES ('QA', 'Performs regression and integration testing', @ProjectId, 1, 0);
END;

--------------------------------------------
-- Project Nimbus
--------------------------------------------
SELECT @ProjectId = Id FROM Project WHERE Name = 'Project Nimbus';
IF @ProjectId IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM Reporter WHERE Name = 'R&D' AND ProjectId = @ProjectId)
        INSERT INTO Reporter VALUES ('R&D', 'Responsible for new feature prototyping', @ProjectId, 1, 0);

    IF NOT EXISTS (SELECT 1 FROM Reporter WHERE Name = 'Product' AND ProjectId = @ProjectId)
        INSERT INTO Reporter VALUES ('Product', 'Manages product requirements and backlog', @ProjectId, 1, 0);

    IF NOT EXISTS (SELECT 1 FROM Reporter WHERE Name = 'Design' AND ProjectId = @ProjectId)
        INSERT INTO Reporter VALUES ('Design', 'Creates and reviews UI/UX designs', @ProjectId, 1, 0);

    IF NOT EXISTS (SELECT 1 FROM Reporter WHERE Name = 'IT' AND ProjectId = @ProjectId)
        INSERT INTO Reporter VALUES ('IT', 'Handles system configurations and builds', @ProjectId, 1, 0);

    IF NOT EXISTS (SELECT 1 FROM Reporter WHERE Name = 'QA' AND ProjectId = @ProjectId)
        INSERT INTO Reporter VALUES ('QA', 'Performs test automation and release validation', @ProjectId, 1, 0);
END;

--------------------------------------------
-- Project Vertex
--------------------------------------------
SELECT @ProjectId = Id FROM Project WHERE Name = 'Project Vertex';
IF @ProjectId IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM Reporter WHERE Name = 'Analytics' AND ProjectId = @ProjectId)
        INSERT INTO Reporter VALUES ('Analytics', 'Collects and interprets usage metrics', @ProjectId, 1, 0);

    IF NOT EXISTS (SELECT 1 FROM Reporter WHERE Name = 'Security' AND ProjectId = @ProjectId)
        INSERT INTO Reporter VALUES ('Security', 'Ensures app and data security compliance', @ProjectId, 1, 0);

    IF NOT EXISTS (SELECT 1 FROM Reporter WHERE Name = 'DevOps' AND ProjectId = @ProjectId)
        INSERT INTO Reporter VALUES ('DevOps', 'Oversees release pipelines and infrastructure', @ProjectId, 1, 0);

    IF NOT EXISTS (SELECT 1 FROM Reporter WHERE Name = 'Support' AND ProjectId = @ProjectId)
        INSERT INTO Reporter VALUES ('Support', 'Resolves reported user issues', @ProjectId, 1, 0);

    IF NOT EXISTS (SELECT 1 FROM Reporter WHERE Name = 'Finance' AND ProjectId = @ProjectId)
        INSERT INTO Reporter VALUES ('Finance', 'Monitors financial operations', @ProjectId, 1, 0);
END;

--------------------------------------------
-- Project Helix
--------------------------------------------
SELECT @ProjectId = Id FROM Project WHERE Name = 'Project Helix';
IF @ProjectId IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM Reporter WHERE Name = 'Operations' AND ProjectId = @ProjectId)
        INSERT INTO Reporter VALUES ('Operations', 'Manages internal task assignments', @ProjectId, 1, 0);

    IF NOT EXISTS (SELECT 1 FROM Reporter WHERE Name = 'Support' AND ProjectId = @ProjectId)
        INSERT INTO Reporter VALUES ('Support', 'Handles technical and client support', @ProjectId, 1, 0);

    IF NOT EXISTS (SELECT 1 FROM Reporter WHERE Name = 'Marketing' AND ProjectId = @ProjectId)
        INSERT INTO Reporter VALUES ('Marketing', 'Manages campaigns and outreach', @ProjectId, 1, 0);

    IF NOT EXISTS (SELECT 1 FROM Reporter WHERE Name = 'QA' AND ProjectId = @ProjectId)
        INSERT INTO Reporter VALUES ('QA', 'Ensures build and release quality', @ProjectId, 1, 0);

    IF NOT EXISTS (SELECT 1 FROM Reporter WHERE Name = 'Admin' AND ProjectId = @ProjectId)
        INSERT INTO Reporter VALUES ('Admin', 'Controls permissions and workflow settings', @ProjectId, 1, 0);
END;
GO
