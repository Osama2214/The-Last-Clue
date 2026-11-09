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
    WHERE [MigrationId] = N'20261109084342_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetRoles] (
        [Id] nvarchar(450) NOT NULL,
        [Name] nvarchar(256) NULL,
        [NormalizedName] nvarchar(256) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261109084342_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUsers] (
        [Id] nvarchar(450) NOT NULL,
        [DisplayName] nvarchar(80) NOT NULL,
        [AvatarSeed] nvarchar(80) NOT NULL,
        [IsBlocked] bit NOT NULL,
        [CreatedAt] datetime2(3) NOT NULL,
        [UserName] nvarchar(256) NULL,
        [NormalizedUserName] nvarchar(256) NULL,
        [Email] nvarchar(256) NULL,
        [NormalizedEmail] nvarchar(256) NULL,
        [EmailConfirmed] bit NOT NULL,
        [PasswordHash] nvarchar(max) NULL,
        [SecurityStamp] nvarchar(max) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        [PhoneNumber] nvarchar(max) NULL,
        [PhoneNumberConfirmed] bit NOT NULL,
        [TwoFactorEnabled] bit NOT NULL,
        [LockoutEnd] datetimeoffset NULL,
        [LockoutEnabled] bit NOT NULL,
        [AccessFailedCount] int NOT NULL,
        CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261109084342_InitialCreate'
)
BEGIN
    CREATE TABLE [CaseTemplates] (
        [Id] uniqueidentifier NOT NULL,
        [CaseKey] varchar(60) NOT NULL,
        [Title] nvarchar(120) NOT NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_CaseTemplates] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261109084342_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetRoleClaims] (
        [Id] int NOT NULL IDENTITY,
        [RoleId] nvarchar(450) NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261109084342_InitialCreate'
)
BEGIN
    CREATE TABLE [AdminAudit] (
        [Id] uniqueidentifier NOT NULL,
        [ActorId] nvarchar(450) NOT NULL,
        [Action] varchar(60) NOT NULL,
        [EntityType] varchar(40) NOT NULL,
        [EntityId] nvarchar(450) NOT NULL,
        [OccurredAt] datetime2(3) NOT NULL,
        [SafeDetailsJson] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_AdminAudit] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_AdminAudit_SafeDetailsJson] CHECK (ISJSON([SafeDetailsJson]) = 1),
        CONSTRAINT [FK_AdminAudit_AspNetUsers_ActorId] FOREIGN KEY ([ActorId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261109084342_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUserClaims] (
        [Id] int NOT NULL IDENTITY,
        [UserId] nvarchar(450) NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261109084342_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUserLogins] (
        [LoginProvider] nvarchar(450) NOT NULL,
        [ProviderKey] nvarchar(450) NOT NULL,
        [ProviderDisplayName] nvarchar(max) NULL,
        [UserId] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
        CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261109084342_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUserRoles] (
        [UserId] nvarchar(450) NOT NULL,
        [RoleId] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY NONCLUSTERED ([UserId], [RoleId]),
        CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261109084342_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUserTokens] (
        [UserId] nvarchar(450) NOT NULL,
        [LoginProvider] nvarchar(450) NOT NULL,
        [Name] nvarchar(450) NOT NULL,
        [Value] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY NONCLUSTERED ([UserId], [LoginProvider], [Name]),
        CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261109084342_InitialCreate'
)
BEGIN
    CREATE TABLE [Rooms] (
        [Id] uniqueidentifier NOT NULL,
        [Code] varchar(12) NOT NULL,
        [HostUserId] nvarchar(450) NULL,
        [Language] char(2) NOT NULL,
        [Status] varchar(16) NOT NULL,
        [CreatedAt] datetime2(3) NOT NULL,
        [StateVersion] bigint NOT NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_Rooms] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_Rooms_Language] CHECK ([Language] IN ('en', 'ar')),
        CONSTRAINT [CK_Rooms_Status] CHECK ([Status] IN ('Lobby', 'Starting', 'Active', 'Closed')),
        CONSTRAINT [FK_Rooms_AspNetUsers_HostUserId] FOREIGN KEY ([HostUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261109084342_InitialCreate'
)
BEGIN
    CREATE TABLE [SettingsVersions] (
        [Id] uniqueidentifier NOT NULL,
        [VersionNo] int NOT NULL,
        [ValuesJson] nvarchar(max) NOT NULL,
        [PublishedAt] datetime2(3) NOT NULL,
        [ChangedBy] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_SettingsVersions] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_SettingsVersions_ValuesJson] CHECK (ISJSON([ValuesJson]) = 1),
        CONSTRAINT [FK_SettingsVersions_AspNetUsers_ChangedBy] FOREIGN KEY ([ChangedBy]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261109084342_InitialCreate'
)
BEGIN
    CREATE TABLE [CaseVersions] (
        [Id] uniqueidentifier NOT NULL,
        [TemplateId] uniqueidentifier NOT NULL,
        [VersionNo] int NOT NULL,
        [ReviewStatus] varchar(16) NOT NULL,
        [ReviewedBy] nvarchar(450) NULL,
        [ReviewedAt] datetime2(3) NULL,
        CONSTRAINT [PK_CaseVersions] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_CaseVersions_ReviewStatus] CHECK ([ReviewStatus] IN ('Draft', 'Reviewed', 'Published', 'Retired')),
        CONSTRAINT [FK_CaseVersions_AspNetUsers_ReviewedBy] FOREIGN KEY ([ReviewedBy]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_CaseVersions_CaseTemplates_TemplateId] FOREIGN KEY ([TemplateId]) REFERENCES [CaseTemplates] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261109084342_InitialCreate'
)
BEGIN
    CREATE TABLE [RoomMembers] (
        [Id] uniqueidentifier NOT NULL,
        [RoomId] uniqueidentifier NOT NULL,
        [UserId] nvarchar(450) NOT NULL,
        [JoinedAt] datetime2(3) NOT NULL,
        [LeftAt] datetime2(3) NULL,
        [IsReady] bit NOT NULL,
        CONSTRAINT [PK_RoomMembers] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_RoomMembers_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_RoomMembers_Rooms_RoomId] FOREIGN KEY ([RoomId]) REFERENCES [Rooms] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261109084342_InitialCreate'
)
BEGIN
    CREATE TABLE [AchievementDefinitions] (
        [Id] uniqueidentifier NOT NULL,
        [Code] varchar(40) NOT NULL,
        [Name] nvarchar(80) NOT NULL,
        [CriteriaJson] nvarchar(max) NOT NULL,
        [SettingsVersionId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_AchievementDefinitions] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_AchievementDefinitions_CriteriaJson] CHECK (ISJSON([CriteriaJson]) = 1),
        CONSTRAINT [FK_AchievementDefinitions_SettingsVersions_SettingsVersionId] FOREIGN KEY ([SettingsVersionId]) REFERENCES [SettingsVersions] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261109084342_InitialCreate'
)
BEGIN
    CREATE TABLE [Games] (
        [Id] uniqueidentifier NOT NULL,
        [RoomId] uniqueidentifier NOT NULL,
        [Number] int NOT NULL,
        [Round] tinyint NOT NULL,
        [Phase] varchar(20) NOT NULL,
        [PhaseEndsAt] datetime2(3) NULL,
        [PauseStartedAt] datetime2(3) NULL,
        [AccusationsUsed] tinyint NOT NULL,
        [SettingsVersionId] uniqueidentifier NOT NULL,
        [SettingsJson] nvarchar(max) NOT NULL,
        [StateVersion] bigint NOT NULL,
        [CreatedAt] datetime2(3) NOT NULL,
        [CompletedAt] datetime2(3) NULL,
        [Outcome] varchar(24) NULL,
        [CancellationReason] nvarchar(200) NULL,
        [RowVersion] rowversion NOT NULL,
        [Status] varchar(16) NOT NULL,
        [IsRunning] bit NOT NULL,
        CONSTRAINT [PK_Games] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_Games_Outcome] CHECK ([Outcome] IN ('DetectivesWin', 'CriminalWin')),
        CONSTRAINT [CK_Games_Phase] CHECK ([Phase] IN ('Briefing', 'Statements', 'Questioning', 'ClueRelease', 'Accusation')),
        CONSTRAINT [CK_Games_RunningStatus] CHECK (([IsRunning] = 1 AND [Status] IN ('Starting', 'Active')) OR ([IsRunning] = 0 AND [Status] IN ('Completed', 'Cancelled'))),
        CONSTRAINT [CK_Games_SettingsJson] CHECK (ISJSON([SettingsJson]) = 1),
        CONSTRAINT [CK_Games_Status] CHECK ([Status] IN ('Starting', 'Active', 'Completed', 'Cancelled')),
        CONSTRAINT [FK_Games_Rooms_RoomId] FOREIGN KEY ([RoomId]) REFERENCES [Rooms] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Games_SettingsVersions_SettingsVersionId] FOREIGN KEY ([SettingsVersionId]) REFERENCES [SettingsVersions] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261109084342_InitialCreate'
)
BEGIN
    CREATE TABLE [RankDefinitions] (
        [Id] uniqueidentifier NOT NULL,
        [Code] varchar(40) NOT NULL,
        [Name] nvarchar(80) NOT NULL,
        [MinPoints] int NOT NULL,
        [SettingsVersionId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_RankDefinitions] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_RankDefinitions_SettingsVersions_SettingsVersionId] FOREIGN KEY ([SettingsVersionId]) REFERENCES [SettingsVersions] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261109084342_InitialCreate'
)
BEGIN
    CREATE TABLE [CaseVariants] (
        [Id] uniqueidentifier NOT NULL,
        [CaseVersionId] uniqueidentifier NOT NULL,
        [PlayerCount] tinyint NOT NULL,
        [Language] char(2) NOT NULL,
        [PayloadJson] nvarchar(max) NOT NULL,
        [IsValidated] bit NOT NULL,
        CONSTRAINT [PK_CaseVariants] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_CaseVariants_Language] CHECK ([Language] IN ('en', 'ar')),
        CONSTRAINT [CK_CaseVariants_PayloadJson] CHECK (ISJSON([PayloadJson]) = 1),
        CONSTRAINT [CK_CaseVariants_PlayerCount] CHECK ([PlayerCount] BETWEEN 4 AND 8),
        CONSTRAINT [FK_CaseVariants_CaseVersions_CaseVersionId] FOREIGN KEY ([CaseVersionId]) REFERENCES [CaseVersions] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261109084342_InitialCreate'
)
BEGIN
    CREATE TABLE [ActionReceipts] (
        [Id] uniqueidentifier NOT NULL,
        [UserId] nvarchar(450) NOT NULL,
        [GameId] uniqueidentifier NULL,
        [RequestId] uniqueidentifier NOT NULL,
        [PayloadHash] char(64) NOT NULL,
        [ResponseJson] nvarchar(max) NOT NULL,
        [CreatedAt] datetime2(3) NOT NULL,
        CONSTRAINT [PK_ActionReceipts] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_ActionReceipts_ResponseJson] CHECK (ISJSON([ResponseJson]) = 1),
        CONSTRAINT [FK_ActionReceipts_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ActionReceipts_Games_GameId] FOREIGN KEY ([GameId]) REFERENCES [Games] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261109084342_InitialCreate'
)
BEGIN
    CREATE TABLE [GameClues] (
        [Id] uniqueidentifier NOT NULL,
        [GameId] uniqueidentifier NOT NULL,
        [ClueKey] varchar(60) NOT NULL,
        [Round] tinyint NOT NULL,
        [Audience] varchar(12) NOT NULL,
        [CharacterKey] varchar(40) NULL,
        [Body] nvarchar(2000) NOT NULL,
        [ReleasedAt] datetime2(3) NULL,
        CONSTRAINT [PK_GameClues] PRIMARY KEY ([Id]),
        CONSTRAINT [AK_GameClues_GameId_Id] UNIQUE ([GameId], [Id]),
        CONSTRAINT [CK_GameClues_Audience] CHECK ([Audience] IN ('Public', 'Detectives', 'Character')),
        CONSTRAINT [CK_GameClues_Round] CHECK ([Round] BETWEEN 1 AND 3),
        CONSTRAINT [FK_GameClues_Games_GameId] FOREIGN KEY ([GameId]) REFERENCES [Games] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261109084342_InitialCreate'
)
BEGIN
    CREATE TABLE [GamePlayers] (
        [GameId] uniqueidentifier NOT NULL,
        [UserId] nvarchar(450) NOT NULL,
        [Role] varchar(12) NOT NULL,
        [CharacterKey] varchar(40) NULL,
        [PrivateScript] nvarchar(max) NOT NULL,
        [IsActive] bit NOT NULL,
        [IsArrested] bit NOT NULL,
        [GraceUsed] bit NOT NULL,
        [GraceUntil] datetime2(3) NULL,
        [DepartedAt] datetime2(3) NULL,
        CONSTRAINT [PK_GamePlayers] PRIMARY KEY NONCLUSTERED ([GameId], [UserId]),
        CONSTRAINT [CK_GamePlayers_Role] CHECK ([Role] IN ('Detective', 'Criminal', 'Innocent')),
        CONSTRAINT [FK_GamePlayers_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_GamePlayers_Games_GameId] FOREIGN KEY ([GameId]) REFERENCES [Games] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261109084342_InitialCreate'
)
BEGIN
    CREATE TABLE [GenerationJobs] (
        [Id] uniqueidentifier NOT NULL,
        [GameId] uniqueidentifier NOT NULL,
        [RequestId] uniqueidentifier NOT NULL,
        [Status] varchar(16) NOT NULL,
        [DeadlineAt] datetime2(3) NOT NULL,
        [LeaseUntil] datetime2(3) NULL,
        [Attempts] int NOT NULL,
        [LastErrorCode] varchar(40) NULL,
        CONSTRAINT [PK_GenerationJobs] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_GenerationJobs_Status] CHECK ([Status] IN ('Pending', 'Running', 'Succeeded', 'Failed')),
        CONSTRAINT [FK_GenerationJobs_Games_GameId] FOREIGN KEY ([GameId]) REFERENCES [Games] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261109084342_InitialCreate'
)
BEGIN
    CREATE TABLE [GameCases] (
        [GameId] uniqueidentifier NOT NULL,
        [CaseVariantId] uniqueidentifier NULL,
        [Source] varchar(12) NOT NULL,
        [PayloadJson] nvarchar(max) NOT NULL,
        [SnapshotHash] char(64) NOT NULL,
        CONSTRAINT [PK_GameCases] PRIMARY KEY ([GameId]),
        CONSTRAINT [CK_GameCases_PayloadJson] CHECK (ISJSON([PayloadJson]) = 1),
        CONSTRAINT [CK_GameCases_SourceVariant] CHECK (([Source] = 'AI' AND [CaseVariantId] IS NULL) OR ([Source] = 'Fallback' AND [CaseVariantId] IS NOT NULL)),
        CONSTRAINT [FK_GameCases_CaseVariants_CaseVariantId] FOREIGN KEY ([CaseVariantId]) REFERENCES [CaseVariants] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_GameCases_Games_GameId] FOREIGN KEY ([GameId]) REFERENCES [Games] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261109084342_InitialCreate'
)
BEGIN
    CREATE TABLE [AccusationVotes] (
        [GameId] uniqueidentifier NOT NULL,
        [Round] tinyint NOT NULL,
        [DetectiveId] nvarchar(450) NOT NULL,
        [TargetId] nvarchar(450) NOT NULL,
        [SubmittedAt] datetime2(3) NOT NULL,
        CONSTRAINT [PK_AccusationVotes] PRIMARY KEY NONCLUSTERED ([GameId], [Round], [DetectiveId]),
        CONSTRAINT [CK_AccusationVotes_Round] CHECK ([Round] BETWEEN 1 AND 3),
        CONSTRAINT [FK_AccusationVotes_GamePlayers_GameId_DetectiveId] FOREIGN KEY ([GameId], [DetectiveId]) REFERENCES [GamePlayers] ([GameId], [UserId]) ON DELETE NO ACTION,
        CONSTRAINT [FK_AccusationVotes_GamePlayers_GameId_TargetId] FOREIGN KEY ([GameId], [TargetId]) REFERENCES [GamePlayers] ([GameId], [UserId]) ON DELETE NO ACTION,
        CONSTRAINT [FK_AccusationVotes_Games_GameId] FOREIGN KEY ([GameId]) REFERENCES [Games] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261109084342_InitialCreate'
)
BEGIN
    CREATE TABLE [Arrests] (
        [GameId] uniqueidentifier NOT NULL,
        [Round] tinyint NOT NULL,
        [TargetId] nvarchar(450) NOT NULL,
        [IsCriminal] bit NOT NULL,
        [ArrestedAt] datetime2(3) NOT NULL,
        CONSTRAINT [PK_Arrests] PRIMARY KEY ([GameId], [Round]),
        CONSTRAINT [CK_Arrests_Round] CHECK ([Round] BETWEEN 1 AND 3),
        CONSTRAINT [FK_Arrests_GamePlayers_GameId_TargetId] FOREIGN KEY ([GameId], [TargetId]) REFERENCES [GamePlayers] ([GameId], [UserId]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Arrests_Games_GameId] FOREIGN KEY ([GameId]) REFERENCES [Games] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261109084342_InitialCreate'
)
BEGIN
    CREATE TABLE [ClueRecipients] (
        [GameId] uniqueidentifier NOT NULL,
        [ClueId] uniqueidentifier NOT NULL,
        [UserId] nvarchar(450) NOT NULL,
        [AvailableAt] datetime2(3) NOT NULL,
        CONSTRAINT [PK_ClueRecipients] PRIMARY KEY NONCLUSTERED ([GameId], [ClueId], [UserId]),
        CONSTRAINT [FK_ClueRecipients_GameClues_GameId_ClueId] FOREIGN KEY ([GameId], [ClueId]) REFERENCES [GameClues] ([GameId], [Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ClueRecipients_GamePlayers_GameId_UserId] FOREIGN KEY ([GameId], [UserId]) REFERENCES [GamePlayers] ([GameId], [UserId]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261109084342_InitialCreate'
)
BEGIN
    CREATE TABLE [OutboxMessages] (
        [Id] uniqueidentifier NOT NULL,
        [RoomId] uniqueidentifier NULL,
        [GameId] uniqueidentifier NULL,
        [StateVersion] bigint NOT NULL,
        [Audience] varchar(16) NOT NULL,
        [UserId] nvarchar(450) NULL,
        [EventType] varchar(40) NOT NULL,
        [PayloadJson] nvarchar(max) NOT NULL,
        [CreatedAt] datetime2(3) NOT NULL,
        [DispatchedAt] datetime2(3) NULL,
        CONSTRAINT [PK_OutboxMessages] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_OutboxMessages_PayloadJson] CHECK (ISJSON([PayloadJson]) = 1),
        CONSTRAINT [CK_OutboxMessages_Target] CHECK (([RoomId] IS NOT NULL AND [GameId] IS NULL) OR ([RoomId] IS NULL AND [GameId] IS NOT NULL)),
        CONSTRAINT [FK_OutboxMessages_GamePlayers_GameId_UserId] FOREIGN KEY ([GameId], [UserId]) REFERENCES [GamePlayers] ([GameId], [UserId]) ON DELETE NO ACTION,
        CONSTRAINT [FK_OutboxMessages_Games_GameId] FOREIGN KEY ([GameId]) REFERENCES [Games] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_OutboxMessages_Rooms_RoomId] FOREIGN KEY ([RoomId]) REFERENCES [Rooms] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261109084342_InitialCreate'
)
BEGIN
    CREATE TABLE [PendingScoreEvents] (
        [Id] uniqueidentifier NOT NULL,
        [GameId] uniqueidentifier NOT NULL,
        [UserId] nvarchar(450) NOT NULL,
        [EventKey] varchar(80) NOT NULL,
        [Delta] int NOT NULL,
        CONSTRAINT [PK_PendingScoreEvents] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PendingScoreEvents_GamePlayers_GameId_UserId] FOREIGN KEY ([GameId], [UserId]) REFERENCES [GamePlayers] ([GameId], [UserId]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261109084342_InitialCreate'
)
BEGIN
    CREATE TABLE [PlayerAchievements] (
        [UserId] nvarchar(450) NOT NULL,
        [AchievementCode] varchar(40) NOT NULL,
        [DefinitionId] uniqueidentifier NOT NULL,
        [GameId] uniqueidentifier NOT NULL,
        [AwardedAt] datetime2(3) NOT NULL,
        CONSTRAINT [PK_PlayerAchievements] PRIMARY KEY NONCLUSTERED ([UserId], [AchievementCode]),
        CONSTRAINT [FK_PlayerAchievements_AchievementDefinitions_DefinitionId] FOREIGN KEY ([DefinitionId]) REFERENCES [AchievementDefinitions] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_PlayerAchievements_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_PlayerAchievements_GamePlayers_GameId_UserId] FOREIGN KEY ([GameId], [UserId]) REFERENCES [GamePlayers] ([GameId], [UserId]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261109084342_InitialCreate'
)
BEGIN
    CREATE TABLE [PlayerConnections] (
        [ConnectionId] nvarchar(128) NOT NULL,
        [GameId] uniqueidentifier NOT NULL,
        [UserId] nvarchar(450) NOT NULL,
        [LastSeenAt] datetime2(3) NOT NULL,
        [ClosedAt] datetime2(3) NULL,
        CONSTRAINT [PK_PlayerConnections] PRIMARY KEY ([ConnectionId]),
        CONSTRAINT [FK_PlayerConnections_GamePlayers_GameId_UserId] FOREIGN KEY ([GameId], [UserId]) REFERENCES [GamePlayers] ([GameId], [UserId]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261109084342_InitialCreate'
)
BEGIN
    CREATE TABLE [Questions] (
        [Id] uniqueidentifier NOT NULL,
        [GameId] uniqueidentifier NOT NULL,
        [Round] tinyint NOT NULL,
        [DetectiveId] nvarchar(450) NOT NULL,
        [Slot] tinyint NOT NULL,
        [TargetId] nvarchar(450) NOT NULL,
        [Text] nvarchar(300) NOT NULL,
        [AnswerText] nvarchar(1000) NULL,
        [AskedAt] datetime2(3) NOT NULL,
        [AnsweredAt] datetime2(3) NULL,
        CONSTRAINT [PK_Questions] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_Questions_Round] CHECK ([Round] BETWEEN 1 AND 3),
        CONSTRAINT [CK_Questions_Slot] CHECK ([Slot] IN (1, 2)),
        CONSTRAINT [FK_Questions_GamePlayers_GameId_DetectiveId] FOREIGN KEY ([GameId], [DetectiveId]) REFERENCES [GamePlayers] ([GameId], [UserId]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Questions_GamePlayers_GameId_TargetId] FOREIGN KEY ([GameId], [TargetId]) REFERENCES [GamePlayers] ([GameId], [UserId]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Questions_Games_GameId] FOREIGN KEY ([GameId]) REFERENCES [Games] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261109084342_InitialCreate'
)
BEGIN
    CREATE TABLE [ScoreEntries] (
        [Id] uniqueidentifier NOT NULL,
        [GameId] uniqueidentifier NOT NULL,
        [UserId] nvarchar(450) NOT NULL,
        [EventKey] varchar(80) NOT NULL,
        [Delta] int NOT NULL,
        [CompletedAt] datetime2(3) NOT NULL,
        CONSTRAINT [PK_ScoreEntries] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ScoreEntries_GamePlayers_GameId_UserId] FOREIGN KEY ([GameId], [UserId]) REFERENCES [GamePlayers] ([GameId], [UserId]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261109084342_InitialCreate'
)
BEGIN
    CREATE TABLE [Statements] (
        [Id] uniqueidentifier NOT NULL,
        [GameId] uniqueidentifier NOT NULL,
        [Round] tinyint NOT NULL,
        [AuthorId] nvarchar(450) NOT NULL,
        [Text] nvarchar(1000) NOT NULL,
        [CreatedAt] datetime2(3) NOT NULL,
        CONSTRAINT [PK_Statements] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_Statements_Round] CHECK ([Round] BETWEEN 1 AND 3),
        CONSTRAINT [CK_Statements_Text] CHECK (LEN(TRIM([Text])) > 0),
        CONSTRAINT [FK_Statements_GamePlayers_GameId_AuthorId] FOREIGN KEY ([GameId], [AuthorId]) REFERENCES [GamePlayers] ([GameId], [UserId]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Statements_Games_GameId] FOREIGN KEY ([GameId]) REFERENCES [Games] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261109084342_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AccusationVotes_GameId_DetectiveId] ON [AccusationVotes] ([GameId], [DetectiveId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261109084342_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AccusationVotes_GameId_TargetId] ON [AccusationVotes] ([GameId], [TargetId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261109084342_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_AchievementDefinitions_SettingsVersionId_Code] ON [AchievementDefinitions] ([SettingsVersionId], [Code]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261109084342_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ActionReceipts_GameId] ON [ActionReceipts] ([GameId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261109084342_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ActionReceipts_UserId_RequestId] ON [ActionReceipts] ([UserId], [RequestId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261109084342_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AdminAudit_ActorId] ON [AdminAudit] ([ActorId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261109084342_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AdminAudit_OccurredAt] ON [AdminAudit] ([OccurredAt]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261109084342_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Arrests_GameId_TargetId] ON [Arrests] ([GameId], [TargetId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261109084342_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261109084342_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261109084342_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261109084342_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261109084342_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261109084342_InitialCreate'
)
BEGIN
    CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261109084342_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261109084342_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_CaseTemplates_CaseKey] ON [CaseTemplates] ([CaseKey]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261109084342_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_CaseVariants_CaseVersionId_PlayerCount_Language] ON [CaseVariants] ([CaseVersionId], [PlayerCount], [Language]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261109084342_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_CaseVersions_ReviewedBy] ON [CaseVersions] ([ReviewedBy]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261109084342_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_CaseVersions_TemplateId] ON [CaseVersions] ([TemplateId]) WHERE [ReviewStatus] = ''Published''');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261109084342_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_CaseVersions_TemplateId_VersionNo] ON [CaseVersions] ([TemplateId], [VersionNo]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261109084342_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ClueRecipients_GameId_UserId] ON [ClueRecipients] ([GameId], [UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261109084342_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_GameCases_CaseVariantId] ON [GameCases] ([CaseVariantId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261109084342_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_GameClues_GameId_ClueKey] ON [GameClues] ([GameId], [ClueKey]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261109084342_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_GamePlayers_GameId_CharacterKey] ON [GamePlayers] ([GameId], [CharacterKey]) WHERE [CharacterKey] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261109084342_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_GamePlayers_UserId] ON [GamePlayers] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261109084342_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Games_RoomId] ON [Games] ([RoomId]) WHERE [IsRunning] = 1');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261109084342_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Games_RoomId_Number] ON [Games] ([RoomId], [Number]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261109084342_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Games_SettingsVersionId] ON [Games] ([SettingsVersionId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261109084342_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Games_Status_PhaseEndsAt] ON [Games] ([Status], [PhaseEndsAt]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261109084342_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_GenerationJobs_GameId] ON [GenerationJobs] ([GameId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261109084342_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_GenerationJobs_RequestId] ON [GenerationJobs] ([RequestId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261109084342_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_GenerationJobs_Status_LeaseUntil] ON [GenerationJobs] ([Status], [LeaseUntil]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261109084342_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_OutboxMessages_DispatchedAt_CreatedAt] ON [OutboxMessages] ([DispatchedAt], [CreatedAt]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261109084342_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_OutboxMessages_GameId_UserId] ON [OutboxMessages] ([GameId], [UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261109084342_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_OutboxMessages_RoomId] ON [OutboxMessages] ([RoomId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261109084342_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_PendingScoreEvents_GameId_UserId_EventKey] ON [PendingScoreEvents] ([GameId], [UserId], [EventKey]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261109084342_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_PlayerAchievements_DefinitionId] ON [PlayerAchievements] ([DefinitionId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261109084342_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_PlayerAchievements_GameId_UserId] ON [PlayerAchievements] ([GameId], [UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261109084342_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_PlayerConnections_GameId_UserId] ON [PlayerConnections] ([GameId], [UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261109084342_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Questions_GameId_DetectiveId] ON [Questions] ([GameId], [DetectiveId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261109084342_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Questions_GameId_Round_DetectiveId_Slot] ON [Questions] ([GameId], [Round], [DetectiveId], [Slot]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261109084342_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Questions_GameId_TargetId] ON [Questions] ([GameId], [TargetId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261109084342_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_RankDefinitions_SettingsVersionId_Code] ON [RankDefinitions] ([SettingsVersionId], [Code]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261109084342_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_RankDefinitions_SettingsVersionId_MinPoints] ON [RankDefinitions] ([SettingsVersionId], [MinPoints]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261109084342_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_RoomMembers_RoomId_UserId] ON [RoomMembers] ([RoomId], [UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261109084342_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_RoomMembers_UserId] ON [RoomMembers] ([UserId]) WHERE [LeftAt] IS NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261109084342_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Rooms_Code] ON [Rooms] ([Code]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261109084342_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Rooms_HostUserId] ON [Rooms] ([HostUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261109084342_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ScoreEntries_CompletedAt_UserId] ON [ScoreEntries] ([CompletedAt], [UserId]) INCLUDE ([Delta]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261109084342_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ScoreEntries_GameId_UserId_EventKey] ON [ScoreEntries] ([GameId], [UserId], [EventKey]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261109084342_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_SettingsVersions_ChangedBy] ON [SettingsVersions] ([ChangedBy]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261109084342_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_SettingsVersions_VersionNo] ON [SettingsVersions] ([VersionNo]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261109084342_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Statements_GameId_AuthorId] ON [Statements] ([GameId], [AuthorId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261109084342_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Statements_GameId_Round_AuthorId] ON [Statements] ([GameId], [Round], [AuthorId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261109084342_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261109084342_InitialCreate', N'10.0.12');
END;

COMMIT;
GO
