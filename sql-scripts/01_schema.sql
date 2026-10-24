IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020195900_InitialCreate'
)
BEGIN
    CREATE TABLE [ActivityLog] (
        [Id] int NOT NULL IDENTITY,
        [SystemName] nvarchar(100) NOT NULL,
        [EntityId] int NULL,
        [EntityName] nvarchar(500) NOT NULL,
        [UserId] int NOT NULL,
        [Comment] nvarchar(max) NOT NULL,
        [CreatedOnUtc] datetime2 NOT NULL,
        [IpAddress] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_ActivityLog] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020195900_InitialCreate'
)
BEGIN
    CREATE TABLE [Address] (
        [Id] int NOT NULL IDENTITY,
        [Address1] nvarchar(750) NOT NULL,
        [Address2] nvarchar(750) NULL,
        [City] nvarchar(100) NULL,
        [ZipPostalCode] nvarchar(20) NOT NULL,
        CONSTRAINT [PK_Address] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020195900_InitialCreate'
)
BEGIN
    CREATE TABLE [Announcement] (
        [Id] int NOT NULL IDENTITY,
        [Title] nvarchar(200) NOT NULL,
        [Body] nvarchar(max) NOT NULL,
        [Type] int NOT NULL,
        [Severity] int NOT NULL,
        [Audience] int NOT NULL,
        [ValidFrom] date NOT NULL,
        [ValidTo] date NOT NULL,
        [RequiresAcknowledgement] bit NOT NULL,
        [DisplayOrder] int NOT NULL,
        [Active] bit NOT NULL,
        [Deleted] bit NOT NULL,
        CONSTRAINT [PK_Announcement] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020195900_InitialCreate'
)
BEGIN
    CREATE TABLE [AuditLog] (
        [Id] int NOT NULL IDENTITY,
        [EntityName] nvarchar(500) NOT NULL,
        [EntityId] int NULL,
        [Action] nvarchar(20) NOT NULL,
        [UserId] int NULL,
        [OldValues] nvarchar(max) NULL,
        [NewValues] nvarchar(max) NULL,
        [CreatedOnUtc] datetime2 NOT NULL,
        [IpAddress] nvarchar(100) NULL,
        CONSTRAINT [PK_AuditLog] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020195900_InitialCreate'
)
BEGIN
    CREATE TABLE [Client] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(100) NOT NULL,
        [Description] nvarchar(250) NULL,
        [ContactPerson] nvarchar(100) NULL,
        [PhoneNumber] nvarchar(100) NULL,
        [Email] nvarchar(250) NULL,
        [Website] nvarchar(750) NULL,
        [Active] bit NOT NULL,
        [Deleted] bit NOT NULL,
        CONSTRAINT [PK_Client] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020195900_InitialCreate'
)
BEGIN
    CREATE TABLE [Document] (
        [Id] int NOT NULL IDENTITY,
        [FileName] nvarchar(255) NOT NULL,
        [FileData] varbinary(max) NOT NULL,
        [ContentType] nvarchar(50) NOT NULL,
        [FileSize] bigint NOT NULL,
        [Extension] nvarchar(10) NOT NULL,
        CONSTRAINT [PK_Document] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020195900_InitialCreate'
)
BEGIN
    CREATE TABLE [EmailAccount] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(100) NOT NULL,
        [Description] nvarchar(250) NULL,
        [UserName] nvarchar(max) NOT NULL,
        [Host] nvarchar(max) NOT NULL,
        [Port] int NOT NULL,
        [Password] nvarchar(max) NOT NULL,
        [EnableSsl] bit NOT NULL,
        [FromEmail] nvarchar(max) NOT NULL,
        [FromName] nvarchar(max) NOT NULL,
        [Active] bit NOT NULL,
        CONSTRAINT [PK_EmailAccount] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020195900_InitialCreate'
)
BEGIN
    CREATE TABLE [GenericAttribute] (
        [Id] int NOT NULL IDENTITY,
        [EntityId] int NOT NULL,
        [KeyGroup] nvarchar(200) NOT NULL,
        [Key] nvarchar(200) NOT NULL,
        [Value] nvarchar(max) NOT NULL,
        [CreatedOrUpdatedDate] datetime2 NULL,
        CONSTRAINT [PK_GenericAttribute] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020195900_InitialCreate'
)
BEGIN
    CREATE TABLE [GitConnection] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(100) NOT NULL,
        [ProviderType] int NOT NULL,
        [BaseUrl] nvarchar(500) NOT NULL,
        [ApiUrl] nvarchar(500) NULL,
        [AuthMode] int NOT NULL,
        [ServiceToken] nvarchar(max) NULL,
        [AppId] nvarchar(50) NULL,
        [AppPrivateKey] nvarchar(max) NULL,
        [OAuthClientId] nvarchar(200) NULL,
        [OAuthClientSecret] nvarchar(max) NULL,
        [ServiceAccountUserName] nvarchar(255) NULL,
        [ServiceAccountExternalId] nvarchar(64) NULL,
        [AllowInsecureHttp] bit NOT NULL,
        [IsDefault] bit NOT NULL,
        [Active] bit NOT NULL,
        [Deleted] bit NOT NULL,
        [CreatedOn] datetime2 NOT NULL,
        [ModifiedOn] datetime2 NULL,
        CONSTRAINT [PK_GitConnection] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020195900_InitialCreate'
)
BEGIN
    CREATE TABLE [Language] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(750) NOT NULL,
        [LanguageCulture] nvarchar(10) NOT NULL,
        [DisplayName] nvarchar(750) NOT NULL,
        [Rtl] bit NOT NULL,
        [DisplayOrder] int NOT NULL,
        [Active] bit NOT NULL,
        CONSTRAINT [PK_Language] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020195900_InitialCreate'
)
BEGIN
    CREATE TABLE [Log] (
        [Id] int NOT NULL IDENTITY,
        [LogLevelId] int NOT NULL,
        [ShortMessage] nvarchar(max) NOT NULL,
        [FullMessage] nvarchar(max) NOT NULL,
        [IpAddress] nvarchar(max) NULL,
        [UserId] int NULL,
        [PageUrl] nvarchar(max) NULL,
        [ReferrerUrl] nvarchar(max) NULL,
        [CreatedOnUtc] datetime2 NOT NULL,
        [LogLevel] int NOT NULL,
        CONSTRAINT [PK_Log] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020195900_InitialCreate'
)
BEGIN
    CREATE TABLE [Menu] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(50) NOT NULL,
        [SystemName] nvarchar(100) NOT NULL,
        [Code] int NOT NULL,
        [ActionName] nvarchar(100) NULL,
        [ControllerName] nvarchar(100) NULL,
        [ParentCode] int NULL,
        [Icon] nvarchar(100) NULL,
        [DisplayOrder] int NOT NULL,
        [Permission] nvarchar(100) NOT NULL,
        [Active] bit NOT NULL,
        CONSTRAINT [PK_Menu] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020195900_InitialCreate'
)
BEGIN
    CREATE TABLE [NotificationChannel] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(100) NOT NULL,
        [ChannelType] int NOT NULL,
        [EndpointUrl] nvarchar(max) NOT NULL,
        [SigningSecret] nvarchar(max) NULL,
        [Active] bit NOT NULL,
        [Deleted] bit NOT NULL,
        [CreatedOn] datetime2 NOT NULL,
        [ModifiedOn] datetime2 NULL,
        [LastSentOn] datetime2 NULL,
        [LastError] nvarchar(1000) NULL,
        CONSTRAINT [PK_NotificationChannel] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020195900_InitialCreate'
)
BEGIN
    CREATE TABLE [Setting] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(250) NOT NULL,
        [Value] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_Setting] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020195900_InitialCreate'
)
BEGIN
    CREATE TABLE [Severity] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(100) NOT NULL,
        [Description] nvarchar(250) NULL,
        [GroupId] int NOT NULL,
        [TextColor] nvarchar(20) NULL,
        [BackgroundColor] nvarchar(20) NULL,
        [IconClass] nvarchar(50) NULL,
        [Active] bit NOT NULL,
        [Deleted] bit NOT NULL,
        CONSTRAINT [PK_Severity] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020195900_InitialCreate'
)
BEGIN
    CREATE TABLE [Tag] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(100) NOT NULL,
        [Active] bit NOT NULL,
        [Deleted] bit NOT NULL,
        CONSTRAINT [PK_Tag] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020195900_InitialCreate'
)
BEGIN
    CREATE TABLE [TaskType] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(100) NOT NULL,
        [Description] nvarchar(250) NULL,
        [GroupId] int NOT NULL,
        [TextColor] nvarchar(20) NULL,
        [BackgroundColor] nvarchar(20) NULL,
        [IconClass] nvarchar(50) NULL,
        [Active] bit NOT NULL,
        [Deleted] bit NOT NULL,
        CONSTRAINT [PK_TaskType] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020195900_InitialCreate'
)
BEGIN
    CREATE TABLE [UserRole] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(50) NOT NULL,
        [SystemName] nvarchar(50) NOT NULL,
        [Description] nvarchar(250) NULL,
        [SystemDefined] bit NOT NULL,
        [Active] bit NOT NULL,
        CONSTRAINT [PK_UserRole] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020195900_InitialCreate'
)
BEGIN
    CREATE TABLE [UserRolePermission] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(100) NOT NULL,
        [SystemName] nvarchar(255) NOT NULL,
        [RoleGroup] nvarchar(255) NOT NULL,
        [SystemDefined] bit NOT NULL,
        CONSTRAINT [PK_UserRolePermission] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020195900_InitialCreate'
)
BEGIN
    CREATE TABLE [EmailTemplate] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(100) NOT NULL,
        [EmailSubject] nvarchar(max) NOT NULL,
        [EmailBody] nvarchar(max) NOT NULL,
        [EmailAccountId] int NOT NULL,
        [Active] bit NOT NULL,
        CONSTRAINT [PK_EmailTemplate] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_EmailTemplate_EmailAccount_EmailAccountId] FOREIGN KEY ([EmailAccountId]) REFERENCES [EmailAccount] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020195900_InitialCreate'
)
BEGIN
    CREATE TABLE [LocaleResource] (
        [Id] int NOT NULL IDENTITY,
        [LanguageId] int NOT NULL,
        [ResourceKey] nvarchar(100) NOT NULL,
        [ResourceValue] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_LocaleResource] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_LocaleResource_Language_LanguageId] FOREIGN KEY ([LanguageId]) REFERENCES [Language] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020195900_InitialCreate'
)
BEGIN
    CREATE TABLE [User] (
        [Id] int NOT NULL IDENTITY,
        [Code] uniqueidentifier NOT NULL,
        [FirstName] nvarchar(50) NOT NULL,
        [LastName] nvarchar(50) NOT NULL,
        [FriendlyName] nvarchar(100) NULL,
        [MobileNumber] nvarchar(10) NOT NULL,
        [Email] nvarchar(150) NOT NULL,
        [GenderId] int NOT NULL,
        [MaritalStatus] int NOT NULL,
        [DateOfBirth] date NOT NULL,
        [DateOfJoin] date NOT NULL,
        [LanguageId] int NOT NULL,
        [AddressId] int NOT NULL,
        [GitUserName] nvarchar(100) NULL,
        [PhotoDocumentId] int NULL,
        [SystemAccount] bit NOT NULL,
        [FailedLoginAttempts] int NOT NULL,
        [LastIPAddress] nvarchar(100) NULL,
        [LastLoginDate] datetime2 NULL,
        [LastActivityDate] datetime2 NOT NULL,
        [Locked] bit NOT NULL,
        [Status] int NOT NULL,
        [Deleted] bit NOT NULL,
        CONSTRAINT [PK_User] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_User_Address_AddressId] FOREIGN KEY ([AddressId]) REFERENCES [Address] ([Id]),
        CONSTRAINT [FK_User_Language_LanguageId] FOREIGN KEY ([LanguageId]) REFERENCES [Language] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020195900_InitialCreate'
)
BEGIN
    CREATE TABLE [AnnouncementRole] (
        [Id] int NOT NULL IDENTITY,
        [AnnouncementId] int NOT NULL,
        [UserRoleId] int NOT NULL,
        CONSTRAINT [PK_AnnouncementRole] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AnnouncementRole_Announcement_AnnouncementId] FOREIGN KEY ([AnnouncementId]) REFERENCES [Announcement] ([Id]),
        CONSTRAINT [FK_AnnouncementRole_UserRole_UserRoleId] FOREIGN KEY ([UserRoleId]) REFERENCES [UserRole] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020195900_InitialCreate'
)
BEGIN
    CREATE TABLE [UserRolePermissionMap] (
        [PermissionId] int NOT NULL,
        [UserRoleId] int NOT NULL,
        CONSTRAINT [PK_UserRolePermissionMap] PRIMARY KEY ([PermissionId], [UserRoleId]),
        CONSTRAINT [FK_UserRolePermissionMap_UserRolePermission_PermissionId] FOREIGN KEY ([PermissionId]) REFERENCES [UserRolePermission] ([Id]),
        CONSTRAINT [FK_UserRolePermissionMap_UserRole_UserRoleId] FOREIGN KEY ([UserRoleId]) REFERENCES [UserRole] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020195900_InitialCreate'
)
BEGIN
    CREATE TABLE [AnnouncementAcknowledgement] (
        [Id] int NOT NULL IDENTITY,
        [AnnouncementId] int NOT NULL,
        [UserId] int NOT NULL,
        [AcknowledgedOnUtc] datetime2 NOT NULL,
        CONSTRAINT [PK_AnnouncementAcknowledgement] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AnnouncementAcknowledgement_Announcement_AnnouncementId] FOREIGN KEY ([AnnouncementId]) REFERENCES [Announcement] ([Id]),
        CONSTRAINT [FK_AnnouncementAcknowledgement_User_UserId] FOREIGN KEY ([UserId]) REFERENCES [User] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020195900_InitialCreate'
)
BEGIN
    CREATE TABLE [Project] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(100) NOT NULL,
        [Description] nvarchar(250) NULL,
        [StartDate] date NULL,
        [EndDate] date NULL,
        [ClientId] int NOT NULL,
        [ProjectManagerId] int NULL,
        [TeamLeadId] int NULL,
        [CodeReviewerId] int NULL,
        [GitProjectReference] nvarchar(50) NULL,
        [ReleaseVersionFormat] nvarchar(50) NULL,
        [ReleaseFullIncrement] int NULL,
        [Active] bit NOT NULL,
        [Deleted] bit NOT NULL,
        CONSTRAINT [PK_Project] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Project_Client_ClientId] FOREIGN KEY ([ClientId]) REFERENCES [Client] ([Id]),
        CONSTRAINT [FK_Project_User_CodeReviewerId] FOREIGN KEY ([CodeReviewerId]) REFERENCES [User] ([Id]),
        CONSTRAINT [FK_Project_User_ProjectManagerId] FOREIGN KEY ([ProjectManagerId]) REFERENCES [User] ([Id]),
        CONSTRAINT [FK_Project_User_TeamLeadId] FOREIGN KEY ([TeamLeadId]) REFERENCES [User] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020195900_InitialCreate'
)
BEGIN
    CREATE TABLE [UserGitAccount] (
        [Id] int NOT NULL IDENTITY,
        [UserId] int NOT NULL,
        [GitConnectionId] int NOT NULL,
        [ExternalUserId] nvarchar(64) NOT NULL,
        [UserName] nvarchar(255) NOT NULL,
        [DisplayName] nvarchar(255) NULL,
        [TokenType] int NOT NULL,
        [AccessToken] nvarchar(max) NOT NULL,
        [RefreshToken] nvarchar(max) NULL,
        [Scopes] nvarchar(500) NULL,
        [ExpiresOn] datetime2 NULL,
        [RefreshExpiresOn] datetime2 NULL,
        [ConnectedOn] datetime2 NOT NULL,
        [LastUsedOn] datetime2 NULL,
        CONSTRAINT [PK_UserGitAccount] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_UserGitAccount_GitConnection_GitConnectionId] FOREIGN KEY ([GitConnectionId]) REFERENCES [GitConnection] ([Id]),
        CONSTRAINT [FK_UserGitAccount_User_UserId] FOREIGN KEY ([UserId]) REFERENCES [User] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020195900_InitialCreate'
)
BEGIN
    CREATE TABLE [UserNotification] (
        [Id] int NOT NULL IDENTITY,
        [UserId] int NOT NULL,
        [Type] int NOT NULL,
        [Title] nvarchar(300) NOT NULL,
        [Message] nvarchar(1000) NULL,
        [Url] nvarchar(500) NULL,
        [DedupeKey] nvarchar(200) NULL,
        [IsRead] bit NOT NULL,
        [CreatedOnUtc] datetime2 NOT NULL,
        [ReadOnUtc] datetime2 NULL,
        CONSTRAINT [PK_UserNotification] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_UserNotification_User_UserId] FOREIGN KEY ([UserId]) REFERENCES [User] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020195900_InitialCreate'
)
BEGIN
    CREATE TABLE [UserPassword] (
        [Id] int NOT NULL IDENTITY,
        [UserId] int NOT NULL,
        [Password] nvarchar(max) NOT NULL,
        [PasswordSalt] nvarchar(max) NOT NULL,
        [CreatedOn] datetime2 NOT NULL,
        CONSTRAINT [PK_UserPassword] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_UserPassword_User_UserId] FOREIGN KEY ([UserId]) REFERENCES [User] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020195900_InitialCreate'
)
BEGIN
    CREATE TABLE [UserRoleMap] (
        [UserId] int NOT NULL,
        [UserRoleId] int NOT NULL,
        CONSTRAINT [PK_UserRoleMap] PRIMARY KEY ([UserId], [UserRoleId]),
        CONSTRAINT [FK_UserRoleMap_UserRole_UserRoleId] FOREIGN KEY ([UserRoleId]) REFERENCES [UserRole] ([Id]),
        CONSTRAINT [FK_UserRoleMap_User_UserId] FOREIGN KEY ([UserId]) REFERENCES [User] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020195900_InitialCreate'
)
BEGIN
    CREATE TABLE [CustomField] (
        [Id] int NOT NULL IDENTITY,
        [ProjectId] int NOT NULL,
        [Label] nvarchar(max) NOT NULL,
        [ResourceKey] nvarchar(100) NOT NULL,
        [FieldType] int NOT NULL,
        [Options] nvarchar(max) NULL,
        [Mandatory] bit NOT NULL,
        [ColumnClass] int NOT NULL,
        [Placement] int NOT NULL,
        CONSTRAINT [PK_CustomField] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_CustomField_Project_ProjectId] FOREIGN KEY ([ProjectId]) REFERENCES [Project] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020195900_InitialCreate'
)
BEGIN
    CREATE TABLE [Module] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(100) NOT NULL,
        [Description] nvarchar(max) NULL,
        [ProjectId] int NULL,
        [Active] bit NOT NULL,
        [Deleted] bit NOT NULL,
        CONSTRAINT [PK_Module] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Module_Project_ProjectId] FOREIGN KEY ([ProjectId]) REFERENCES [Project] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020195900_InitialCreate'
)
BEGIN
    CREATE TABLE [ProjectNotificationChannel] (
        [Id] int NOT NULL IDENTITY,
        [ProjectId] int NOT NULL,
        [NotificationChannelId] int NOT NULL,
        [Events] int NOT NULL,
        [Active] bit NOT NULL,
        CONSTRAINT [PK_ProjectNotificationChannel] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ProjectNotificationChannel_NotificationChannel_NotificationChannelId] FOREIGN KEY ([NotificationChannelId]) REFERENCES [NotificationChannel] ([Id]),
        CONSTRAINT [FK_ProjectNotificationChannel_Project_ProjectId] FOREIGN KEY ([ProjectId]) REFERENCES [Project] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020195900_InitialCreate'
)
BEGIN
    CREATE TABLE [Release] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(50) NOT NULL,
        [ReleaseType] int NOT NULL,
        [Description] nvarchar(500) NULL,
        [ProjectId] int NULL,
        [ReleaseDate] date NOT NULL,
        [Status] int NOT NULL,
        [Deleted] bit NOT NULL,
        CONSTRAINT [PK_Release] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Release_Project_ProjectId] FOREIGN KEY ([ProjectId]) REFERENCES [Project] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020195900_InitialCreate'
)
BEGIN
    CREATE TABLE [Reporter] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(100) NOT NULL,
        [Description] nvarchar(250) NULL,
        [ProjectId] int NULL,
        [Active] bit NOT NULL,
        [Deleted] bit NOT NULL,
        CONSTRAINT [PK_Reporter] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Reporter_Project_ProjectId] FOREIGN KEY ([ProjectId]) REFERENCES [Project] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020195900_InitialCreate'
)
BEGIN
    CREATE TABLE [Status] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(100) NOT NULL,
        [Description] nvarchar(250) NULL,
        [GroupId] int NOT NULL,
        [ProjectId] int NULL,
        [TextColor] nvarchar(20) NULL,
        [BackgroundColor] nvarchar(20) NULL,
        [IconClass] nvarchar(50) NULL,
        [RequiredRootCauseForBug] bit NOT NULL,
        [RequiredRootCauseForCr] bit NOT NULL,
        [RequiredResolutionForBug] bit NOT NULL,
        [RequiredResolutionForCr] bit NOT NULL,
        [RequiredMrNumberForBug] bit NOT NULL,
        [RequiredMrNumberForCr] bit NOT NULL,
        [SystemDefined] bit NOT NULL,
        [ExternalUse] bit NOT NULL,
        [Active] bit NOT NULL,
        [Deleted] bit NOT NULL,
        CONSTRAINT [PK_Status] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Status_Project_ProjectId] FOREIGN KEY ([ProjectId]) REFERENCES [Project] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020195900_InitialCreate'
)
BEGIN
    CREATE TABLE [UserProjectMap] (
        [Id] int NOT NULL IDENTITY,
        [UserId] int NOT NULL,
        [ProjectId] int NOT NULL,
        [CanReportToDo] bit NOT NULL,
        [CanReportCR] bit NOT NULL,
        [CanReportBug] bit NOT NULL,
        [CanEdit] bit NOT NULL,
        [CanClose] bit NOT NULL,
        [CanReOpen] bit NOT NULL,
        [CanComment] bit NOT NULL,
        [CanViewOthersTask] bit NOT NULL,
        [CanEditOthersTask] bit NOT NULL,
        [CanAddWiki] bit NOT NULL,
        [CanDeleteTask] bit NOT NULL,
        [CanChangeProject] bit NOT NULL,
        [CanReviewMergeRequests] bit NOT NULL,
        [CanMergeMergeRequests] bit NOT NULL,
        CONSTRAINT [PK_UserProjectMap] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_UserProjectMap_Project_ProjectId] FOREIGN KEY ([ProjectId]) REFERENCES [Project] ([Id]),
        CONSTRAINT [FK_UserProjectMap_User_UserId] FOREIGN KEY ([UserId]) REFERENCES [User] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020195900_InitialCreate'
)
BEGIN
    CREATE TABLE [WikiCategory] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(100) NOT NULL,
        [Description] nvarchar(250) NULL,
        [ParentCategoryId] int NULL,
        [ProjectId] int NOT NULL,
        [Deleted] bit NOT NULL,
        [Active] bit NOT NULL,
        CONSTRAINT [PK_WikiCategory] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_WikiCategory_Project_ProjectId] FOREIGN KEY ([ProjectId]) REFERENCES [Project] ([Id]),
        CONSTRAINT [FK_WikiCategory_WikiCategory_ParentCategoryId] FOREIGN KEY ([ParentCategoryId]) REFERENCES [WikiCategory] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020195900_InitialCreate'
)
BEGIN
    CREATE TABLE [SubModule] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(100) NOT NULL,
        [Description] nvarchar(250) NULL,
        [ModuleId] int NOT NULL,
        [OwnerId] int NULL,
        [Active] bit NOT NULL,
        [Deleted] bit NOT NULL,
        CONSTRAINT [PK_SubModule] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_SubModule_Module_ModuleId] FOREIGN KEY ([ModuleId]) REFERENCES [Module] ([Id]),
        CONSTRAINT [FK_SubModule_User_OwnerId] FOREIGN KEY ([OwnerId]) REFERENCES [User] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020195900_InitialCreate'
)
BEGIN
    CREATE TABLE [ReleaseProject] (
        [Id] int NOT NULL IDENTITY,
        [ReleaseId] int NOT NULL,
        [ProjectId] int NOT NULL,
        CONSTRAINT [PK_ReleaseProject] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ReleaseProject_Project_ProjectId] FOREIGN KEY ([ProjectId]) REFERENCES [Project] ([Id]),
        CONSTRAINT [FK_ReleaseProject_Release_ReleaseId] FOREIGN KEY ([ReleaseId]) REFERENCES [Release] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020195900_InitialCreate'
)
BEGIN
    CREATE TABLE [ProjectRepository] (
        [Id] int NOT NULL IDENTITY,
        [ProjectId] int NOT NULL,
        [GitConnectionId] int NOT NULL,
        [RepoRef] nvarchar(255) NOT NULL,
        [ExternalRepoId] nvarchar(64) NULL,
        [DisplayName] nvarchar(255) NULL,
        [WebUrl] nvarchar(500) NULL,
        [DefaultBranch] nvarchar(255) NULL,
        [IsPrivate] bit NULL,
        [IsPrimary] bit NOT NULL,
        [WebhookKey] nvarchar(64) NOT NULL,
        [WebhookSecret] nvarchar(max) NULL,
        [WebhookSecretCreatedOn] datetime2 NULL,
        [WebhookLastDeliveryOn] datetime2 NULL,
        [IssueSyncMode] int NOT NULL,
        [IssueSyncEnabledOn] datetime2 NULL,
        [IssueImportLabel] nvarchar(100) NULL,
        [IssueSyncTaskTypeIds] nvarchar(200) NULL,
        [IssueSyncComments] bit NOT NULL,
        [IssueTaskTypeId] int NULL,
        [IssueSeverityId] int NULL,
        [IssueOpenStatusId] int NULL,
        [IssueClosedStatusId] int NULL,
        [IssueSyncUserId] int NULL,
        [IssueImportLimitPerHour] int NOT NULL,
        [IssueLastPolledOn] datetime2 NULL,
        [Active] bit NOT NULL,
        [Deleted] bit NOT NULL,
        [CreatedOn] datetime2 NOT NULL,
        [ModifiedOn] datetime2 NULL,
        CONSTRAINT [PK_ProjectRepository] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ProjectRepository_GitConnection_GitConnectionId] FOREIGN KEY ([GitConnectionId]) REFERENCES [GitConnection] ([Id]),
        CONSTRAINT [FK_ProjectRepository_Project_ProjectId] FOREIGN KEY ([ProjectId]) REFERENCES [Project] ([Id]),
        CONSTRAINT [FK_ProjectRepository_Severity_IssueSeverityId] FOREIGN KEY ([IssueSeverityId]) REFERENCES [Severity] ([Id]),
        CONSTRAINT [FK_ProjectRepository_Status_IssueClosedStatusId] FOREIGN KEY ([IssueClosedStatusId]) REFERENCES [Status] ([Id]),
        CONSTRAINT [FK_ProjectRepository_Status_IssueOpenStatusId] FOREIGN KEY ([IssueOpenStatusId]) REFERENCES [Status] ([Id]),
        CONSTRAINT [FK_ProjectRepository_TaskType_IssueTaskTypeId] FOREIGN KEY ([IssueTaskTypeId]) REFERENCES [TaskType] ([Id]),
        CONSTRAINT [FK_ProjectRepository_User_IssueSyncUserId] FOREIGN KEY ([IssueSyncUserId]) REFERENCES [User] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020195900_InitialCreate'
)
BEGIN
    CREATE TABLE [StatusDurationMetric] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(100) NOT NULL,
        [FromStatusId] int NOT NULL,
        [ToStatusId] int NOT NULL,
        [ProjectId] int NULL,
        [ShowOnTask] bit NOT NULL,
        [DisplayOrder] int NOT NULL,
        [Active] bit NOT NULL,
        [Deleted] bit NOT NULL,
        CONSTRAINT [PK_StatusDurationMetric] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_StatusDurationMetric_Project_ProjectId] FOREIGN KEY ([ProjectId]) REFERENCES [Project] ([Id]),
        CONSTRAINT [FK_StatusDurationMetric_Status_FromStatusId] FOREIGN KEY ([FromStatusId]) REFERENCES [Status] ([Id]),
        CONSTRAINT [FK_StatusDurationMetric_Status_ToStatusId] FOREIGN KEY ([ToStatusId]) REFERENCES [Status] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020195900_InitialCreate'
)
BEGIN
    CREATE TABLE [WikiDocument] (
        [Id] int NOT NULL IDENTITY,
        [Title] nvarchar(250) NOT NULL,
        [ContentHtml] nvarchar(max) NOT NULL,
        [CategoryId] int NOT NULL,
        [ModuleId] int NULL,
        [SubModuleId] int NULL,
        [CreatedOn] datetime2 NOT NULL,
        [CreatedById] int NOT NULL,
        [ModifiedOn] datetime2 NOT NULL,
        [ModifiedById] int NOT NULL,
        [Deleted] bit NOT NULL,
        CONSTRAINT [PK_WikiDocument] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_WikiDocument_Module_ModuleId] FOREIGN KEY ([ModuleId]) REFERENCES [Module] ([Id]),
        CONSTRAINT [FK_WikiDocument_SubModule_SubModuleId] FOREIGN KEY ([SubModuleId]) REFERENCES [SubModule] ([Id]),
        CONSTRAINT [FK_WikiDocument_User_CreatedById] FOREIGN KEY ([CreatedById]) REFERENCES [User] ([Id]),
        CONSTRAINT [FK_WikiDocument_User_ModifiedById] FOREIGN KEY ([ModifiedById]) REFERENCES [User] ([Id]),
        CONSTRAINT [FK_WikiDocument_WikiCategory_CategoryId] FOREIGN KEY ([CategoryId]) REFERENCES [WikiCategory] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020195900_InitialCreate'
)
BEGIN
    CREATE TABLE [GitBranch] (
        [Id] int NOT NULL IDENTITY,
        [ProjectId] int NOT NULL,
        [ProjectRepositoryId] int NULL,
        [GitProjectReference] nvarchar(255) NOT NULL,
        [Name] nvarchar(255) NOT NULL,
        [BaseBranch] nvarchar(255) NULL,
        [Origin] int NULL,
        [CreatedById] int NOT NULL,
        [CreatedOn] datetime2 NOT NULL,
        [Deleted] bit NOT NULL,
        CONSTRAINT [PK_GitBranch] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_GitBranch_ProjectRepository_ProjectRepositoryId] FOREIGN KEY ([ProjectRepositoryId]) REFERENCES [ProjectRepository] ([Id]),
        CONSTRAINT [FK_GitBranch_Project_ProjectId] FOREIGN KEY ([ProjectId]) REFERENCES [Project] ([Id]),
        CONSTRAINT [FK_GitBranch_User_CreatedById] FOREIGN KEY ([CreatedById]) REFERENCES [User] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020195900_InitialCreate'
)
BEGIN
    CREATE TABLE [GitWebhookDelivery] (
        [Id] int NOT NULL IDENTITY,
        [ProjectRepositoryId] int NOT NULL,
        [DeliveryId] nvarchar(100) NOT NULL,
        [EventName] nvarchar(100) NOT NULL,
        [Status] int NOT NULL,
        [ReceivedOn] datetime2 NOT NULL,
        [ProcessedOn] datetime2 NULL,
        [Attempts] int NOT NULL,
        [Error] nvarchar(1000) NULL,
        [Payload] nvarchar(max) NULL,
        CONSTRAINT [PK_GitWebhookDelivery] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_GitWebhookDelivery_ProjectRepository_ProjectRepositoryId] FOREIGN KEY ([ProjectRepositoryId]) REFERENCES [ProjectRepository] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020195900_InitialCreate'
)
BEGIN
    CREATE TABLE [Backlog] (
        [Id] int NOT NULL IDENTITY,
        [Code] uniqueidentifier NOT NULL,
        [Title] nvarchar(500) NOT NULL,
        [Description] nvarchar(max) NULL,
        [RootCauseAnalysis] nvarchar(max) NULL,
        [ExternalNotes] nvarchar(max) NULL,
        [EnvironmentJson] nvarchar(max) NULL,
        [ParentId] int NULL,
        [TaskTypeId] int NOT NULL,
        [SeverityId] int NOT NULL,
        [DueDate] date NULL,
        [PlannedStartDate] date NULL,
        [ProjectId] int NOT NULL,
        [ModuleId] int NULL,
        [SubModuleId] int NULL,
        [ReleaseId] int NULL,
        [ReporterId] int NULL,
        [AssigneeId] int NULL,
        [TesterId] int NULL,
        [ReOpenCount] int NOT NULL,
        [SubTaskCount] int NOT NULL,
        [AttachmentCount] int NULL,
        [CommentCount] int NULL,
        [StatusId] int NOT NULL,
        [ResolutionDetails] nvarchar(max) NULL,
        [MergeRequestId] int NULL,
        [MergeRequestRepositoryId] int NULL,
        [SourceId] int NULL,
        [SourceWikiDocumentId] int NULL,
        [CreatedById] int NOT NULL,
        [CreatedOn] datetime2 NOT NULL,
        [ModifiedById] int NULL,
        [ModifiedOn] datetime2 NULL,
        [Deleted] bit NOT NULL,
        CONSTRAINT [PK_Backlog] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Backlog_Backlog_ParentId] FOREIGN KEY ([ParentId]) REFERENCES [Backlog] ([Id]),
        CONSTRAINT [FK_Backlog_Module_ModuleId] FOREIGN KEY ([ModuleId]) REFERENCES [Module] ([Id]),
        CONSTRAINT [FK_Backlog_ProjectRepository_MergeRequestRepositoryId] FOREIGN KEY ([MergeRequestRepositoryId]) REFERENCES [ProjectRepository] ([Id]),
        CONSTRAINT [FK_Backlog_Project_ProjectId] FOREIGN KEY ([ProjectId]) REFERENCES [Project] ([Id]),
        CONSTRAINT [FK_Backlog_Release_ReleaseId] FOREIGN KEY ([ReleaseId]) REFERENCES [Release] ([Id]),
        CONSTRAINT [FK_Backlog_Reporter_ReporterId] FOREIGN KEY ([ReporterId]) REFERENCES [Reporter] ([Id]),
        CONSTRAINT [FK_Backlog_Severity_SeverityId] FOREIGN KEY ([SeverityId]) REFERENCES [Severity] ([Id]),
        CONSTRAINT [FK_Backlog_Status_StatusId] FOREIGN KEY ([StatusId]) REFERENCES [Status] ([Id]),
        CONSTRAINT [FK_Backlog_SubModule_SubModuleId] FOREIGN KEY ([SubModuleId]) REFERENCES [SubModule] ([Id]),
        CONSTRAINT [FK_Backlog_TaskType_TaskTypeId] FOREIGN KEY ([TaskTypeId]) REFERENCES [TaskType] ([Id]),
        CONSTRAINT [FK_Backlog_User_AssigneeId] FOREIGN KEY ([AssigneeId]) REFERENCES [User] ([Id]),
        CONSTRAINT [FK_Backlog_User_CreatedById] FOREIGN KEY ([CreatedById]) REFERENCES [User] ([Id]),
        CONSTRAINT [FK_Backlog_User_ModifiedById] FOREIGN KEY ([ModifiedById]) REFERENCES [User] ([Id]),
        CONSTRAINT [FK_Backlog_User_TesterId] FOREIGN KEY ([TesterId]) REFERENCES [User] ([Id]),
        CONSTRAINT [FK_Backlog_WikiDocument_SourceWikiDocumentId] FOREIGN KEY ([SourceWikiDocumentId]) REFERENCES [WikiDocument] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020195900_InitialCreate'
)
BEGIN
    CREATE TABLE [WikiDocumentVersion] (
        [Id] int NOT NULL IDENTITY,
        [WikiDocumentId] int NOT NULL,
        [Title] nvarchar(250) NOT NULL,
        [ContentHtml] nvarchar(max) NOT NULL,
        [CreatedOn] datetime2 NOT NULL,
        [ModifiedById] int NOT NULL,
        [Note] nvarchar(max) NULL,
        CONSTRAINT [PK_WikiDocumentVersion] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_WikiDocumentVersion_User_ModifiedById] FOREIGN KEY ([ModifiedById]) REFERENCES [User] ([Id]),
        CONSTRAINT [FK_WikiDocumentVersion_WikiDocument_WikiDocumentId] FOREIGN KEY ([WikiDocumentId]) REFERENCES [WikiDocument] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020195900_InitialCreate'
)
BEGIN
    CREATE TABLE [BacklogComment] (
        [Id] int NOT NULL IDENTITY,
        [BacklogId] int NOT NULL,
        [Comment] nvarchar(max) NOT NULL,
        [CreatedById] int NOT NULL,
        [SystemComment] bit NOT NULL,
        [PostToIssue] bit NOT NULL,
        [FieldName] nvarchar(100) NULL,
        [OldValue] nvarchar(max) NULL,
        [NewValue] nvarchar(max) NULL,
        [CreatedOn] datetime2 NOT NULL,
        [ModifiedById] int NULL,
        [ModifiedOn] datetime2 NULL,
        CONSTRAINT [PK_BacklogComment] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_BacklogComment_Backlog_BacklogId] FOREIGN KEY ([BacklogId]) REFERENCES [Backlog] ([Id]),
        CONSTRAINT [FK_BacklogComment_User_CreatedById] FOREIGN KEY ([CreatedById]) REFERENCES [User] ([Id]),
        CONSTRAINT [FK_BacklogComment_User_ModifiedById] FOREIGN KEY ([ModifiedById]) REFERENCES [User] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020195900_InitialCreate'
)
BEGIN
    CREATE TABLE [BacklogCustomFieldValue] (
        [Id] int NOT NULL IDENTITY,
        [BacklogId] int NOT NULL,
        [CustomFieldId] int NOT NULL,
        [Value] nvarchar(max) NULL,
        CONSTRAINT [PK_BacklogCustomFieldValue] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_BacklogCustomFieldValue_Backlog_BacklogId] FOREIGN KEY ([BacklogId]) REFERENCES [Backlog] ([Id]),
        CONSTRAINT [FK_BacklogCustomFieldValue_CustomField_CustomFieldId] FOREIGN KEY ([CustomFieldId]) REFERENCES [CustomField] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020195900_InitialCreate'
)
BEGIN
    CREATE TABLE [BacklogDocument] (
        [Id] int NOT NULL IDENTITY,
        [BacklogId] int NOT NULL,
        [DocumentId] int NOT NULL,
        [CreatedById] int NOT NULL,
        [CreatedOn] datetime2 NOT NULL,
        CONSTRAINT [PK_BacklogDocument] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_BacklogDocument_Backlog_BacklogId] FOREIGN KEY ([BacklogId]) REFERENCES [Backlog] ([Id]),
        CONSTRAINT [FK_BacklogDocument_Document_DocumentId] FOREIGN KEY ([DocumentId]) REFERENCES [Document] ([Id]),
        CONSTRAINT [FK_BacklogDocument_User_CreatedById] FOREIGN KEY ([CreatedById]) REFERENCES [User] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020195900_InitialCreate'
)
BEGIN
    CREATE TABLE [BacklogStatusLog] (
        [Id] int NOT NULL IDENTITY,
        [BacklogId] int NOT NULL,
        [StatusId] int NOT NULL,
        [CreatedById] int NOT NULL,
        [CreatedOn] datetime2 NOT NULL,
        CONSTRAINT [PK_BacklogStatusLog] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_BacklogStatusLog_Backlog_BacklogId] FOREIGN KEY ([BacklogId]) REFERENCES [Backlog] ([Id]),
        CONSTRAINT [FK_BacklogStatusLog_Status_StatusId] FOREIGN KEY ([StatusId]) REFERENCES [Status] ([Id]),
        CONSTRAINT [FK_BacklogStatusLog_User_CreatedById] FOREIGN KEY ([CreatedById]) REFERENCES [User] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020195900_InitialCreate'
)
BEGIN
    CREATE TABLE [BacklogTag] (
        [Id] int NOT NULL IDENTITY,
        [BacklogId] int NOT NULL,
        [TagId] int NOT NULL,
        CONSTRAINT [PK_BacklogTag] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_BacklogTag_Backlog_BacklogId] FOREIGN KEY ([BacklogId]) REFERENCES [Backlog] ([Id]),
        CONSTRAINT [FK_BacklogTag_Tag_TagId] FOREIGN KEY ([TagId]) REFERENCES [Tag] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020195900_InitialCreate'
)
BEGIN
    CREATE TABLE [ExternalIssueLink] (
        [Id] int NOT NULL IDENTITY,
        [BacklogId] int NOT NULL,
        [ProjectRepositoryId] int NOT NULL,
        [ExternalId] nvarchar(64) NOT NULL,
        [Number] int NOT NULL,
        [Url] nvarchar(500) NULL,
        [RemoteState] nvarchar(20) NULL,
        [Snapshot] nvarchar(max) NULL,
        [SyncEnabled] bit NOT NULL,
        [CreatedOn] datetime2 NOT NULL,
        [LastSyncedOn] datetime2 NULL,
        [RemoteUpdatedOn] datetime2 NULL,
        [LastError] nvarchar(1000) NULL,
        CONSTRAINT [PK_ExternalIssueLink] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ExternalIssueLink_Backlog_BacklogId] FOREIGN KEY ([BacklogId]) REFERENCES [Backlog] ([Id]),
        CONSTRAINT [FK_ExternalIssueLink_ProjectRepository_ProjectRepositoryId] FOREIGN KEY ([ProjectRepositoryId]) REFERENCES [ProjectRepository] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020195900_InitialCreate'
)
BEGIN
    CREATE TABLE [GitBranchTask] (
        [Id] int NOT NULL IDENTITY,
        [GitBranchId] int NOT NULL,
        [BacklogId] int NOT NULL,
        [LinkedById] int NOT NULL,
        [LinkedOn] datetime2 NOT NULL,
        CONSTRAINT [PK_GitBranchTask] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_GitBranchTask_Backlog_BacklogId] FOREIGN KEY ([BacklogId]) REFERENCES [Backlog] ([Id]),
        CONSTRAINT [FK_GitBranchTask_GitBranch_GitBranchId] FOREIGN KEY ([GitBranchId]) REFERENCES [GitBranch] ([Id]),
        CONSTRAINT [FK_GitBranchTask_User_LinkedById] FOREIGN KEY ([LinkedById]) REFERENCES [User] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020195900_InitialCreate'
)
BEGIN
    CREATE TABLE [ExternalCommentLink] (
        [Id] int NOT NULL IDENTITY,
        [ExternalIssueLinkId] int NOT NULL,
        [BacklogCommentId] int NOT NULL,
        [ExternalCommentId] nvarchar(64) NOT NULL,
        [Inbound] bit NOT NULL,
        [ContentHash] nvarchar(64) NULL,
        [LastSyncedOn] datetime2 NOT NULL,
        CONSTRAINT [PK_ExternalCommentLink] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ExternalCommentLink_BacklogComment_BacklogCommentId] FOREIGN KEY ([BacklogCommentId]) REFERENCES [BacklogComment] ([Id]),
        CONSTRAINT [FK_ExternalCommentLink_ExternalIssueLink_ExternalIssueLinkId] FOREIGN KEY ([ExternalIssueLinkId]) REFERENCES [ExternalIssueLink] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020195900_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Announcement_Active_Deleted] ON [Announcement] ([Active], [Deleted]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020195900_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_AnnouncementAcknowledgement_AnnouncementId_UserId] ON [AnnouncementAcknowledgement] ([AnnouncementId], [UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020195900_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AnnouncementAcknowledgement_UserId] ON [AnnouncementAcknowledgement] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020195900_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AnnouncementRole_AnnouncementId_UserRoleId] ON [AnnouncementRole] ([AnnouncementId], [UserRoleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020195900_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AnnouncementRole_UserRoleId] ON [AnnouncementRole] ([UserRoleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020195900_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AuditLog_EntityName_EntityId] ON [AuditLog] ([EntityName], [EntityId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020195900_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Backlog_AssigneeId] ON [Backlog] ([AssigneeId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020195900_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Backlog_CreatedById] ON [Backlog] ([CreatedById]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020195900_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Backlog_MergeRequestRepositoryId] ON [Backlog] ([MergeRequestRepositoryId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020195900_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Backlog_ModifiedById] ON [Backlog] ([ModifiedById]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020195900_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Backlog_ModuleId] ON [Backlog] ([ModuleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020195900_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Backlog_ParentId] ON [Backlog] ([ParentId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020195900_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Backlog_ProjectId] ON [Backlog] ([ProjectId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020195900_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Backlog_ReleaseId] ON [Backlog] ([ReleaseId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020195900_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Backlog_ReporterId] ON [Backlog] ([ReporterId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020195900_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Backlog_SeverityId] ON [Backlog] ([SeverityId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020195900_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Backlog_SourceWikiDocumentId] ON [Backlog] ([SourceWikiDocumentId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020195900_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Backlog_StatusId] ON [Backlog] ([StatusId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020195900_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Backlog_SubModuleId] ON [Backlog] ([SubModuleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020195900_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Backlog_TaskTypeId] ON [Backlog] ([TaskTypeId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020195900_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Backlog_TesterId] ON [Backlog] ([TesterId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020195900_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_BacklogComment_BacklogId] ON [BacklogComment] ([BacklogId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020195900_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_BacklogComment_CreatedById] ON [BacklogComment] ([CreatedById]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020195900_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_BacklogComment_ModifiedById] ON [BacklogComment] ([ModifiedById]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020195900_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_BacklogCustomFieldValue_BacklogId] ON [BacklogCustomFieldValue] ([BacklogId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020195900_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_BacklogCustomFieldValue_CustomFieldId] ON [BacklogCustomFieldValue] ([CustomFieldId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020195900_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_BacklogDocument_BacklogId] ON [BacklogDocument] ([BacklogId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020195900_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_BacklogDocument_CreatedById] ON [BacklogDocument] ([CreatedById]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020195900_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_BacklogDocument_DocumentId] ON [BacklogDocument] ([DocumentId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020195900_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_BacklogStatusLog_BacklogId] ON [BacklogStatusLog] ([BacklogId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020195900_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_BacklogStatusLog_CreatedById] ON [BacklogStatusLog] ([CreatedById]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020195900_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_BacklogStatusLog_StatusId] ON [BacklogStatusLog] ([StatusId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020195900_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_BacklogTag_BacklogId_TagId] ON [BacklogTag] ([BacklogId], [TagId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020195900_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_BacklogTag_TagId] ON [BacklogTag] ([TagId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020195900_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_CustomField_ProjectId] ON [CustomField] ([ProjectId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020195900_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_EmailTemplate_EmailAccountId] ON [EmailTemplate] ([EmailAccountId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020195900_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ExternalCommentLink_BacklogCommentId] ON [ExternalCommentLink] ([BacklogCommentId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020195900_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ExternalCommentLink_ExternalIssueLinkId_ExternalCommentId] ON [ExternalCommentLink] ([ExternalIssueLinkId], [ExternalCommentId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020195900_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ExternalIssueLink_BacklogId] ON [ExternalIssueLink] ([BacklogId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020195900_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ExternalIssueLink_ProjectRepositoryId_Number] ON [ExternalIssueLink] ([ProjectRepositoryId], [Number]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020195900_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_GitBranch_CreatedById] ON [GitBranch] ([CreatedById]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020195900_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_GitBranch_GitProjectReference_Name] ON [GitBranch] ([GitProjectReference], [Name]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020195900_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_GitBranch_ProjectId] ON [GitBranch] ([ProjectId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020195900_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_GitBranch_ProjectRepositoryId] ON [GitBranch] ([ProjectRepositoryId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020195900_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_GitBranchTask_BacklogId] ON [GitBranchTask] ([BacklogId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020195900_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_GitBranchTask_GitBranchId_BacklogId] ON [GitBranchTask] ([GitBranchId], [BacklogId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020195900_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_GitBranchTask_LinkedById] ON [GitBranchTask] ([LinkedById]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020195900_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_GitWebhookDelivery_ProjectRepositoryId_DeliveryId] ON [GitWebhookDelivery] ([ProjectRepositoryId], [DeliveryId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020195900_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_GitWebhookDelivery_ReceivedOn] ON [GitWebhookDelivery] ([ReceivedOn]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020195900_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_LocaleResource_LanguageId] ON [LocaleResource] ([LanguageId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020195900_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Module_ProjectId] ON [Module] ([ProjectId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020195900_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Project_ClientId] ON [Project] ([ClientId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020195900_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Project_CodeReviewerId] ON [Project] ([CodeReviewerId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020195900_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Project_ProjectManagerId] ON [Project] ([ProjectManagerId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020195900_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Project_TeamLeadId] ON [Project] ([TeamLeadId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020195900_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ProjectNotificationChannel_NotificationChannelId] ON [ProjectNotificationChannel] ([NotificationChannelId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020195900_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ProjectNotificationChannel_ProjectId_NotificationChannelId] ON [ProjectNotificationChannel] ([ProjectId], [NotificationChannelId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020195900_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ProjectRepository_GitConnectionId] ON [ProjectRepository] ([GitConnectionId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020195900_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ProjectRepository_IssueClosedStatusId] ON [ProjectRepository] ([IssueClosedStatusId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020195900_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ProjectRepository_IssueOpenStatusId] ON [ProjectRepository] ([IssueOpenStatusId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020195900_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ProjectRepository_IssueSeverityId] ON [ProjectRepository] ([IssueSeverityId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020195900_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ProjectRepository_IssueSyncUserId] ON [ProjectRepository] ([IssueSyncUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020195900_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ProjectRepository_IssueTaskTypeId] ON [ProjectRepository] ([IssueTaskTypeId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020195900_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ProjectRepository_ProjectId_GitConnectionId_RepoRef] ON [ProjectRepository] ([ProjectId], [GitConnectionId], [RepoRef]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020195900_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ProjectRepository_WebhookKey] ON [ProjectRepository] ([WebhookKey]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020195900_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Release_ProjectId] ON [Release] ([ProjectId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020195900_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ReleaseProject_ProjectId] ON [ReleaseProject] ([ProjectId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020195900_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ReleaseProject_ReleaseId_ProjectId] ON [ReleaseProject] ([ReleaseId], [ProjectId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020195900_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Reporter_ProjectId] ON [Reporter] ([ProjectId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020195900_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Status_ProjectId] ON [Status] ([ProjectId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020195900_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_StatusDurationMetric_FromStatusId] ON [StatusDurationMetric] ([FromStatusId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020195900_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_StatusDurationMetric_ProjectId] ON [StatusDurationMetric] ([ProjectId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020195900_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_StatusDurationMetric_ToStatusId] ON [StatusDurationMetric] ([ToStatusId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020195900_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_SubModule_ModuleId] ON [SubModule] ([ModuleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020195900_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_SubModule_OwnerId] ON [SubModule] ([OwnerId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020195900_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_User_AddressId] ON [User] ([AddressId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020195900_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_User_LanguageId] ON [User] ([LanguageId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020195900_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_UserGitAccount_GitConnectionId_ExternalUserId] ON [UserGitAccount] ([GitConnectionId], [ExternalUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020195900_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_UserGitAccount_UserId_GitConnectionId] ON [UserGitAccount] ([UserId], [GitConnectionId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020195900_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_UserNotification_UserId_DedupeKey] ON [UserNotification] ([UserId], [DedupeKey]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020195900_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_UserNotification_UserId_IsRead_CreatedOnUtc] ON [UserNotification] ([UserId], [IsRead], [CreatedOnUtc]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020195900_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_UserPassword_UserId] ON [UserPassword] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020195900_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_UserProjectMap_ProjectId] ON [UserProjectMap] ([ProjectId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020195900_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_UserProjectMap_UserId] ON [UserProjectMap] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020195900_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_UserRoleMap_UserRoleId] ON [UserRoleMap] ([UserRoleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020195900_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_UserRolePermissionMap_UserRoleId] ON [UserRolePermissionMap] ([UserRoleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020195900_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_WikiCategory_ParentCategoryId] ON [WikiCategory] ([ParentCategoryId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020195900_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_WikiCategory_ProjectId] ON [WikiCategory] ([ProjectId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020195900_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_WikiDocument_CategoryId] ON [WikiDocument] ([CategoryId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020195900_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_WikiDocument_CreatedById] ON [WikiDocument] ([CreatedById]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020195900_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_WikiDocument_ModifiedById] ON [WikiDocument] ([ModifiedById]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020195900_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_WikiDocument_ModuleId] ON [WikiDocument] ([ModuleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020195900_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_WikiDocument_SubModuleId] ON [WikiDocument] ([SubModuleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020195900_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_WikiDocumentVersion_ModifiedById] ON [WikiDocumentVersion] ([ModifiedById]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020195900_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_WikiDocumentVersion_WikiDocumentId] ON [WikiDocumentVersion] ([WikiDocumentId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020195900_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261020195900_InitialCreate', N'10.0.13');
END;

COMMIT;
GO

