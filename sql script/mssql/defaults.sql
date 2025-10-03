--Menu
SET IDENTITY_INSERT [Menu] ON;
GO
IF NOT EXISTS (SELECT 1 FROM [Menu] WHERE [SystemName] = 'WorkItems')
BEGIN
    INSERT INTO [Menu] ([Name] ,[SystemName] ,[Code] ,[ActionName] ,[ControllerName] ,[ParentCode] ,[Icon] ,[DisplayOrder] ,[Permission] ,[Active]) 
	VALUES('Work Items','WorkItems',100,'NULL','NULL',0,'fas fa-check-double',1,'WorkItem.ManageWorkItems',1)
END
GO
IF NOT EXISTS (SELECT 1 FROM [Menu] WHERE [SystemName] = 'Backlog')
BEGIN
    INSERT INTO [Menu] ([Name] ,[SystemName] ,[Code] ,[ActionName] ,[ControllerName] ,[ParentCode] ,[Icon] ,[DisplayOrder] ,[Permission] ,[Active]) 
	VALUES('Backlog','Backlog',101,'Index','Backlog',100,'NULL',1,'WorkItem.ManageBacklog',1)
END
GO
IF NOT EXISTS (SELECT 1 FROM [Menu] WHERE [SystemName] = 'Board')
BEGIN
    INSERT INTO [Menu] ([Name] ,[SystemName] ,[Code] ,[ActionName] ,[ControllerName] ,[ParentCode] ,[Icon] ,[DisplayOrder] ,[Permission] ,[Active]) 
	VALUES('Board','Board',102,'Index','Board',100,'NULL',2,'WorkItem.ManageBoard',1)
END
GO
IF NOT EXISTS (SELECT 1 FROM [Menu] WHERE [SystemName] = 'Sprints')
BEGIN
    INSERT INTO [Menu] ([Name] ,[SystemName] ,[Code] ,[ActionName] ,[ControllerName] ,[ParentCode] ,[Icon] ,[DisplayOrder] ,[Permission] ,[Active]) 
	VALUES('Sprints','Sprints',103,'Index','Sprint',100,'NULL',3,'WorkItem.ManageSprints',1)
END
GO
IF NOT EXISTS (SELECT 1 FROM [Menu] WHERE [SystemName] = 'Configuration')
BEGIN
    INSERT INTO [Menu] ([Name] ,[SystemName] ,[Code] ,[ActionName] ,[ControllerName] ,[ParentCode] ,[Icon] ,[DisplayOrder] ,[Permission] ,[Active]) 
	VALUES('Configuration','Configuration',200,'NULL','NULL',0,'fas fa-layer-group',1,'Configuration.ManageConfiguration',1)
END
GO
IF NOT EXISTS (SELECT 1 FROM [Menu] WHERE [SystemName] = 'Users')
BEGIN
    INSERT INTO [Menu] ([Name] ,[SystemName] ,[Code] ,[ActionName] ,[ControllerName] ,[ParentCode] ,[Icon] ,[DisplayOrder] ,[Permission] ,[Active]) 
	VALUES('Users','Users',201,'Index','User',200,'NULL',2,'Configuration.ManageUser',1)
END
GO
IF NOT EXISTS (SELECT 1 FROM [Menu] WHERE [SystemName] = 'Client')
BEGIN
    INSERT INTO [Menu] ([Name] ,[SystemName] ,[Code] ,[ActionName] ,[ControllerName] ,[ParentCode] ,[Icon] ,[DisplayOrder] ,[Permission] ,[Active]) 
	VALUES('Client','Client',202,'Index','Client',200,'NULL',3,'Configuration.ManageClient',1)
END
GO
IF NOT EXISTS (SELECT 1 FROM [Menu] WHERE [SystemName] = 'Project')
BEGIN
    INSERT INTO [Menu] ([Name] ,[SystemName] ,[Code] ,[ActionName] ,[ControllerName] ,[ParentCode] ,[Icon] ,[DisplayOrder] ,[Permission] ,[Active]) 
	VALUES('Project','Project',203,'Index','Project',200,'NULL',4,'Configuration.ManageProject',1)
END
GO
IF NOT EXISTS (SELECT 1 FROM [Menu] WHERE [SystemName] = 'Module')
BEGIN
    INSERT INTO [Menu] ([Name] ,[SystemName] ,[Code] ,[ActionName] ,[ControllerName] ,[ParentCode] ,[Icon] ,[DisplayOrder] ,[Permission] ,[Active]) 
	VALUES('Module','Module',204,'Index','Module',200,'NULL',5,'Configuration.ManageModule',1)
END
GO
IF NOT EXISTS (SELECT 1 FROM [Menu] WHERE [SystemName] = 'SubModule')
BEGIN
    INSERT INTO [Menu] ([Name] ,[SystemName] ,[Code] ,[ActionName] ,[ControllerName] ,[ParentCode] ,[Icon] ,[DisplayOrder] ,[Permission] ,[Active]) 
	VALUES('Sub Module','SubModule',205,'Index','SubModule',200,'NULL',6,'Configuration.ManageSubModule',1)
END
GO
IF NOT EXISTS (SELECT 1 FROM [Menu] WHERE [SystemName] = 'Severity')
BEGIN
    INSERT INTO [Menu] ([Name] ,[SystemName] ,[Code] ,[ActionName] ,[ControllerName] ,[ParentCode] ,[Icon] ,[DisplayOrder] ,[Permission] ,[Active]) 
	VALUES('Severity','Severity',206,'Index','Severity',200,'NULL',7,'Configuration.ManageSeverity',1)
END
GO
IF NOT EXISTS (SELECT 1 FROM [Menu] WHERE [SystemName] = 'Status')
BEGIN
    INSERT INTO [Menu] ([Name] ,[SystemName] ,[Code] ,[ActionName] ,[ControllerName] ,[ParentCode] ,[Icon] ,[DisplayOrder] ,[Permission] ,[Active]) 
	VALUES('Status','Status',207,'Index','Status',200,'NULL',8,'Configuration.ManageStatus',1)
END
GO
IF NOT EXISTS (SELECT 1 FROM [Menu] WHERE [SystemName] = 'TaskType')
BEGIN
    INSERT INTO [Menu] ([Name] ,[SystemName] ,[Code] ,[ActionName] ,[ControllerName] ,[ParentCode] ,[Icon] ,[DisplayOrder] ,[Permission] ,[Active]) 
	VALUES('Task Type','TaskType',208,'Index','TaskType',200,'NULL',9,'Configuration.ManageTaskType',1)
END
GO
IF NOT EXISTS (SELECT 1 FROM [Menu] WHERE [SystemName] = 'Reporter')
BEGIN
	INSERT INTO [Menu] ([Name] ,[SystemName] ,[Code] ,[ActionName] ,[ControllerName] ,[ParentCode] ,[Icon] ,[DisplayOrder] ,[Permission] ,[Active]) 
	VALUES('Reporter','Reporter',209,'Index','Reporter',200,'NULL',10,'Configuration.ManageReporter',1)
END
GO
IF NOT EXISTS (SELECT 1 FROM [Menu] WHERE [SystemName] = 'UserRole')
BEGIN
	INSERT INTO [Menu] ([Name] ,[SystemName] ,[Code] ,[ActionName] ,[ControllerName] ,[ParentCode] ,[Icon] ,[DisplayOrder] ,[Permission] ,[Active]) 
	VALUES('User Role','UserRole',210,'Index','UserRole',200,'NULL',11,'Configuration.ManageUserRole',1)
END
GO
IF NOT EXISTS (SELECT 1 FROM [Menu] WHERE [SystemName] = 'EmailAccount')
BEGIN
	INSERT INTO [Menu] ([Name] ,[SystemName] ,[Code] ,[ActionName] ,[ControllerName] ,[ParentCode] ,[Icon] ,[DisplayOrder] ,[Permission] ,[Active]) 
	VALUES('Email Account','EmailAccount',211,'Index','EmailAccount',200,'NULL',15,'Configuration.ManageEmailAccount',1)
END
GO
IF NOT EXISTS (SELECT 1 FROM [Menu] WHERE [SystemName] = 'EmailTemplate')
BEGIN
	INSERT INTO [Menu] ([Name] ,[SystemName] ,[Code] ,[ActionName] ,[ControllerName] ,[ParentCode] ,[Icon] ,[DisplayOrder] ,[Permission] ,[Active]) 
	VALUES('Email Template','EmailTemplate',212,'Index','EmailTemplate',200,'NULL',16,'Configuration.ManageEmailTemplate',1)
END
GO
IF NOT EXISTS (SELECT 1 FROM [Menu] WHERE [SystemName] = 'Language')
BEGIN
	INSERT INTO [Menu] ([Name] ,[SystemName] ,[Code] ,[ActionName] ,[ControllerName] ,[ParentCode] ,[Icon] ,[DisplayOrder] ,[Permission] ,[Active]) 
	VALUES('Language','Language',213,'Index','Language',200,'NULL',17,'Configuration.ManageLanguage',1)
END
GO
SET IDENTITY_INSERT [Menu] OFF;
GO
--User Roles
SET IDENTITY_INSERT [UserRole] ON;
GO
IF NOT EXISTS (SELECT 1 FROM [UserRole] WHERE [SystemName] = 'Registered')
BEGIN
	INSERT [UserRole] ([Id], [Name], [SystemName], [Description], [SystemDefined], [Active]) 
	VALUES (1, N'Registered', N'Registered', N'Default role for all users.', 1, 1)
END
GO
IF NOT EXISTS (SELECT 1 FROM [UserRole] WHERE [SystemName] = 'SystemAdministrator')
BEGIN
	INSERT [UserRole] ([Id], [Name], [SystemName], [Description], [SystemDefined], [Active]) 
	VALUES (2, N'System Administrator', N'SystemAdministrator', N'System administrator.', 1, 1)
END
GO
IF NOT EXISTS (SELECT 1 FROM [UserRole] WHERE [SystemName] = 'Developer')
BEGIN
	INSERT [UserRole] ([Id], [Name], [SystemName], [Description], [SystemDefined], [Active]) 
	VALUES (3, N'Developer', N'Developer', N'Developers', 1, 1)
END
GO
IF NOT EXISTS (SELECT 1 FROM [UserRole] WHERE [SystemName] = 'Reporters')
BEGIN
	INSERT [UserRole] ([Id], [Name], [SystemName], [Description], [SystemDefined], [Active]) 
	VALUES (4, N'Reporter', N'Reporter', N'Reporters', 1, 1)
END
GO
SET IDENTITY_INSERT [UserRole] OFF;
GO
--Language
SET IDENTITY_INSERT [Language] ON 
GO
IF NOT EXISTS (SELECT 1 FROM [Language] WHERE [LanguageCulture] = 'en-IN')
BEGIN
	INSERT [Language] ([Id], [Name], [LanguageCulture], [DisplayName], [Rtl], [DisplayOrder], [Active]) 
	VALUES (1, N'English (India)', N'en-IN', N'English (India)', 0, 1, 1)
END
GO
SET IDENTITY_INSERT [Language] OFF
--User
SET IDENTITY_INSERT [User] ON 
GO
DECLARE @LanguageId INT
SELECT @LanguageId = Id FROM [Language] WHERE [LanguageCulture] = 'en-IN'
IF NOT EXISTS (SELECT 1 FROM [User] WHERE [Email] = 'admin@wisecoders.in' AND @LanguageId > 0)
BEGIN
	DECLARE @UserId INT;
	INSERT [User] ([Id], [Code], [FirstName], [LastName], [Email], [GenderId], [LanguageId], [SystemAccount], [FailedLoginAttempts], [LastIPAddress], [LastLoginDate], [LastActivityDate], [Locked], [Status], [Deleted]) 
	VALUES (1, N'f2e8296a-5b13-467d-9eb9-60a9f18ffeee', N'Admin', N'Wise Coder', N'admin@wisecoders.in', 1, @LanguageId, 1, 0, N'0.0.0.0', NULL, CAST(N'2025-10-02T17:49:12.0317624' AS DateTime2), 0, 1, 0)
	
	SET @UserId = SCOPE_IDENTITY();
	
	IF NOT EXISTS (SELECT 1 FROM [UserPassword] WHERE [UserId] = @UserId AND @UserId > 0)
	BEGIN
		SET IDENTITY_INSERT [UserPassword] ON 
		GO
		INSERT [UserPassword] ([Id], [UserId], [Password], [PasswordSalt], [CreatedOn]) 
		VALUES (1, @UserId, N'075CD543BABC2C36B967A257B471A8B24C97D65C', N'n1DRhPvrjpXMEA==', CAST(N'2025-10-02T16:40:13.5020809' AS DateTime2))
		GO
		SET IDENTITY_INSERT [UserPassword] OFF 
	END
END
GO
SET IDENTITY_INSERT [User] OFF