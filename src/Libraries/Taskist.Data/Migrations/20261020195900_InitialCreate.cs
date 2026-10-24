using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Taskist.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ActivityLog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SystemName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EntityId = table.Column<int>(type: "int", nullable: true),
                    EntityName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IpAddress = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivityLog", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Address",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Address1 = table.Column<string>(type: "nvarchar(750)", maxLength: 750, nullable: false),
                    Address2 = table.Column<string>(type: "nvarchar(750)", maxLength: 750, nullable: true),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ZipPostalCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Address", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Announcement",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Body = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Severity = table.Column<int>(type: "int", nullable: false),
                    Audience = table.Column<int>(type: "int", nullable: false),
                    ValidFrom = table.Column<DateOnly>(type: "date", nullable: false),
                    ValidTo = table.Column<DateOnly>(type: "date", nullable: false),
                    RequiresAcknowledgement = table.Column<bool>(type: "bit", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    Deleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Announcement", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AuditLog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EntityName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    EntityId = table.Column<int>(type: "int", nullable: true),
                    Action = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    OldValues = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NewValues = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IpAddress = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLog", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Client",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    ContactPerson = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Website = table.Column<string>(type: "nvarchar(750)", maxLength: 750, nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    Deleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Client", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Document",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    FileData = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    Extension = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Document", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EmailAccount",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    UserName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Host = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Port = table.Column<int>(type: "int", nullable: false),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EnableSsl = table.Column<bool>(type: "bit", nullable: false),
                    FromEmail = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FromName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailAccount", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GenericAttribute",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EntityId = table.Column<int>(type: "int", nullable: false),
                    KeyGroup = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Key = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedOrUpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GenericAttribute", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GitConnection",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ProviderType = table.Column<int>(type: "int", nullable: false),
                    BaseUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ApiUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    AuthMode = table.Column<int>(type: "int", nullable: false),
                    ServiceToken = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AppId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    AppPrivateKey = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OAuthClientId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    OAuthClientSecret = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ServiceAccountUserName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ServiceAccountExternalId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    AllowInsecureHttp = table.Column<bool>(type: "bit", nullable: false),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GitConnection", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Language",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(750)", maxLength: 750, nullable: false),
                    LanguageCulture = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(750)", maxLength: 750, nullable: false),
                    Rtl = table.Column<bool>(type: "bit", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Language", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Log",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LogLevelId = table.Column<int>(type: "int", nullable: false),
                    ShortMessage = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FullMessage = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IpAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    PageUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReferrerUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LogLevel = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Log", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Menu",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SystemName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Code = table.Column<int>(type: "int", nullable: false),
                    ActionName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ControllerName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ParentCode = table.Column<int>(type: "int", nullable: true),
                    Icon = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    Permission = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Menu", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NotificationChannel",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ChannelType = table.Column<int>(type: "int", nullable: false),
                    EndpointUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SigningSecret = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastSentOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastError = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationChannel", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Setting",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Setting", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Severity",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    GroupId = table.Column<int>(type: "int", nullable: false),
                    TextColor = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    BackgroundColor = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    IconClass = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    Deleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Severity", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tag",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    Deleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tag", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TaskType",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    GroupId = table.Column<int>(type: "int", nullable: false),
                    TextColor = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    BackgroundColor = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    IconClass = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    Deleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserRole",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SystemName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    SystemDefined = table.Column<bool>(type: "bit", nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRole", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserRolePermission",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SystemName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    RoleGroup = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    SystemDefined = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRolePermission", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EmailTemplate",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EmailSubject = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EmailBody = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EmailAccountId = table.Column<int>(type: "int", nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailTemplate", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmailTemplate_EmailAccount_EmailAccountId",
                        column: x => x.EmailAccountId,
                        principalTable: "EmailAccount",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LocaleResource",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LanguageId = table.Column<int>(type: "int", nullable: false),
                    ResourceKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ResourceValue = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocaleResource", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LocaleResource_Language_LanguageId",
                        column: x => x.LanguageId,
                        principalTable: "Language",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FriendlyName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    MobileNumber = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    GenderId = table.Column<int>(type: "int", nullable: false),
                    MaritalStatus = table.Column<int>(type: "int", nullable: false),
                    DateOfBirth = table.Column<DateOnly>(type: "date", nullable: false),
                    DateOfJoin = table.Column<DateOnly>(type: "date", nullable: false),
                    LanguageId = table.Column<int>(type: "int", nullable: false),
                    AddressId = table.Column<int>(type: "int", nullable: false),
                    GitUserName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PhotoDocumentId = table.Column<int>(type: "int", nullable: true),
                    SystemAccount = table.Column<bool>(type: "bit", nullable: false),
                    FailedLoginAttempts = table.Column<int>(type: "int", nullable: false),
                    LastIPAddress = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    LastLoginDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastActivityDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Locked = table.Column<bool>(type: "bit", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Deleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.Id);
                    table.ForeignKey(
                        name: "FK_User_Address_AddressId",
                        column: x => x.AddressId,
                        principalTable: "Address",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_User_Language_LanguageId",
                        column: x => x.LanguageId,
                        principalTable: "Language",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AnnouncementRole",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AnnouncementId = table.Column<int>(type: "int", nullable: false),
                    UserRoleId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnnouncementRole", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AnnouncementRole_Announcement_AnnouncementId",
                        column: x => x.AnnouncementId,
                        principalTable: "Announcement",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AnnouncementRole_UserRole_UserRoleId",
                        column: x => x.UserRoleId,
                        principalTable: "UserRole",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "UserRolePermissionMap",
                columns: table => new
                {
                    PermissionId = table.Column<int>(type: "int", nullable: false),
                    UserRoleId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRolePermissionMap", x => new { x.PermissionId, x.UserRoleId });
                    table.ForeignKey(
                        name: "FK_UserRolePermissionMap_UserRolePermission_PermissionId",
                        column: x => x.PermissionId,
                        principalTable: "UserRolePermission",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_UserRolePermissionMap_UserRole_UserRoleId",
                        column: x => x.UserRoleId,
                        principalTable: "UserRole",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AnnouncementAcknowledgement",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AnnouncementId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    AcknowledgedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnnouncementAcknowledgement", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AnnouncementAcknowledgement_Announcement_AnnouncementId",
                        column: x => x.AnnouncementId,
                        principalTable: "Announcement",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AnnouncementAcknowledgement_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Project",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: true),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    ClientId = table.Column<int>(type: "int", nullable: false),
                    ProjectManagerId = table.Column<int>(type: "int", nullable: true),
                    TeamLeadId = table.Column<int>(type: "int", nullable: true),
                    CodeReviewerId = table.Column<int>(type: "int", nullable: true),
                    GitProjectReference = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ReleaseVersionFormat = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ReleaseFullIncrement = table.Column<int>(type: "int", nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    Deleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Project", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Project_Client_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Client",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Project_User_CodeReviewerId",
                        column: x => x.CodeReviewerId,
                        principalTable: "User",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Project_User_ProjectManagerId",
                        column: x => x.ProjectManagerId,
                        principalTable: "User",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Project_User_TeamLeadId",
                        column: x => x.TeamLeadId,
                        principalTable: "User",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "UserGitAccount",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    GitConnectionId = table.Column<int>(type: "int", nullable: false),
                    ExternalUserId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    TokenType = table.Column<int>(type: "int", nullable: false),
                    AccessToken = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RefreshToken = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Scopes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ExpiresOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RefreshExpiresOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ConnectedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUsedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserGitAccount", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserGitAccount_GitConnection_GitConnectionId",
                        column: x => x.GitConnectionId,
                        principalTable: "GitConnection",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_UserGitAccount_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "UserNotification",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DedupeKey = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    CreatedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReadOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserNotification", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserNotification_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "UserPassword",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PasswordSalt = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPassword", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserPassword_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "UserRoleMap",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    UserRoleId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoleMap", x => new { x.UserId, x.UserRoleId });
                    table.ForeignKey(
                        name: "FK_UserRoleMap_UserRole_UserRoleId",
                        column: x => x.UserRoleId,
                        principalTable: "UserRole",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_UserRoleMap_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CustomField",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectId = table.Column<int>(type: "int", nullable: false),
                    Label = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ResourceKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FieldType = table.Column<int>(type: "int", nullable: false),
                    Options = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Mandatory = table.Column<bool>(type: "bit", nullable: false),
                    ColumnClass = table.Column<int>(type: "int", maxLength: 20, nullable: false),
                    Placement = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomField", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomField_Project_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Project",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Module",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProjectId = table.Column<int>(type: "int", nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    Deleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Module", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Module_Project_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Project",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ProjectNotificationChannel",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectId = table.Column<int>(type: "int", nullable: false),
                    NotificationChannelId = table.Column<int>(type: "int", nullable: false),
                    Events = table.Column<int>(type: "int", nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectNotificationChannel", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectNotificationChannel_NotificationChannel_NotificationChannelId",
                        column: x => x.NotificationChannelId,
                        principalTable: "NotificationChannel",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ProjectNotificationChannel_Project_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Project",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Release",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ReleaseType = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ProjectId = table.Column<int>(type: "int", nullable: true),
                    ReleaseDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Deleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Release", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Release_Project_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Project",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Reporter",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    ProjectId = table.Column<int>(type: "int", nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    Deleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reporter", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reporter_Project_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Project",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Status",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    GroupId = table.Column<int>(type: "int", nullable: false),
                    ProjectId = table.Column<int>(type: "int", nullable: true),
                    TextColor = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    BackgroundColor = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    IconClass = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    RequiredRootCauseForBug = table.Column<bool>(type: "bit", nullable: false),
                    RequiredRootCauseForCr = table.Column<bool>(type: "bit", nullable: false),
                    RequiredResolutionForBug = table.Column<bool>(type: "bit", nullable: false),
                    RequiredResolutionForCr = table.Column<bool>(type: "bit", nullable: false),
                    RequiredMrNumberForBug = table.Column<bool>(type: "bit", nullable: false),
                    RequiredMrNumberForCr = table.Column<bool>(type: "bit", nullable: false),
                    SystemDefined = table.Column<bool>(type: "bit", nullable: false),
                    ExternalUse = table.Column<bool>(type: "bit", nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    Deleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Status", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Status_Project_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Project",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "UserProjectMap",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    ProjectId = table.Column<int>(type: "int", nullable: false),
                    CanReportToDo = table.Column<bool>(type: "bit", nullable: false),
                    CanReportCR = table.Column<bool>(type: "bit", nullable: false),
                    CanReportBug = table.Column<bool>(type: "bit", nullable: false),
                    CanEdit = table.Column<bool>(type: "bit", nullable: false),
                    CanClose = table.Column<bool>(type: "bit", nullable: false),
                    CanReOpen = table.Column<bool>(type: "bit", nullable: false),
                    CanComment = table.Column<bool>(type: "bit", nullable: false),
                    CanViewOthersTask = table.Column<bool>(type: "bit", nullable: false),
                    CanEditOthersTask = table.Column<bool>(type: "bit", nullable: false),
                    CanAddWiki = table.Column<bool>(type: "bit", nullable: false),
                    CanDeleteTask = table.Column<bool>(type: "bit", nullable: false),
                    CanChangeProject = table.Column<bool>(type: "bit", nullable: false),
                    CanReviewMergeRequests = table.Column<bool>(type: "bit", nullable: false),
                    CanMergeMergeRequests = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserProjectMap", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserProjectMap_Project_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Project",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_UserProjectMap_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "WikiCategory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    ParentCategoryId = table.Column<int>(type: "int", nullable: true),
                    ProjectId = table.Column<int>(type: "int", nullable: false),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WikiCategory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WikiCategory_Project_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Project",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_WikiCategory_WikiCategory_ParentCategoryId",
                        column: x => x.ParentCategoryId,
                        principalTable: "WikiCategory",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SubModule",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    ModuleId = table.Column<int>(type: "int", nullable: false),
                    OwnerId = table.Column<int>(type: "int", nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    Deleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubModule", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubModule_Module_ModuleId",
                        column: x => x.ModuleId,
                        principalTable: "Module",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SubModule_User_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "User",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ReleaseProject",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReleaseId = table.Column<int>(type: "int", nullable: false),
                    ProjectId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReleaseProject", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReleaseProject_Project_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Project",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ReleaseProject_Release_ReleaseId",
                        column: x => x.ReleaseId,
                        principalTable: "Release",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ProjectRepository",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectId = table.Column<int>(type: "int", nullable: false),
                    GitConnectionId = table.Column<int>(type: "int", nullable: false),
                    RepoRef = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ExternalRepoId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    DisplayName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    WebUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DefaultBranch = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    IsPrivate = table.Column<bool>(type: "bit", nullable: true),
                    IsPrimary = table.Column<bool>(type: "bit", nullable: false),
                    WebhookKey = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    WebhookSecret = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WebhookSecretCreatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    WebhookLastDeliveryOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IssueSyncMode = table.Column<int>(type: "int", nullable: false),
                    IssueSyncEnabledOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IssueImportLabel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IssueSyncTaskTypeIds = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IssueSyncComments = table.Column<bool>(type: "bit", nullable: false),
                    IssueTaskTypeId = table.Column<int>(type: "int", nullable: true),
                    IssueSeverityId = table.Column<int>(type: "int", nullable: true),
                    IssueOpenStatusId = table.Column<int>(type: "int", nullable: true),
                    IssueClosedStatusId = table.Column<int>(type: "int", nullable: true),
                    IssueSyncUserId = table.Column<int>(type: "int", nullable: true),
                    IssueImportLimitPerHour = table.Column<int>(type: "int", nullable: false),
                    IssueLastPolledOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectRepository", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectRepository_GitConnection_GitConnectionId",
                        column: x => x.GitConnectionId,
                        principalTable: "GitConnection",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ProjectRepository_Project_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Project",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ProjectRepository_Severity_IssueSeverityId",
                        column: x => x.IssueSeverityId,
                        principalTable: "Severity",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ProjectRepository_Status_IssueClosedStatusId",
                        column: x => x.IssueClosedStatusId,
                        principalTable: "Status",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ProjectRepository_Status_IssueOpenStatusId",
                        column: x => x.IssueOpenStatusId,
                        principalTable: "Status",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ProjectRepository_TaskType_IssueTaskTypeId",
                        column: x => x.IssueTaskTypeId,
                        principalTable: "TaskType",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ProjectRepository_User_IssueSyncUserId",
                        column: x => x.IssueSyncUserId,
                        principalTable: "User",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "StatusDurationMetric",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FromStatusId = table.Column<int>(type: "int", nullable: false),
                    ToStatusId = table.Column<int>(type: "int", nullable: false),
                    ProjectId = table.Column<int>(type: "int", nullable: true),
                    ShowOnTask = table.Column<bool>(type: "bit", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    Deleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StatusDurationMetric", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StatusDurationMetric_Project_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Project",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_StatusDurationMetric_Status_FromStatusId",
                        column: x => x.FromStatusId,
                        principalTable: "Status",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_StatusDurationMetric_Status_ToStatusId",
                        column: x => x.ToStatusId,
                        principalTable: "Status",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "WikiDocument",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    ContentHtml = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    ModuleId = table.Column<int>(type: "int", nullable: true),
                    SubModuleId = table.Column<int>(type: "int", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedById = table.Column<int>(type: "int", nullable: false),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedById = table.Column<int>(type: "int", nullable: false),
                    Deleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WikiDocument", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WikiDocument_Module_ModuleId",
                        column: x => x.ModuleId,
                        principalTable: "Module",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_WikiDocument_SubModule_SubModuleId",
                        column: x => x.SubModuleId,
                        principalTable: "SubModule",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_WikiDocument_User_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "User",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_WikiDocument_User_ModifiedById",
                        column: x => x.ModifiedById,
                        principalTable: "User",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_WikiDocument_WikiCategory_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "WikiCategory",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GitBranch",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectId = table.Column<int>(type: "int", nullable: false),
                    ProjectRepositoryId = table.Column<int>(type: "int", nullable: true),
                    GitProjectReference = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    BaseBranch = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Origin = table.Column<int>(type: "int", nullable: true),
                    CreatedById = table.Column<int>(type: "int", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Deleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GitBranch", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GitBranch_ProjectRepository_ProjectRepositoryId",
                        column: x => x.ProjectRepositoryId,
                        principalTable: "ProjectRepository",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_GitBranch_Project_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Project",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_GitBranch_User_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "User",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "GitWebhookDelivery",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectRepositoryId = table.Column<int>(type: "int", nullable: false),
                    DeliveryId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EventName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ReceivedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProcessedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Attempts = table.Column<int>(type: "int", nullable: false),
                    Error = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Payload = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GitWebhookDelivery", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GitWebhookDelivery_ProjectRepository_ProjectRepositoryId",
                        column: x => x.ProjectRepositoryId,
                        principalTable: "ProjectRepository",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Backlog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RootCauseAnalysis = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExternalNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EnvironmentJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ParentId = table.Column<int>(type: "int", nullable: true),
                    TaskTypeId = table.Column<int>(type: "int", nullable: false),
                    SeverityId = table.Column<int>(type: "int", nullable: false),
                    DueDate = table.Column<DateOnly>(type: "date", nullable: true),
                    PlannedStartDate = table.Column<DateOnly>(type: "date", nullable: true),
                    ProjectId = table.Column<int>(type: "int", nullable: false),
                    ModuleId = table.Column<int>(type: "int", nullable: true),
                    SubModuleId = table.Column<int>(type: "int", nullable: true),
                    ReleaseId = table.Column<int>(type: "int", nullable: true),
                    ReporterId = table.Column<int>(type: "int", nullable: true),
                    AssigneeId = table.Column<int>(type: "int", nullable: true),
                    TesterId = table.Column<int>(type: "int", nullable: true),
                    ReOpenCount = table.Column<int>(type: "int", nullable: false),
                    SubTaskCount = table.Column<int>(type: "int", nullable: false),
                    AttachmentCount = table.Column<int>(type: "int", nullable: true),
                    CommentCount = table.Column<int>(type: "int", nullable: true),
                    StatusId = table.Column<int>(type: "int", nullable: false),
                    ResolutionDetails = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MergeRequestId = table.Column<int>(type: "int", nullable: true),
                    MergeRequestRepositoryId = table.Column<int>(type: "int", nullable: true),
                    SourceId = table.Column<int>(type: "int", nullable: true),
                    SourceWikiDocumentId = table.Column<int>(type: "int", nullable: true),
                    CreatedById = table.Column<int>(type: "int", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedById = table.Column<int>(type: "int", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Backlog", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Backlog_Backlog_ParentId",
                        column: x => x.ParentId,
                        principalTable: "Backlog",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Backlog_Module_ModuleId",
                        column: x => x.ModuleId,
                        principalTable: "Module",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Backlog_ProjectRepository_MergeRequestRepositoryId",
                        column: x => x.MergeRequestRepositoryId,
                        principalTable: "ProjectRepository",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Backlog_Project_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Project",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Backlog_Release_ReleaseId",
                        column: x => x.ReleaseId,
                        principalTable: "Release",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Backlog_Reporter_ReporterId",
                        column: x => x.ReporterId,
                        principalTable: "Reporter",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Backlog_Severity_SeverityId",
                        column: x => x.SeverityId,
                        principalTable: "Severity",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Backlog_Status_StatusId",
                        column: x => x.StatusId,
                        principalTable: "Status",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Backlog_SubModule_SubModuleId",
                        column: x => x.SubModuleId,
                        principalTable: "SubModule",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Backlog_TaskType_TaskTypeId",
                        column: x => x.TaskTypeId,
                        principalTable: "TaskType",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Backlog_User_AssigneeId",
                        column: x => x.AssigneeId,
                        principalTable: "User",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Backlog_User_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "User",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Backlog_User_ModifiedById",
                        column: x => x.ModifiedById,
                        principalTable: "User",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Backlog_User_TesterId",
                        column: x => x.TesterId,
                        principalTable: "User",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Backlog_WikiDocument_SourceWikiDocumentId",
                        column: x => x.SourceWikiDocumentId,
                        principalTable: "WikiDocument",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "WikiDocumentVersion",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WikiDocumentId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    ContentHtml = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedById = table.Column<int>(type: "int", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WikiDocumentVersion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WikiDocumentVersion_User_ModifiedById",
                        column: x => x.ModifiedById,
                        principalTable: "User",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_WikiDocumentVersion_WikiDocument_WikiDocumentId",
                        column: x => x.WikiDocumentId,
                        principalTable: "WikiDocument",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "BacklogComment",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BacklogId = table.Column<int>(type: "int", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedById = table.Column<int>(type: "int", nullable: false),
                    SystemComment = table.Column<bool>(type: "bit", nullable: false),
                    PostToIssue = table.Column<bool>(type: "bit", nullable: false),
                    FieldName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    OldValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NewValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedById = table.Column<int>(type: "int", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BacklogComment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BacklogComment_Backlog_BacklogId",
                        column: x => x.BacklogId,
                        principalTable: "Backlog",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BacklogComment_User_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "User",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BacklogComment_User_ModifiedById",
                        column: x => x.ModifiedById,
                        principalTable: "User",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "BacklogCustomFieldValue",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BacklogId = table.Column<int>(type: "int", nullable: false),
                    CustomFieldId = table.Column<int>(type: "int", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BacklogCustomFieldValue", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BacklogCustomFieldValue_Backlog_BacklogId",
                        column: x => x.BacklogId,
                        principalTable: "Backlog",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BacklogCustomFieldValue_CustomField_CustomFieldId",
                        column: x => x.CustomFieldId,
                        principalTable: "CustomField",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "BacklogDocument",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BacklogId = table.Column<int>(type: "int", nullable: false),
                    DocumentId = table.Column<int>(type: "int", nullable: false),
                    CreatedById = table.Column<int>(type: "int", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BacklogDocument", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BacklogDocument_Backlog_BacklogId",
                        column: x => x.BacklogId,
                        principalTable: "Backlog",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BacklogDocument_Document_DocumentId",
                        column: x => x.DocumentId,
                        principalTable: "Document",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BacklogDocument_User_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "User",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "BacklogStatusLog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BacklogId = table.Column<int>(type: "int", nullable: false),
                    StatusId = table.Column<int>(type: "int", nullable: false),
                    CreatedById = table.Column<int>(type: "int", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BacklogStatusLog", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BacklogStatusLog_Backlog_BacklogId",
                        column: x => x.BacklogId,
                        principalTable: "Backlog",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BacklogStatusLog_Status_StatusId",
                        column: x => x.StatusId,
                        principalTable: "Status",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BacklogStatusLog_User_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "User",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "BacklogTag",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BacklogId = table.Column<int>(type: "int", nullable: false),
                    TagId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BacklogTag", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BacklogTag_Backlog_BacklogId",
                        column: x => x.BacklogId,
                        principalTable: "Backlog",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BacklogTag_Tag_TagId",
                        column: x => x.TagId,
                        principalTable: "Tag",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ExternalIssueLink",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BacklogId = table.Column<int>(type: "int", nullable: false),
                    ProjectRepositoryId = table.Column<int>(type: "int", nullable: false),
                    ExternalId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Number = table.Column<int>(type: "int", nullable: false),
                    Url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RemoteState = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Snapshot = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SyncEnabled = table.Column<bool>(type: "bit", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastSyncedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RemoteUpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastError = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExternalIssueLink", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExternalIssueLink_Backlog_BacklogId",
                        column: x => x.BacklogId,
                        principalTable: "Backlog",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ExternalIssueLink_ProjectRepository_ProjectRepositoryId",
                        column: x => x.ProjectRepositoryId,
                        principalTable: "ProjectRepository",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "GitBranchTask",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GitBranchId = table.Column<int>(type: "int", nullable: false),
                    BacklogId = table.Column<int>(type: "int", nullable: false),
                    LinkedById = table.Column<int>(type: "int", nullable: false),
                    LinkedOn = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GitBranchTask", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GitBranchTask_Backlog_BacklogId",
                        column: x => x.BacklogId,
                        principalTable: "Backlog",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_GitBranchTask_GitBranch_GitBranchId",
                        column: x => x.GitBranchId,
                        principalTable: "GitBranch",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_GitBranchTask_User_LinkedById",
                        column: x => x.LinkedById,
                        principalTable: "User",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ExternalCommentLink",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExternalIssueLinkId = table.Column<int>(type: "int", nullable: false),
                    BacklogCommentId = table.Column<int>(type: "int", nullable: false),
                    ExternalCommentId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Inbound = table.Column<bool>(type: "bit", nullable: false),
                    ContentHash = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    LastSyncedOn = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExternalCommentLink", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExternalCommentLink_BacklogComment_BacklogCommentId",
                        column: x => x.BacklogCommentId,
                        principalTable: "BacklogComment",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ExternalCommentLink_ExternalIssueLink_ExternalIssueLinkId",
                        column: x => x.ExternalIssueLinkId,
                        principalTable: "ExternalIssueLink",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Announcement_Active_Deleted",
                table: "Announcement",
                columns: new[] { "Active", "Deleted" });

            migrationBuilder.CreateIndex(
                name: "IX_AnnouncementAcknowledgement_AnnouncementId_UserId",
                table: "AnnouncementAcknowledgement",
                columns: new[] { "AnnouncementId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AnnouncementAcknowledgement_UserId",
                table: "AnnouncementAcknowledgement",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AnnouncementRole_AnnouncementId_UserRoleId",
                table: "AnnouncementRole",
                columns: new[] { "AnnouncementId", "UserRoleId" });

            migrationBuilder.CreateIndex(
                name: "IX_AnnouncementRole_UserRoleId",
                table: "AnnouncementRole",
                column: "UserRoleId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLog_EntityName_EntityId",
                table: "AuditLog",
                columns: new[] { "EntityName", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_Backlog_AssigneeId",
                table: "Backlog",
                column: "AssigneeId");

            migrationBuilder.CreateIndex(
                name: "IX_Backlog_CreatedById",
                table: "Backlog",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Backlog_MergeRequestRepositoryId",
                table: "Backlog",
                column: "MergeRequestRepositoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Backlog_ModifiedById",
                table: "Backlog",
                column: "ModifiedById");

            migrationBuilder.CreateIndex(
                name: "IX_Backlog_ModuleId",
                table: "Backlog",
                column: "ModuleId");

            migrationBuilder.CreateIndex(
                name: "IX_Backlog_ParentId",
                table: "Backlog",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_Backlog_ProjectId",
                table: "Backlog",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Backlog_ReleaseId",
                table: "Backlog",
                column: "ReleaseId");

            migrationBuilder.CreateIndex(
                name: "IX_Backlog_ReporterId",
                table: "Backlog",
                column: "ReporterId");

            migrationBuilder.CreateIndex(
                name: "IX_Backlog_SeverityId",
                table: "Backlog",
                column: "SeverityId");

            migrationBuilder.CreateIndex(
                name: "IX_Backlog_SourceWikiDocumentId",
                table: "Backlog",
                column: "SourceWikiDocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_Backlog_StatusId",
                table: "Backlog",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_Backlog_SubModuleId",
                table: "Backlog",
                column: "SubModuleId");

            migrationBuilder.CreateIndex(
                name: "IX_Backlog_TaskTypeId",
                table: "Backlog",
                column: "TaskTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Backlog_TesterId",
                table: "Backlog",
                column: "TesterId");

            migrationBuilder.CreateIndex(
                name: "IX_BacklogComment_BacklogId",
                table: "BacklogComment",
                column: "BacklogId");

            migrationBuilder.CreateIndex(
                name: "IX_BacklogComment_CreatedById",
                table: "BacklogComment",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_BacklogComment_ModifiedById",
                table: "BacklogComment",
                column: "ModifiedById");

            migrationBuilder.CreateIndex(
                name: "IX_BacklogCustomFieldValue_BacklogId",
                table: "BacklogCustomFieldValue",
                column: "BacklogId");

            migrationBuilder.CreateIndex(
                name: "IX_BacklogCustomFieldValue_CustomFieldId",
                table: "BacklogCustomFieldValue",
                column: "CustomFieldId");

            migrationBuilder.CreateIndex(
                name: "IX_BacklogDocument_BacklogId",
                table: "BacklogDocument",
                column: "BacklogId");

            migrationBuilder.CreateIndex(
                name: "IX_BacklogDocument_CreatedById",
                table: "BacklogDocument",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_BacklogDocument_DocumentId",
                table: "BacklogDocument",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_BacklogStatusLog_BacklogId",
                table: "BacklogStatusLog",
                column: "BacklogId");

            migrationBuilder.CreateIndex(
                name: "IX_BacklogStatusLog_CreatedById",
                table: "BacklogStatusLog",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_BacklogStatusLog_StatusId",
                table: "BacklogStatusLog",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_BacklogTag_BacklogId_TagId",
                table: "BacklogTag",
                columns: new[] { "BacklogId", "TagId" });

            migrationBuilder.CreateIndex(
                name: "IX_BacklogTag_TagId",
                table: "BacklogTag",
                column: "TagId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomField_ProjectId",
                table: "CustomField",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailTemplate_EmailAccountId",
                table: "EmailTemplate",
                column: "EmailAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_ExternalCommentLink_BacklogCommentId",
                table: "ExternalCommentLink",
                column: "BacklogCommentId");

            migrationBuilder.CreateIndex(
                name: "IX_ExternalCommentLink_ExternalIssueLinkId_ExternalCommentId",
                table: "ExternalCommentLink",
                columns: new[] { "ExternalIssueLinkId", "ExternalCommentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExternalIssueLink_BacklogId",
                table: "ExternalIssueLink",
                column: "BacklogId");

            migrationBuilder.CreateIndex(
                name: "IX_ExternalIssueLink_ProjectRepositoryId_Number",
                table: "ExternalIssueLink",
                columns: new[] { "ProjectRepositoryId", "Number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GitBranch_CreatedById",
                table: "GitBranch",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_GitBranch_GitProjectReference_Name",
                table: "GitBranch",
                columns: new[] { "GitProjectReference", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_GitBranch_ProjectId",
                table: "GitBranch",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_GitBranch_ProjectRepositoryId",
                table: "GitBranch",
                column: "ProjectRepositoryId");

            migrationBuilder.CreateIndex(
                name: "IX_GitBranchTask_BacklogId",
                table: "GitBranchTask",
                column: "BacklogId");

            migrationBuilder.CreateIndex(
                name: "IX_GitBranchTask_GitBranchId_BacklogId",
                table: "GitBranchTask",
                columns: new[] { "GitBranchId", "BacklogId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GitBranchTask_LinkedById",
                table: "GitBranchTask",
                column: "LinkedById");

            migrationBuilder.CreateIndex(
                name: "IX_GitWebhookDelivery_ProjectRepositoryId_DeliveryId",
                table: "GitWebhookDelivery",
                columns: new[] { "ProjectRepositoryId", "DeliveryId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GitWebhookDelivery_ReceivedOn",
                table: "GitWebhookDelivery",
                column: "ReceivedOn");

            migrationBuilder.CreateIndex(
                name: "IX_LocaleResource_LanguageId",
                table: "LocaleResource",
                column: "LanguageId");

            migrationBuilder.CreateIndex(
                name: "IX_Module_ProjectId",
                table: "Module",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Project_ClientId",
                table: "Project",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_Project_CodeReviewerId",
                table: "Project",
                column: "CodeReviewerId");

            migrationBuilder.CreateIndex(
                name: "IX_Project_ProjectManagerId",
                table: "Project",
                column: "ProjectManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_Project_TeamLeadId",
                table: "Project",
                column: "TeamLeadId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectNotificationChannel_NotificationChannelId",
                table: "ProjectNotificationChannel",
                column: "NotificationChannelId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectNotificationChannel_ProjectId_NotificationChannelId",
                table: "ProjectNotificationChannel",
                columns: new[] { "ProjectId", "NotificationChannelId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectRepository_GitConnectionId",
                table: "ProjectRepository",
                column: "GitConnectionId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectRepository_IssueClosedStatusId",
                table: "ProjectRepository",
                column: "IssueClosedStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectRepository_IssueOpenStatusId",
                table: "ProjectRepository",
                column: "IssueOpenStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectRepository_IssueSeverityId",
                table: "ProjectRepository",
                column: "IssueSeverityId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectRepository_IssueSyncUserId",
                table: "ProjectRepository",
                column: "IssueSyncUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectRepository_IssueTaskTypeId",
                table: "ProjectRepository",
                column: "IssueTaskTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectRepository_ProjectId_GitConnectionId_RepoRef",
                table: "ProjectRepository",
                columns: new[] { "ProjectId", "GitConnectionId", "RepoRef" });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectRepository_WebhookKey",
                table: "ProjectRepository",
                column: "WebhookKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Release_ProjectId",
                table: "Release",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_ReleaseProject_ProjectId",
                table: "ReleaseProject",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_ReleaseProject_ReleaseId_ProjectId",
                table: "ReleaseProject",
                columns: new[] { "ReleaseId", "ProjectId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reporter_ProjectId",
                table: "Reporter",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Status_ProjectId",
                table: "Status",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_StatusDurationMetric_FromStatusId",
                table: "StatusDurationMetric",
                column: "FromStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_StatusDurationMetric_ProjectId",
                table: "StatusDurationMetric",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_StatusDurationMetric_ToStatusId",
                table: "StatusDurationMetric",
                column: "ToStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_SubModule_ModuleId",
                table: "SubModule",
                column: "ModuleId");

            migrationBuilder.CreateIndex(
                name: "IX_SubModule_OwnerId",
                table: "SubModule",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_User_AddressId",
                table: "User",
                column: "AddressId");

            migrationBuilder.CreateIndex(
                name: "IX_User_LanguageId",
                table: "User",
                column: "LanguageId");

            migrationBuilder.CreateIndex(
                name: "IX_UserGitAccount_GitConnectionId_ExternalUserId",
                table: "UserGitAccount",
                columns: new[] { "GitConnectionId", "ExternalUserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserGitAccount_UserId_GitConnectionId",
                table: "UserGitAccount",
                columns: new[] { "UserId", "GitConnectionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserNotification_UserId_DedupeKey",
                table: "UserNotification",
                columns: new[] { "UserId", "DedupeKey" });

            migrationBuilder.CreateIndex(
                name: "IX_UserNotification_UserId_IsRead_CreatedOnUtc",
                table: "UserNotification",
                columns: new[] { "UserId", "IsRead", "CreatedOnUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_UserPassword_UserId",
                table: "UserPassword",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserProjectMap_ProjectId",
                table: "UserProjectMap",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_UserProjectMap_UserId",
                table: "UserProjectMap",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoleMap_UserRoleId",
                table: "UserRoleMap",
                column: "UserRoleId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRolePermissionMap_UserRoleId",
                table: "UserRolePermissionMap",
                column: "UserRoleId");

            migrationBuilder.CreateIndex(
                name: "IX_WikiCategory_ParentCategoryId",
                table: "WikiCategory",
                column: "ParentCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_WikiCategory_ProjectId",
                table: "WikiCategory",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_WikiDocument_CategoryId",
                table: "WikiDocument",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_WikiDocument_CreatedById",
                table: "WikiDocument",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_WikiDocument_ModifiedById",
                table: "WikiDocument",
                column: "ModifiedById");

            migrationBuilder.CreateIndex(
                name: "IX_WikiDocument_ModuleId",
                table: "WikiDocument",
                column: "ModuleId");

            migrationBuilder.CreateIndex(
                name: "IX_WikiDocument_SubModuleId",
                table: "WikiDocument",
                column: "SubModuleId");

            migrationBuilder.CreateIndex(
                name: "IX_WikiDocumentVersion_ModifiedById",
                table: "WikiDocumentVersion",
                column: "ModifiedById");

            migrationBuilder.CreateIndex(
                name: "IX_WikiDocumentVersion_WikiDocumentId",
                table: "WikiDocumentVersion",
                column: "WikiDocumentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActivityLog");

            migrationBuilder.DropTable(
                name: "AnnouncementAcknowledgement");

            migrationBuilder.DropTable(
                name: "AnnouncementRole");

            migrationBuilder.DropTable(
                name: "AuditLog");

            migrationBuilder.DropTable(
                name: "BacklogCustomFieldValue");

            migrationBuilder.DropTable(
                name: "BacklogDocument");

            migrationBuilder.DropTable(
                name: "BacklogStatusLog");

            migrationBuilder.DropTable(
                name: "BacklogTag");

            migrationBuilder.DropTable(
                name: "EmailTemplate");

            migrationBuilder.DropTable(
                name: "ExternalCommentLink");

            migrationBuilder.DropTable(
                name: "GenericAttribute");

            migrationBuilder.DropTable(
                name: "GitBranchTask");

            migrationBuilder.DropTable(
                name: "GitWebhookDelivery");

            migrationBuilder.DropTable(
                name: "LocaleResource");

            migrationBuilder.DropTable(
                name: "Log");

            migrationBuilder.DropTable(
                name: "Menu");

            migrationBuilder.DropTable(
                name: "ProjectNotificationChannel");

            migrationBuilder.DropTable(
                name: "ReleaseProject");

            migrationBuilder.DropTable(
                name: "Setting");

            migrationBuilder.DropTable(
                name: "StatusDurationMetric");

            migrationBuilder.DropTable(
                name: "UserGitAccount");

            migrationBuilder.DropTable(
                name: "UserNotification");

            migrationBuilder.DropTable(
                name: "UserPassword");

            migrationBuilder.DropTable(
                name: "UserProjectMap");

            migrationBuilder.DropTable(
                name: "UserRoleMap");

            migrationBuilder.DropTable(
                name: "UserRolePermissionMap");

            migrationBuilder.DropTable(
                name: "WikiDocumentVersion");

            migrationBuilder.DropTable(
                name: "Announcement");

            migrationBuilder.DropTable(
                name: "CustomField");

            migrationBuilder.DropTable(
                name: "Document");

            migrationBuilder.DropTable(
                name: "Tag");

            migrationBuilder.DropTable(
                name: "EmailAccount");

            migrationBuilder.DropTable(
                name: "BacklogComment");

            migrationBuilder.DropTable(
                name: "ExternalIssueLink");

            migrationBuilder.DropTable(
                name: "GitBranch");

            migrationBuilder.DropTable(
                name: "NotificationChannel");

            migrationBuilder.DropTable(
                name: "UserRolePermission");

            migrationBuilder.DropTable(
                name: "UserRole");

            migrationBuilder.DropTable(
                name: "Backlog");

            migrationBuilder.DropTable(
                name: "ProjectRepository");

            migrationBuilder.DropTable(
                name: "Release");

            migrationBuilder.DropTable(
                name: "Reporter");

            migrationBuilder.DropTable(
                name: "WikiDocument");

            migrationBuilder.DropTable(
                name: "GitConnection");

            migrationBuilder.DropTable(
                name: "Severity");

            migrationBuilder.DropTable(
                name: "Status");

            migrationBuilder.DropTable(
                name: "TaskType");

            migrationBuilder.DropTable(
                name: "SubModule");

            migrationBuilder.DropTable(
                name: "WikiCategory");

            migrationBuilder.DropTable(
                name: "Module");

            migrationBuilder.DropTable(
                name: "Project");

            migrationBuilder.DropTable(
                name: "Client");

            migrationBuilder.DropTable(
                name: "User");

            migrationBuilder.DropTable(
                name: "Address");

            migrationBuilder.DropTable(
                name: "Language");
        }
    }
}
