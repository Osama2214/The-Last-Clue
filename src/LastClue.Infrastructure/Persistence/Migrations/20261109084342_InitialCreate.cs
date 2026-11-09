using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LastClue.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    AvatarSeed = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    IsBlocked = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CaseTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CaseKey = table.Column<string>(type: "varchar(60)", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CaseTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AdminAudit",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ActorId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Action = table.Column<string>(type: "varchar(60)", nullable: false),
                    EntityType = table.Column<string>(type: "varchar(40)", nullable: false),
                    EntityId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    OccurredAt = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false),
                    SafeDetailsJson = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdminAudit", x => x.Id);
                    table.CheckConstraint("CK_AdminAudit_SafeDetailsJson", "ISJSON([SafeDetailsJson]) = 1");
                    table.ForeignKey(
                        name: "FK_AdminAudit_AspNetUsers_ActorId",
                        column: x => x.ActorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId })
                        .Annotation("SqlServer:Clustered", false);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name })
                        .Annotation("SqlServer:Clustered", false);
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Rooms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "varchar(12)", nullable: false),
                    HostUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    Language = table.Column<string>(type: "char(2)", nullable: false),
                    Status = table.Column<string>(type: "varchar(16)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false),
                    StateVersion = table.Column<long>(type: "bigint", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rooms", x => x.Id);
                    table.CheckConstraint("CK_Rooms_Language", "[Language] IN ('en', 'ar')");
                    table.CheckConstraint("CK_Rooms_Status", "[Status] IN ('Lobby', 'Starting', 'Active', 'Closed')");
                    table.ForeignKey(
                        name: "FK_Rooms_AspNetUsers_HostUserId",
                        column: x => x.HostUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SettingsVersions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VersionNo = table.Column<int>(type: "int", nullable: false),
                    ValuesJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PublishedAt = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false),
                    ChangedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SettingsVersions", x => x.Id);
                    table.CheckConstraint("CK_SettingsVersions_ValuesJson", "ISJSON([ValuesJson]) = 1");
                    table.ForeignKey(
                        name: "FK_SettingsVersions_AspNetUsers_ChangedBy",
                        column: x => x.ChangedBy,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CaseVersions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TemplateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VersionNo = table.Column<int>(type: "int", nullable: false),
                    ReviewStatus = table.Column<string>(type: "varchar(16)", nullable: false),
                    ReviewedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CaseVersions", x => x.Id);
                    table.CheckConstraint("CK_CaseVersions_ReviewStatus", "[ReviewStatus] IN ('Draft', 'Reviewed', 'Published', 'Retired')");
                    table.ForeignKey(
                        name: "FK_CaseVersions_AspNetUsers_ReviewedBy",
                        column: x => x.ReviewedBy,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CaseVersions_CaseTemplates_TemplateId",
                        column: x => x.TemplateId,
                        principalTable: "CaseTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RoomMembers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoomId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    JoinedAt = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false),
                    LeftAt = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true),
                    IsReady = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoomMembers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoomMembers_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RoomMembers_Rooms_RoomId",
                        column: x => x.RoomId,
                        principalTable: "Rooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AchievementDefinitions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "varchar(40)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    CriteriaJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SettingsVersionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AchievementDefinitions", x => x.Id);
                    table.CheckConstraint("CK_AchievementDefinitions_CriteriaJson", "ISJSON([CriteriaJson]) = 1");
                    table.ForeignKey(
                        name: "FK_AchievementDefinitions_SettingsVersions_SettingsVersionId",
                        column: x => x.SettingsVersionId,
                        principalTable: "SettingsVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Games",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoomId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Number = table.Column<int>(type: "int", nullable: false),
                    Round = table.Column<byte>(type: "tinyint", nullable: false),
                    Phase = table.Column<string>(type: "varchar(20)", nullable: false),
                    PhaseEndsAt = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true),
                    PauseStartedAt = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true),
                    AccusationsUsed = table.Column<byte>(type: "tinyint", nullable: false),
                    SettingsVersionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SettingsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StateVersion = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true),
                    Outcome = table.Column<string>(type: "varchar(24)", nullable: true),
                    CancellationReason = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    Status = table.Column<string>(type: "varchar(16)", nullable: false),
                    IsRunning = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Games", x => x.Id);
                    table.CheckConstraint("CK_Games_Outcome", "[Outcome] IN ('DetectivesWin', 'CriminalWin')");
                    table.CheckConstraint("CK_Games_Phase", "[Phase] IN ('Briefing', 'Statements', 'Questioning', 'ClueRelease', 'Accusation')");
                    table.CheckConstraint("CK_Games_RunningStatus", "([IsRunning] = 1 AND [Status] IN ('Starting', 'Active')) OR ([IsRunning] = 0 AND [Status] IN ('Completed', 'Cancelled'))");
                    table.CheckConstraint("CK_Games_SettingsJson", "ISJSON([SettingsJson]) = 1");
                    table.CheckConstraint("CK_Games_Status", "[Status] IN ('Starting', 'Active', 'Completed', 'Cancelled')");
                    table.ForeignKey(
                        name: "FK_Games_Rooms_RoomId",
                        column: x => x.RoomId,
                        principalTable: "Rooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Games_SettingsVersions_SettingsVersionId",
                        column: x => x.SettingsVersionId,
                        principalTable: "SettingsVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RankDefinitions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "varchar(40)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    MinPoints = table.Column<int>(type: "int", nullable: false),
                    SettingsVersionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RankDefinitions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RankDefinitions_SettingsVersions_SettingsVersionId",
                        column: x => x.SettingsVersionId,
                        principalTable: "SettingsVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CaseVariants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CaseVersionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PlayerCount = table.Column<byte>(type: "tinyint", nullable: false),
                    Language = table.Column<string>(type: "char(2)", nullable: false),
                    PayloadJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsValidated = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CaseVariants", x => x.Id);
                    table.CheckConstraint("CK_CaseVariants_Language", "[Language] IN ('en', 'ar')");
                    table.CheckConstraint("CK_CaseVariants_PayloadJson", "ISJSON([PayloadJson]) = 1");
                    table.CheckConstraint("CK_CaseVariants_PlayerCount", "[PlayerCount] BETWEEN 4 AND 8");
                    table.ForeignKey(
                        name: "FK_CaseVariants_CaseVersions_CaseVersionId",
                        column: x => x.CaseVersionId,
                        principalTable: "CaseVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ActionReceipts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    GameId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PayloadHash = table.Column<string>(type: "char(64)", nullable: false),
                    ResponseJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActionReceipts", x => x.Id);
                    table.CheckConstraint("CK_ActionReceipts_ResponseJson", "ISJSON([ResponseJson]) = 1");
                    table.ForeignKey(
                        name: "FK_ActionReceipts_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ActionReceipts_Games_GameId",
                        column: x => x.GameId,
                        principalTable: "Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GameClues",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GameId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClueKey = table.Column<string>(type: "varchar(60)", nullable: false),
                    Round = table.Column<byte>(type: "tinyint", nullable: false),
                    Audience = table.Column<string>(type: "varchar(12)", nullable: false),
                    CharacterKey = table.Column<string>(type: "varchar(40)", nullable: true),
                    Body = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    ReleasedAt = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameClues", x => x.Id);
                    table.UniqueConstraint("AK_GameClues_GameId_Id", x => new { x.GameId, x.Id });
                    table.CheckConstraint("CK_GameClues_Audience", "[Audience] IN ('Public', 'Detectives', 'Character')");
                    table.CheckConstraint("CK_GameClues_Round", "[Round] BETWEEN 1 AND 3");
                    table.ForeignKey(
                        name: "FK_GameClues_Games_GameId",
                        column: x => x.GameId,
                        principalTable: "Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GamePlayers",
                columns: table => new
                {
                    GameId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Role = table.Column<string>(type: "varchar(12)", nullable: false),
                    CharacterKey = table.Column<string>(type: "varchar(40)", nullable: true),
                    PrivateScript = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsArrested = table.Column<bool>(type: "bit", nullable: false),
                    GraceUsed = table.Column<bool>(type: "bit", nullable: false),
                    GraceUntil = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true),
                    DepartedAt = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GamePlayers", x => new { x.GameId, x.UserId })
                        .Annotation("SqlServer:Clustered", false);
                    table.CheckConstraint("CK_GamePlayers_Role", "[Role] IN ('Detective', 'Criminal', 'Innocent')");
                    table.ForeignKey(
                        name: "FK_GamePlayers_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GamePlayers_Games_GameId",
                        column: x => x.GameId,
                        principalTable: "Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GenerationJobs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GameId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<string>(type: "varchar(16)", nullable: false),
                    DeadlineAt = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false),
                    LeaseUntil = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true),
                    Attempts = table.Column<int>(type: "int", nullable: false),
                    LastErrorCode = table.Column<string>(type: "varchar(40)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GenerationJobs", x => x.Id);
                    table.CheckConstraint("CK_GenerationJobs_Status", "[Status] IN ('Pending', 'Running', 'Succeeded', 'Failed')");
                    table.ForeignKey(
                        name: "FK_GenerationJobs_Games_GameId",
                        column: x => x.GameId,
                        principalTable: "Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GameCases",
                columns: table => new
                {
                    GameId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CaseVariantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Source = table.Column<string>(type: "varchar(12)", nullable: false),
                    PayloadJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SnapshotHash = table.Column<string>(type: "char(64)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameCases", x => x.GameId);
                    table.CheckConstraint("CK_GameCases_PayloadJson", "ISJSON([PayloadJson]) = 1");
                    table.CheckConstraint("CK_GameCases_SourceVariant", "([Source] = 'AI' AND [CaseVariantId] IS NULL) OR ([Source] = 'Fallback' AND [CaseVariantId] IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_GameCases_CaseVariants_CaseVariantId",
                        column: x => x.CaseVariantId,
                        principalTable: "CaseVariants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GameCases_Games_GameId",
                        column: x => x.GameId,
                        principalTable: "Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AccusationVotes",
                columns: table => new
                {
                    GameId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Round = table.Column<byte>(type: "tinyint", nullable: false),
                    DetectiveId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    TargetId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccusationVotes", x => new { x.GameId, x.Round, x.DetectiveId })
                        .Annotation("SqlServer:Clustered", false);
                    table.CheckConstraint("CK_AccusationVotes_Round", "[Round] BETWEEN 1 AND 3");
                    table.ForeignKey(
                        name: "FK_AccusationVotes_GamePlayers_GameId_DetectiveId",
                        columns: x => new { x.GameId, x.DetectiveId },
                        principalTable: "GamePlayers",
                        principalColumns: new[] { "GameId", "UserId" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AccusationVotes_GamePlayers_GameId_TargetId",
                        columns: x => new { x.GameId, x.TargetId },
                        principalTable: "GamePlayers",
                        principalColumns: new[] { "GameId", "UserId" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AccusationVotes_Games_GameId",
                        column: x => x.GameId,
                        principalTable: "Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Arrests",
                columns: table => new
                {
                    GameId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Round = table.Column<byte>(type: "tinyint", nullable: false),
                    TargetId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    IsCriminal = table.Column<bool>(type: "bit", nullable: false),
                    ArrestedAt = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Arrests", x => new { x.GameId, x.Round });
                    table.CheckConstraint("CK_Arrests_Round", "[Round] BETWEEN 1 AND 3");
                    table.ForeignKey(
                        name: "FK_Arrests_GamePlayers_GameId_TargetId",
                        columns: x => new { x.GameId, x.TargetId },
                        principalTable: "GamePlayers",
                        principalColumns: new[] { "GameId", "UserId" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Arrests_Games_GameId",
                        column: x => x.GameId,
                        principalTable: "Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ClueRecipients",
                columns: table => new
                {
                    GameId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClueId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    AvailableAt = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClueRecipients", x => new { x.GameId, x.ClueId, x.UserId })
                        .Annotation("SqlServer:Clustered", false);
                    table.ForeignKey(
                        name: "FK_ClueRecipients_GameClues_GameId_ClueId",
                        columns: x => new { x.GameId, x.ClueId },
                        principalTable: "GameClues",
                        principalColumns: new[] { "GameId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClueRecipients_GamePlayers_GameId_UserId",
                        columns: x => new { x.GameId, x.UserId },
                        principalTable: "GamePlayers",
                        principalColumns: new[] { "GameId", "UserId" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OutboxMessages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoomId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    GameId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    StateVersion = table.Column<long>(type: "bigint", nullable: false),
                    Audience = table.Column<string>(type: "varchar(16)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    EventType = table.Column<string>(type: "varchar(40)", nullable: false),
                    PayloadJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false),
                    DispatchedAt = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutboxMessages", x => x.Id);
                    table.CheckConstraint("CK_OutboxMessages_PayloadJson", "ISJSON([PayloadJson]) = 1");
                    table.CheckConstraint("CK_OutboxMessages_Target", "([RoomId] IS NOT NULL AND [GameId] IS NULL) OR ([RoomId] IS NULL AND [GameId] IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_OutboxMessages_GamePlayers_GameId_UserId",
                        columns: x => new { x.GameId, x.UserId },
                        principalTable: "GamePlayers",
                        principalColumns: new[] { "GameId", "UserId" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OutboxMessages_Games_GameId",
                        column: x => x.GameId,
                        principalTable: "Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OutboxMessages_Rooms_RoomId",
                        column: x => x.RoomId,
                        principalTable: "Rooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PendingScoreEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GameId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    EventKey = table.Column<string>(type: "varchar(80)", nullable: false),
                    Delta = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PendingScoreEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PendingScoreEvents_GamePlayers_GameId_UserId",
                        columns: x => new { x.GameId, x.UserId },
                        principalTable: "GamePlayers",
                        principalColumns: new[] { "GameId", "UserId" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PlayerAchievements",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    AchievementCode = table.Column<string>(type: "varchar(40)", nullable: false),
                    DefinitionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GameId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AwardedAt = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerAchievements", x => new { x.UserId, x.AchievementCode })
                        .Annotation("SqlServer:Clustered", false);
                    table.ForeignKey(
                        name: "FK_PlayerAchievements_AchievementDefinitions_DefinitionId",
                        column: x => x.DefinitionId,
                        principalTable: "AchievementDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PlayerAchievements_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PlayerAchievements_GamePlayers_GameId_UserId",
                        columns: x => new { x.GameId, x.UserId },
                        principalTable: "GamePlayers",
                        principalColumns: new[] { "GameId", "UserId" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PlayerConnections",
                columns: table => new
                {
                    ConnectionId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    GameId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    LastSeenAt = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false),
                    ClosedAt = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerConnections", x => x.ConnectionId);
                    table.ForeignKey(
                        name: "FK_PlayerConnections_GamePlayers_GameId_UserId",
                        columns: x => new { x.GameId, x.UserId },
                        principalTable: "GamePlayers",
                        principalColumns: new[] { "GameId", "UserId" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Questions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GameId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Round = table.Column<byte>(type: "tinyint", nullable: false),
                    DetectiveId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Slot = table.Column<byte>(type: "tinyint", nullable: false),
                    TargetId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Text = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    AnswerText = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    AskedAt = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false),
                    AnsweredAt = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Questions", x => x.Id);
                    table.CheckConstraint("CK_Questions_Round", "[Round] BETWEEN 1 AND 3");
                    table.CheckConstraint("CK_Questions_Slot", "[Slot] IN (1, 2)");
                    table.ForeignKey(
                        name: "FK_Questions_GamePlayers_GameId_DetectiveId",
                        columns: x => new { x.GameId, x.DetectiveId },
                        principalTable: "GamePlayers",
                        principalColumns: new[] { "GameId", "UserId" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Questions_GamePlayers_GameId_TargetId",
                        columns: x => new { x.GameId, x.TargetId },
                        principalTable: "GamePlayers",
                        principalColumns: new[] { "GameId", "UserId" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Questions_Games_GameId",
                        column: x => x.GameId,
                        principalTable: "Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ScoreEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GameId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    EventKey = table.Column<string>(type: "varchar(80)", nullable: false),
                    Delta = table.Column<int>(type: "int", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScoreEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScoreEntries_GamePlayers_GameId_UserId",
                        columns: x => new { x.GameId, x.UserId },
                        principalTable: "GamePlayers",
                        principalColumns: new[] { "GameId", "UserId" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Statements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GameId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Round = table.Column<byte>(type: "tinyint", nullable: false),
                    AuthorId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Text = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Statements", x => x.Id);
                    table.CheckConstraint("CK_Statements_Round", "[Round] BETWEEN 1 AND 3");
                    table.CheckConstraint("CK_Statements_Text", "LEN(TRIM([Text])) > 0");
                    table.ForeignKey(
                        name: "FK_Statements_GamePlayers_GameId_AuthorId",
                        columns: x => new { x.GameId, x.AuthorId },
                        principalTable: "GamePlayers",
                        principalColumns: new[] { "GameId", "UserId" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Statements_Games_GameId",
                        column: x => x.GameId,
                        principalTable: "Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccusationVotes_GameId_DetectiveId",
                table: "AccusationVotes",
                columns: new[] { "GameId", "DetectiveId" });

            migrationBuilder.CreateIndex(
                name: "IX_AccusationVotes_GameId_TargetId",
                table: "AccusationVotes",
                columns: new[] { "GameId", "TargetId" });

            migrationBuilder.CreateIndex(
                name: "IX_AchievementDefinitions_SettingsVersionId_Code",
                table: "AchievementDefinitions",
                columns: new[] { "SettingsVersionId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ActionReceipts_GameId",
                table: "ActionReceipts",
                column: "GameId");

            migrationBuilder.CreateIndex(
                name: "IX_ActionReceipts_UserId_RequestId",
                table: "ActionReceipts",
                columns: new[] { "UserId", "RequestId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AdminAudit_ActorId",
                table: "AdminAudit",
                column: "ActorId");

            migrationBuilder.CreateIndex(
                name: "IX_AdminAudit_OccurredAt",
                table: "AdminAudit",
                column: "OccurredAt");

            migrationBuilder.CreateIndex(
                name: "IX_Arrests_GameId_TargetId",
                table: "Arrests",
                columns: new[] { "GameId", "TargetId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CaseTemplates_CaseKey",
                table: "CaseTemplates",
                column: "CaseKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CaseVariants_CaseVersionId_PlayerCount_Language",
                table: "CaseVariants",
                columns: new[] { "CaseVersionId", "PlayerCount", "Language" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CaseVersions_ReviewedBy",
                table: "CaseVersions",
                column: "ReviewedBy");

            migrationBuilder.CreateIndex(
                name: "IX_CaseVersions_TemplateId",
                table: "CaseVersions",
                column: "TemplateId",
                unique: true,
                filter: "[ReviewStatus] = 'Published'");

            migrationBuilder.CreateIndex(
                name: "IX_CaseVersions_TemplateId_VersionNo",
                table: "CaseVersions",
                columns: new[] { "TemplateId", "VersionNo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClueRecipients_GameId_UserId",
                table: "ClueRecipients",
                columns: new[] { "GameId", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_GameCases_CaseVariantId",
                table: "GameCases",
                column: "CaseVariantId");

            migrationBuilder.CreateIndex(
                name: "IX_GameClues_GameId_ClueKey",
                table: "GameClues",
                columns: new[] { "GameId", "ClueKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GamePlayers_GameId_CharacterKey",
                table: "GamePlayers",
                columns: new[] { "GameId", "CharacterKey" },
                unique: true,
                filter: "[CharacterKey] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_GamePlayers_UserId",
                table: "GamePlayers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Games_RoomId",
                table: "Games",
                column: "RoomId",
                unique: true,
                filter: "[IsRunning] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_Games_RoomId_Number",
                table: "Games",
                columns: new[] { "RoomId", "Number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Games_SettingsVersionId",
                table: "Games",
                column: "SettingsVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_Games_Status_PhaseEndsAt",
                table: "Games",
                columns: new[] { "Status", "PhaseEndsAt" });

            migrationBuilder.CreateIndex(
                name: "IX_GenerationJobs_GameId",
                table: "GenerationJobs",
                column: "GameId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GenerationJobs_RequestId",
                table: "GenerationJobs",
                column: "RequestId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GenerationJobs_Status_LeaseUntil",
                table: "GenerationJobs",
                columns: new[] { "Status", "LeaseUntil" });

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_DispatchedAt_CreatedAt",
                table: "OutboxMessages",
                columns: new[] { "DispatchedAt", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_GameId_UserId",
                table: "OutboxMessages",
                columns: new[] { "GameId", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_RoomId",
                table: "OutboxMessages",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_PendingScoreEvents_GameId_UserId_EventKey",
                table: "PendingScoreEvents",
                columns: new[] { "GameId", "UserId", "EventKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlayerAchievements_DefinitionId",
                table: "PlayerAchievements",
                column: "DefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerAchievements_GameId_UserId",
                table: "PlayerAchievements",
                columns: new[] { "GameId", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_PlayerConnections_GameId_UserId",
                table: "PlayerConnections",
                columns: new[] { "GameId", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_Questions_GameId_DetectiveId",
                table: "Questions",
                columns: new[] { "GameId", "DetectiveId" });

            migrationBuilder.CreateIndex(
                name: "IX_Questions_GameId_Round_DetectiveId_Slot",
                table: "Questions",
                columns: new[] { "GameId", "Round", "DetectiveId", "Slot" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Questions_GameId_TargetId",
                table: "Questions",
                columns: new[] { "GameId", "TargetId" });

            migrationBuilder.CreateIndex(
                name: "IX_RankDefinitions_SettingsVersionId_Code",
                table: "RankDefinitions",
                columns: new[] { "SettingsVersionId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RankDefinitions_SettingsVersionId_MinPoints",
                table: "RankDefinitions",
                columns: new[] { "SettingsVersionId", "MinPoints" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RoomMembers_RoomId_UserId",
                table: "RoomMembers",
                columns: new[] { "RoomId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RoomMembers_UserId",
                table: "RoomMembers",
                column: "UserId",
                unique: true,
                filter: "[LeftAt] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Rooms_Code",
                table: "Rooms",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Rooms_HostUserId",
                table: "Rooms",
                column: "HostUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ScoreEntries_CompletedAt_UserId",
                table: "ScoreEntries",
                columns: new[] { "CompletedAt", "UserId" })
                .Annotation("SqlServer:Include", new[] { "Delta" });

            migrationBuilder.CreateIndex(
                name: "IX_ScoreEntries_GameId_UserId_EventKey",
                table: "ScoreEntries",
                columns: new[] { "GameId", "UserId", "EventKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SettingsVersions_ChangedBy",
                table: "SettingsVersions",
                column: "ChangedBy");

            migrationBuilder.CreateIndex(
                name: "IX_SettingsVersions_VersionNo",
                table: "SettingsVersions",
                column: "VersionNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Statements_GameId_AuthorId",
                table: "Statements",
                columns: new[] { "GameId", "AuthorId" });

            migrationBuilder.CreateIndex(
                name: "IX_Statements_GameId_Round_AuthorId",
                table: "Statements",
                columns: new[] { "GameId", "Round", "AuthorId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccusationVotes");

            migrationBuilder.DropTable(
                name: "ActionReceipts");

            migrationBuilder.DropTable(
                name: "AdminAudit");

            migrationBuilder.DropTable(
                name: "Arrests");

            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "ClueRecipients");

            migrationBuilder.DropTable(
                name: "GameCases");

            migrationBuilder.DropTable(
                name: "GenerationJobs");

            migrationBuilder.DropTable(
                name: "OutboxMessages");

            migrationBuilder.DropTable(
                name: "PendingScoreEvents");

            migrationBuilder.DropTable(
                name: "PlayerAchievements");

            migrationBuilder.DropTable(
                name: "PlayerConnections");

            migrationBuilder.DropTable(
                name: "Questions");

            migrationBuilder.DropTable(
                name: "RankDefinitions");

            migrationBuilder.DropTable(
                name: "RoomMembers");

            migrationBuilder.DropTable(
                name: "ScoreEntries");

            migrationBuilder.DropTable(
                name: "Statements");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "GameClues");

            migrationBuilder.DropTable(
                name: "CaseVariants");

            migrationBuilder.DropTable(
                name: "AchievementDefinitions");

            migrationBuilder.DropTable(
                name: "GamePlayers");

            migrationBuilder.DropTable(
                name: "CaseVersions");

            migrationBuilder.DropTable(
                name: "Games");

            migrationBuilder.DropTable(
                name: "CaseTemplates");

            migrationBuilder.DropTable(
                name: "Rooms");

            migrationBuilder.DropTable(
                name: "SettingsVersions");

            migrationBuilder.DropTable(
                name: "AspNetUsers");
        }
    }
}
