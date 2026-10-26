/* ==========================================================================================================
    Taskist - Default data

    Everything a new installation needs: language, roles, permissions, menu, work item lists, settings,
    email templates and the first administrator account.

    Run after 01_schema.sql (or dotnet ef database update) and before 03_locale.sql.
    Idempotent: rows are matched on their business key, so re-running inserts only what is missing and
    never changes data that already exists.
========================================================================================================== */

SET NOCOUNT ON;
SET XACT_ABORT ON;
BEGIN TRANSACTION;

/* ---------------------------------------------------------------------------------------------------------
    Language
--------------------------------------------------------------------------------------------------------- */
INSERT INTO [Language] ([Name], [LanguageCulture], [DisplayName], [Rtl], [DisplayOrder], [Active])
SELECT s.[Name], s.[LanguageCulture], s.[DisplayName], s.[Rtl], s.[DisplayOrder], s.[Active]
FROM (VALUES
    (N'English', N'en-IN', N'English', 0, 1, 1)
) AS s ([Name], [LanguageCulture], [DisplayName], [Rtl], [DisplayOrder], [Active])
WHERE NOT EXISTS (SELECT 1 FROM [Language] AS t WHERE t.[LanguageCulture] = s.[LanguageCulture]);

/* ---------------------------------------------------------------------------------------------------------
    Roles
--------------------------------------------------------------------------------------------------------- */
INSERT INTO [UserRole] ([Name], [SystemName], [Description], [SystemDefined], [Active])
SELECT s.[Name], s.[SystemName], s.[Description], s.[SystemDefined], s.[Active]
FROM (VALUES
    (N'Registered', N'Registered', N'Default role for all employees.', 1, 1),
    (N'System Administrator', N'SystemAdministrator', N'System administrator.', 1, 1),
    (N'Project Manager', N'ProjectManager', N'Project Manager', 1, 1),
    (N'Team Lead', N'TeamLead', N'Team Leads', 1, 1),
    (N'Developer', N'Developer', N'Developers', 1, 1),
    (N'Quality Assurance', N'QualityAssurance', N'Quality Assurance', 1, 1)
) AS s ([Name], [SystemName], [Description], [SystemDefined], [Active])
WHERE NOT EXISTS (SELECT 1 FROM [UserRole] AS t WHERE t.[SystemName] = s.[SystemName]);

/* ---------------------------------------------------------------------------------------------------------
    Permissions
    Each menu group header has its own permission (Work Items, Portfolio, Work Item Setup, Integrations, ...).
--------------------------------------------------------------------------------------------------------- */
INSERT INTO [UserRolePermission] ([Name], [SystemName], [RoleGroup], [SystemDefined])
SELECT s.[Name], s.[SystemName], s.[RoleGroup], s.[SystemDefined]
FROM (VALUES
    (N'Configuration', N'ManageConfiguration', N'Configuration', 1),
    (N'Manage Background Jobs', N'ManageJobs', N'Configuration', 1),
    (N'View Audit Log', N'ViewAuditLog', N'Configuration', 1),
    (N'Manage General Settings', N'ManageGeneralSettings', N'Configuration', 1),
    (N'Manage Integration Settings', N'ManageIntegrationSettings', N'Configuration', 1),
    (N'Manage Report Settings', N'ManageReportSettings', N'Configuration', 1),
    (N'Manage Security Settings', N'ManageSecuritySettings', N'Configuration', 1),
    (N'Manage Tags', N'ManageTags', N'Configuration', 1),
    (N'Manage Announcement', N'ManageAnnouncement', N'Configuration', 1),
    (N'Manage Work Item Settings', N'ManageWorkItemSettings', N'Configuration', 1),
    (N'Dashboard', N'ManageDashboard', N'Dashboard', 1),
    (N'Docs', N'ManageDocs', N'Docs', 1),
    (N'Wiki', N'ManageWiki', N'Docs', 1),
    (N'Client', N'ManageClient', N'Master', 1),
    (N'Email Account', N'ManageEmailAccount', N'Master', 1),
    (N'Email Template', N'ManageEmailTemplate', N'Master', 1),
    (N'Users', N'ManageUser', N'Master', 1),
    (N'User Role', N'ManageUserRole', N'Master', 1),
    (N'Language', N'ManageLanguage', N'Master', 1),
    (N'Locale Resource', N'ManageLocaleResource', N'Master', 1),
    (N'Module', N'ManageModule', N'Master', 1),
    (N'Project', N'ManageProject', N'Master', 1),
    (N'Reporter', N'ManageReporter', N'Master', 1),
    (N'Severity', N'ManageSeverity', N'Master', 1),
    (N'Status', N'ManageStatus', N'Master', 1),
    (N'Sub-Module', N'ManageSubModule', N'Master', 1),
    (N'Task Type', N'ManageTaskType', N'Master', 1),
    (N'Pivot Report', N'ManagePivotReport', N'Reports', 1),
    (N'Manage Analytics Report', N'ManageAnalyticsReport', N'Reports', 1),
    (N'Manage Resource Report', N'ManageResourceReport', N'Reports', 1),
    (N'Manage Stage Duration', N'ManageStageDuration', N'Reports', 1),
    (N'Backlog', N'ManageBacklog', N'WorkItems', 1),
    (N'Boards', N'ManageBoard', N'WorkItems', 1),
    (N'Release', N'ManageRelease', N'WorkItems', 1),
    (N'Work Items', N'ManageWorkItems', N'WorkItems', 1),
    (N'View Executive Dashboard', N'ViewExecutiveDashboard', N'Dashboard', 1),
    (N'Review Merge Requests', N'ReviewMergeRequests', N'WorkItems', 1),
    (N'Merge Merge Requests', N'MergeMergeRequests', N'WorkItems', 1),
    (N'Manage Git Connections', N'ManageGitConnections', N'Configuration', 1),
    (N'Manage Notification Channels', N'ManageNotificationChannels', N'Configuration', 1),
    (N'Portfolio', N'ManagePortfolio', N'Configuration', 1),
    (N'Work Item Setup', N'ManageWorkItemSetup', N'Configuration', 1),
    (N'Integrations', N'ManageIntegrations', N'Configuration', 1)
) AS s ([Name], [SystemName], [RoleGroup], [SystemDefined])
WHERE NOT EXISTS (SELECT 1 FROM [UserRolePermission] AS t WHERE t.[SystemName] = s.[SystemName]);

/* ---------------------------------------------------------------------------------------------------------
    Role to permission map
--------------------------------------------------------------------------------------------------------- */
INSERT INTO [UserRolePermissionMap] ([UserRoleId], [PermissionId])
SELECT r.[Id], p.[Id]
FROM (VALUES
    (N'Registered', N'ManageDashboard'),
    (N'SystemAdministrator', N'ManageConfiguration'),
    (N'SystemAdministrator', N'ManageJobs'),
    (N'SystemAdministrator', N'ViewAuditLog'),
    (N'SystemAdministrator', N'ManageGeneralSettings'),
    (N'SystemAdministrator', N'ManageIntegrationSettings'),
    (N'SystemAdministrator', N'ManageReportSettings'),
    (N'SystemAdministrator', N'ManageSecuritySettings'),
    (N'SystemAdministrator', N'ManageTags'),
    (N'SystemAdministrator', N'ManageAnnouncement'),
    (N'SystemAdministrator', N'ManageWorkItemSettings'),
    (N'SystemAdministrator', N'ManageDashboard'),
    (N'SystemAdministrator', N'ManageDocs'),
    (N'SystemAdministrator', N'ManageWiki'),
    (N'SystemAdministrator', N'ManageClient'),
    (N'SystemAdministrator', N'ManageEmailAccount'),
    (N'SystemAdministrator', N'ManageEmailTemplate'),
    (N'SystemAdministrator', N'ManageUser'),
    (N'SystemAdministrator', N'ManageUserRole'),
    (N'SystemAdministrator', N'ManageLanguage'),
    (N'SystemAdministrator', N'ManageLocaleResource'),
    (N'SystemAdministrator', N'ManageModule'),
    (N'SystemAdministrator', N'ManageProject'),
    (N'SystemAdministrator', N'ManageReporter'),
    (N'SystemAdministrator', N'ManageSeverity'),
    (N'SystemAdministrator', N'ManageStatus'),
    (N'SystemAdministrator', N'ManageSubModule'),
    (N'SystemAdministrator', N'ManageTaskType'),
    (N'SystemAdministrator', N'ManagePivotReport'),
    (N'SystemAdministrator', N'ManageAnalyticsReport'),
    (N'SystemAdministrator', N'ManageResourceReport'),
    (N'SystemAdministrator', N'ManageStageDuration'),
    (N'SystemAdministrator', N'ManageBacklog'),
    (N'SystemAdministrator', N'ManageBoard'),
    (N'SystemAdministrator', N'ManageRelease'),
    (N'SystemAdministrator', N'ManageWorkItems'),
    (N'SystemAdministrator', N'ViewExecutiveDashboard'),
    (N'SystemAdministrator', N'ReviewMergeRequests'),
    (N'SystemAdministrator', N'MergeMergeRequests'),
    (N'SystemAdministrator', N'ManageGitConnections'),
    (N'SystemAdministrator', N'ManageNotificationChannels'),
    (N'SystemAdministrator', N'ManagePortfolio'),
    (N'SystemAdministrator', N'ManageWorkItemSetup'),
    (N'SystemAdministrator', N'ManageIntegrations'),
    (N'ProjectManager', N'ManagePivotReport'),
    (N'ProjectManager', N'ManageAnalyticsReport'),
    (N'ProjectManager', N'ManageResourceReport'),
    (N'ProjectManager', N'ManageStageDuration'),
    (N'TeamLead', N'ManageDashboard'),
    (N'TeamLead', N'ManageClient'),
    (N'TeamLead', N'ManageModule'),
    (N'TeamLead', N'ManageProject'),
    (N'TeamLead', N'ManageReporter'),
    (N'TeamLead', N'ManageSubModule'),
    (N'TeamLead', N'ManagePivotReport'),
    (N'TeamLead', N'ManageAnalyticsReport'),
    (N'TeamLead', N'ManageResourceReport'),
    (N'TeamLead', N'ManageStageDuration'),
    (N'TeamLead', N'ManageBacklog'),
    (N'TeamLead', N'ManageRelease'),
    (N'TeamLead', N'ManageWorkItems'),
    (N'TeamLead', N'ReviewMergeRequests'),
    (N'TeamLead', N'MergeMergeRequests'),
    (N'TeamLead', N'ManagePortfolio'),
    (N'TeamLead', N'ManageWorkItemSetup'),
    (N'Developer', N'ManageBacklog'),
    (N'Developer', N'ManageWorkItems'),
    (N'QualityAssurance', N'ManageBacklog'),
    (N'QualityAssurance', N'ManageBoard'),
    (N'QualityAssurance', N'ManageWorkItems')
) AS s ([Role], [Permission])
JOIN [UserRole] AS r ON r.[SystemName] = s.[Role]
JOIN [UserRolePermission] AS p ON p.[SystemName] = s.[Permission]
WHERE NOT EXISTS (SELECT 1 FROM [UserRolePermissionMap] AS t WHERE t.[UserRoleId] = r.[Id] AND t.[PermissionId] = p.[Id]);

/* ---------------------------------------------------------------------------------------------------------
    Menu
    Matched on Code. ParentCode 0 = top-level item or group; a group header shows when any child is visible.
--------------------------------------------------------------------------------------------------------- */
INSERT INTO [Menu] ([Name], [SystemName], [Code], [ActionName], [ControllerName], [ParentCode], [Icon], [DisplayOrder], [Permission], [Active])
SELECT s.[Name], s.[SystemName], s.[Code], s.[ActionName], s.[ControllerName], s.[ParentCode], s.[Icon], s.[DisplayOrder], s.[Permission], s.[Active]
FROM (VALUES
    (N'Dashboard', N'Dashboard', 100, N'Index', N'Home', 0, N'fas fa-tachometer-alt', 1, N'ManageDashboard', 1),
    (N'Work Items', N'WorkItems', 300, NULL, NULL, 0, N'fas fa-check-double', 4, N'ManageWorkItems', 1),
    (N'Backlog', N'Backlog', 301, N'Index', N'Backlog', 300, NULL, 1, N'ManageBacklog', 1),
    (N'Board', N'Board', 302, N'Index', N'Board', 300, NULL, 2, N'ManageBoard', 1),
    (N'Reviews', N'Reviews', 306, N'Index', N'Review', 300, NULL, 3, N'ReviewMergeRequests', 1),
    (N'Timeline', N'BoardTimeline', 304, N'Timeline', N'Board', 300, NULL, 4, N'ManageBoard', 1),
    (N'Release', N'Release', 303, N'Index', N'ReleasePlan', 300, NULL, 5, N'ManageRelease', 1),
    (N'Wiki', N'Wiki', 400, N'Index', N'WikiDocument', 0, N'fas fa-edit', 5, N'ManageWiki', 1),
    (N'Reports', N'TeamReports', 520, NULL, NULL, 0, N'fas fa-chart-pie', 6, N'ManageAnalyticsReport', 1),
    (N'Task Explorer', N'TaskExplorer', 501, N'TaskExplorer', N'Report', 520, NULL, 3, N'ManagePivotReport', 1),
    (N'Resource Load', N'ResourceLoad', 510, N'ResourceLoad', N'Report', 520, NULL, 3, N'ManageResourceReport', 1),
    (N'Stage Duration', N'StageDuration', 504, N'StageDuration', N'Report', 520, NULL, 5, N'ManageStageDuration', 1),
    (N'Users & Access', N'UsersAccess', 600, NULL, NULL, 0, N'fas fa-users', 9, N'ManageUser', 1),
    (N'Users', N'Users', 601, N'Index', N'User', 600, NULL, 1, N'ManageUser', 1),
    (N'User Role', N'UserRole', 602, N'Index', N'UserRole', 600, NULL, 2, N'ManageUserRole', 1),
    (N'Roles & Permissions', N'UserAccess', 609, N'Access', N'UserRole', 600, NULL, 9, N'ManageUserRole', 1),
    (N'Configuration', N'Configuration', 700, NULL, NULL, 0, N'fas fa-cogs', 13, N'ManageConfiguration', 1),
    (N'Settings', N'Settings', 715, N'Index', N'Configuration', 700, NULL, 1, N'ManageConfiguration', 1),
    (N'Language', N'Language', 714, N'Index', N'Language', 700, NULL, 2, N'ManageLanguage', 1),
    (N'Announcements', N'Announcement', 718, N'Index', N'Announcement', 700, NULL, 3, N'ManageAnnouncement', 1),
    (N'Background Jobs', N'BackgroundJobs', 716, N'Index', N'Jobs', 700, NULL, 4, N'ManageJobs', 1),
    (N'Activity Log', N'ActivityLog', 717, N'Index', N'ActivityLog', 700, NULL, 5, N'ViewAuditLog', 1),
    (N'Portfolio', N'Portfolio', 720, NULL, NULL, 0, N'fas fa-briefcase', 10, N'ManagePortfolio', 1),
    (N'Client', N'Client', 701, N'Index', N'Client', 720, NULL, 1, N'ManageClient', 1),
    (N'Project', N'Project', 702, N'Index', N'Project', 720, NULL, 2, N'ManageProject', 1),
    (N'Module', N'Module', 703, N'Index', N'Module', 720, NULL, 3, N'ManageModule', 1),
    (N'Sub Module', N'SubModule', 704, N'Index', N'SubModule', 720, NULL, 4, N'ManageSubModule', 1),
    (N'Work Item Setup', N'WorkItemSetup', 730, NULL, NULL, 0, N'fas fa-sliders-h', 11, N'ManageWorkItemSetup', 1),
    (N'Task Type', N'TaskType', 707, N'Index', N'TaskType', 730, NULL, 1, N'ManageTaskType', 1),
    (N'Status', N'Status', 706, N'Index', N'Status', 730, NULL, 2, N'ManageStatus', 1),
    (N'Severity', N'Severity', 705, N'Index', N'Severity', 730, NULL, 3, N'ManageSeverity', 1),
    (N'Tag', N'Tag', 710, N'Index', N'Tag', 730, NULL, 4, N'ManageTags', 1),
    (N'Reporter', N'Reporter', 708, N'Index', N'Reporter', 730, NULL, 5, N'ManageReporter', 1),
    (N'Integrations', N'Integrations', 740, NULL, NULL, 0, N'fas fa-plug', 12, N'ManageIntegrations', 1),
    (N'Git Connections', N'GitConnection', 711, N'Index', N'GitConnection', 740, NULL, 1, N'ManageGitConnections', 1),
    (N'Notification Channels', N'NotificationChannel', 719, N'Index', N'NotificationChannel', 740, NULL, 2, N'ManageNotificationChannels', 1),
    (N'Email Account', N'EmailAccount', 712, N'Index', N'EmailAccount', 740, NULL, 3, N'ManageEmailAccount', 1),
    (N'Email Template', N'EmailTemplate', 713, N'Index', N'EmailTemplate', 740, NULL, 4, N'ManageEmailTemplate', 1)
) AS s ([Name], [SystemName], [Code], [ActionName], [ControllerName], [ParentCode], [Icon], [DisplayOrder], [Permission], [Active])
WHERE NOT EXISTS (SELECT 1 FROM [Menu] AS t WHERE t.[Code] = s.[Code]);

/* ---------------------------------------------------------------------------------------------------------
    Work item lists (shared by all projects)
    GroupId drives behaviour - TaskType: 1 To Do, 2 Change Request, 3 Bug.
    Status: 1 New, 2 In Progress, 3 Closed, 4 Reopened, 5 Resolved. Severity: 1-4 by impact.
--------------------------------------------------------------------------------------------------------- */
INSERT INTO [TaskType] ([Name], [GroupId], [TextColor], [BackgroundColor], [IconClass], [Active], [Deleted])
SELECT s.[Name], s.[GroupId], s.[TextColor], s.[BackgroundColor], s.[IconClass], 1, 0
FROM (VALUES
    (N'To Do', 1, N'#ffffff', N'#3399ff', N'fas fa-list-check'),
    (N'Change Request', 2, N'#ffffff', N'#f9b115', N'fas fa-code-pull-request'),
    (N'Bug', 3, N'#ffffff', N'#e55353', N'fas fa-bug')
) AS s ([Name], [GroupId], [TextColor], [BackgroundColor], [IconClass])
WHERE NOT EXISTS (SELECT 1 FROM [TaskType] AS t WHERE t.[Name] = s.[Name]);

INSERT INTO [Severity] ([Name], [GroupId], [TextColor], [BackgroundColor], [IconClass], [Active], [Deleted])
SELECT s.[Name], s.[GroupId], s.[TextColor], s.[BackgroundColor], s.[IconClass], 1, 0
FROM (VALUES
    (N'Minor', 1, N'#ffffff', N'#2eb85c', N'fas fa-circle'),
    (N'Major', 2, N'#ffffff', N'#f9b115', N'fas fa-circle'),
    (N'Critical', 3, N'#ffffff', N'#e55353', N'fas fa-circle'),
    (N'Show Stopper', 4, N'#ffffff', N'#321fdb', N'fas fa-circle')
) AS s ([Name], [GroupId], [TextColor], [BackgroundColor], [IconClass])
WHERE NOT EXISTS (SELECT 1 FROM [Severity] AS t WHERE t.[Name] = s.[Name]);

INSERT INTO [Status] ([Name], [GroupId], [ProjectId], [TextColor], [BackgroundColor], [IconClass],
    [RequiredRootCauseForBug], [RequiredRootCauseForCr], [RequiredResolutionForBug], [RequiredResolutionForCr],
    [RequiredMrNumberForBug], [RequiredMrNumberForCr], [SystemDefined], [ExternalUse], [Active], [Deleted])
SELECT s.[Name], s.[GroupId], NULL, s.[TextColor], s.[BackgroundColor], s.[IconClass], 0, 0, 0, 0, 0, 0, 1, 0, 1, 0
FROM (VALUES
    (N'New', 1, N'#ffffff', N'#3399ff', N'fas fa-star'),
    (N'In Progress', 2, N'#ffffff', N'#f9b115', N'fas fa-spinner'),
    (N'Reopened', 4, N'#ffffff', N'#e55353', N'fas fa-rotate-left'),
    (N'Resolved', 5, N'#ffffff', N'#2eb85c', N'fas fa-check'),
    (N'Closed', 3, N'#ffffff', N'#636f83', N'fas fa-lock')
) AS s ([Name], [GroupId], [TextColor], [BackgroundColor], [IconClass])
WHERE NOT EXISTS (SELECT 1 FROM [Status] AS t WHERE t.[Name] = s.[Name] AND t.[ProjectId] IS NULL);

/* ---------------------------------------------------------------------------------------------------------
    Settings
    Everything else is edited under Configuration -> Settings.
--------------------------------------------------------------------------------------------------------- */
INSERT INTO [Setting] ([Name], [Value])
SELECT s.[Name], s.[Value]
FROM (VALUES
    (N'commonsettings.allowedipaddress', N''),
    (N'commonsettings.saltkeysize', N'10'),
    (N'generalsettings.companyname', N'Taskist'),
    (N'generalsettings.itemsperpage', N'10'),
    (N'generalsettings.supportemail', N'support@taskist.org'),
    (N'workitemsettings.releaseversionformat', N'{major}.{minor}.{patch}'),
    (N'workitemsettings.releasefullincrement', N'2'),
    (N'reportexportsettings.layoutsjson', N'{"Backlogs":[{"Key":"Id","Title":"ID","Included":true},{"Key":"Title","Title":"Title","Included":true},{"Key":"Description","Title":"Description","Included":true},{"Key":"TaskType","Title":"Task Type","Included":true},{"Key":"Severity","Title":"Severity","Included":true},{"Key":"Status","Title":"Status","Included":true},{"Key":"Project","Title":"Project","Included":true},{"Key":"Module","Title":"Module","Included":true},{"Key":"SubModule","Title":"Sub-Module","Included":true},{"Key":"Release","Title":"Release","Included":true},{"Key":"Assignee","Title":"Assignee","Included":true},{"Key":"Tester","Title":"Tester","Included":true},{"Key":"DueDate","Title":"Due Date","Included":true},{"Key":"ReOpenCount","Title":"Re-open Count","Included":true},{"Key":"SubTaskCount","Title":"Sub-task Count","Included":true},{"Key":"CreatedBy","Title":"Created By","Included":true},{"Key":"CreatedOn","Title":"Created On","Included":true}],"ReleaseTasks":[{"Key":"Project","Title":"Product","Included":true},{"Key":"Id","Title":"ID","Included":true},{"Key":"Module","Title":"Module","Included":true},{"Key":"SubModule","Title":"Sub-Module","Included":true},{"Key":"Title","Title":"Title","Included":true},{"Key":"Description","Title":"Description","Included":true},{"Key":"Release","Title":"Release","Included":true},{"Key":"TaskType","Title":"Task Type","Included":true},{"Key":"Severity","Title":"Severity","Included":true},{"Key":"Status","Title":"Status","Included":true},{"Key":"Assignee","Title":"Developer","Included":true},{"Key":"Tester","Title":"Tester","Included":true},{"Key":"DueDate","Title":"Due Date","Included":true},{"Key":"CreatedBy","Title":"Created By","Included":true},{"Key":"CreatedOn","Title":"Created On","Included":true}]}')
) AS s ([Name], [Value])
WHERE NOT EXISTS (SELECT 1 FROM [Setting] AS t WHERE t.[Name] = s.[Name]);

/* ---------------------------------------------------------------------------------------------------------
    Email account and templates
    Templates need an email account, so an inactive placeholder account is created when none exists.
    Enter the SMTP details under Integrations -> Email Account, then activate it.
--------------------------------------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM [EmailAccount])
    INSERT INTO [EmailAccount] ([Name], [Description], [UserName], [Host], [Port], [Password], [EnableSsl], [FromEmail], [FromName], [Active])
    VALUES (N'Default', N'Placeholder - enter your SMTP details and activate.', N'', N'smtp.example.com', 587, N'', 1, N'noreply@taskist.org', N'Taskist', 0);

DECLARE @emailAccountId INT = (SELECT TOP (1) [Id] FROM [EmailAccount] ORDER BY [Id]);

DECLARE @emailTemplates TABLE (Name nvarchar(100), EmailSubject nvarchar(max), EmailBody nvarchar(max));
DECLARE @btn nvarchar(250) = 'display:inline-block;background:#1cc7c9;color:#ffffff;padding:10px 24px;border-radius:6px;text-decoration:none;font-weight:600;font-size:14px;';
INSERT INTO @emailTemplates VALUES
    ('ResetPassword', 'Reset your password',
     '<p>Hello %displayname%,</p><p>We received a request to reset your password. Click the button below to choose a new one.</p><p><a href="%url%" style="' + @btn + '">Reset my password</a></p><p style="color:#8a97a8;font-size:13px;">If you did not request this, you can safely ignore this email.</p>'),
    ('Activation', 'Activate your account',
     '<p>Hello %display_name%,</p><p>Your account is ready. Click the button below to set your password and sign in.</p><p><a href="%url%" style="' + @btn + '">Set my password</a></p>'),
    ('WelcomeKit', 'Welcome to %company%',
     '<p>Hello %display_name%,</p><p>Welcome aboard! Your taskist account is all set and ready to go.</p><p><a href="%button%" style="' + @btn + '">Open Taskist</a></p>'),
    ('RegistrationKit', 'Your %company% account is ready',
     '<p>Hello %display_name%,</p><p>Thanks for registering. Your account has been created and you can sign in any time.</p><p><a href="%button%" style="' + @btn + '">Go to Taskist</a></p>'),
    ('Invite', 'You have been invited to %company%',
     '<p>Hello %display_name%,</p><p>You have been invited to join us on Taskist.</p><p><a href="%button%" style="' + @btn + '">Accept invite</a></p>'),
    ('Notification', 'Taskist notification',
     '<p>Hello %display_name%,</p>%body%'),
    ('NewTaskNotification', 'New task assigned: {title}',
     '<p>Hello {display_name},</p><p><strong>{created_by}</strong> assigned you a new {task_type}:</p><p style="font-size:16px;"><strong>#{task_id} - {title}</strong></p><p style="color:#5b6b7b;">Project: {project_name} / {module_name} / {sub_module_name}<br/>Severity: {severity_name} &middot; Due: {due_date}</p><p><a href="{url}" style="' + @btn + '">View task</a></p>'),
    ('TaskNotification', 'Task updated: {title}',
     '<p>Hello {display_name},</p><p><strong>{updated_by}</strong> updated a task you are involved in:</p><p style="font-size:16px;"><strong>{title}</strong> &rarr; <strong>{new_status}</strong></p><p style="color:#5b6b7b;">Project: {project_name} / {module_name} / {sub_module_name}</p><p><a href="{url}" style="' + @btn + '">View task</a></p>');

INSERT INTO [EmailTemplate] ([Name], [EmailSubject], [EmailBody], [EmailAccountId], [Active])
SELECT t.[Name], t.[EmailSubject], t.[EmailBody], @emailAccountId, 1
FROM @emailTemplates AS t
WHERE NOT EXISTS (SELECT 1 FROM [EmailTemplate] AS e WHERE e.[Name] = t.[Name]);

/* ---------------------------------------------------------------------------------------------------------
    Administrator account
    Created only while the [User] table is empty, so re-running never restores it after it was renamed,
    replaced or deleted. Mapped to every role.

    Login: admin@taskist.org    Password: Admin@123    >>> CHANGE THE PASSWORD AFTER THE FIRST LOGIN <<<
--------------------------------------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM [User])
BEGIN
    DECLARE @addressId INT, @userId INT;

    INSERT INTO [Address] ([Address1], [Address2], [City], [ZipPostalCode])
    VALUES (N'Head Office', NULL, N'Head Office', N'000000');
    SET @addressId = SCOPE_IDENTITY();

    INSERT INTO [User] ([Code], [FirstName], [LastName], [MobileNumber], [Email], [GenderId], [MaritalStatus],
        [DateOfBirth], [DateOfJoin], [LanguageId], [AddressId],
        [GitUserName], [SystemAccount], [FailedLoginAttempts], [LastActivityDate], [Locked], [Status], [Deleted])
    VALUES (NEWID(), N'System', N'Administrator', N'0000000000', N'admin@taskist.org', 1, 0,
        '1990-01-01', '2024-01-01', (SELECT TOP (1) [Id] FROM [Language] ORDER BY [DisplayOrder], [Id]), @addressId,
        NULL, 0, 0, SYSUTCDATETIME(), 0, 1, 0);
    SET @userId = SCOPE_IDENTITY();

    INSERT INTO [UserPassword] ([UserId], [Password], [PasswordSalt], [CreatedOn])
    VALUES (@userId, N'pbkdf2-sha512$210000$O2S58hPN91Q3fej+T0aO6A==$/lWlgqVpir9JSvJtVINWe1WPEjdTpz5qaoqSXPGN624fWgW9C5ZNpj/WZebqVrxwKuNLrBuFaEJlhj8PbQfCwg==', N'', SYSUTCDATETIME());

    INSERT INTO [UserRoleMap] ([UserId], [UserRoleId])
    SELECT @userId, r.[Id] FROM [UserRole] AS r;
END

COMMIT TRANSACTION;
PRINT 'Default data seeded.';
