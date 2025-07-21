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
CREATE TABLE [AspNetRoles] (
    [Id] nvarchar(450) NOT NULL,
    [Name] nvarchar(256) NULL,
    [NormalizedName] nvarchar(256) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
);

CREATE TABLE [AspNetUsers] (
    [Id] nvarchar(450) NOT NULL,
    [FirstName] nvarchar(50) NOT NULL,
    [LastName] nvarchar(50) NOT NULL,
    [BirthDate] datetime2 NULL,
    [Address] nvarchar(100) NOT NULL,
    [ZipCode] nvarchar(20) NOT NULL,
    [City] nvarchar(50) NULL,
    [Country] nvarchar(50) NULL,
    [Newsletter] bit NOT NULL,
    [DarkModeEnabled] bit NULL,
    [LanguagePreference] nvarchar(max) NULL,
    [DietaryRestrictions] nvarchar(max) NULL,
    [Allergies] nvarchar(max) NULL,
    [HealthConditions] nvarchar(max) NULL,
    [FitnessGoals] nvarchar(max) NULL,
    [DefaultMealsPerDay] int NULL,
    [PreferredCuisines] nvarchar(max) NULL,
    [CookingSkillLevel] nvarchar(max) NULL,
    [AverageCookingTime] int NULL,
    [AccountCreated] datetime2 NOT NULL,
    [LastLogin] datetime2 NOT NULL,
    [MealPlansGenerated] int NOT NULL,
    [UseOllamaAI] bit NOT NULL,
    [PreferredAIModel] nvarchar(50) NOT NULL,
    [AITemperature] real NOT NULL,
    [AIMaxTokens] int NOT NULL,
    [AICustomInstructions] nvarchar(500) NOT NULL,
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

CREATE TABLE [Feedbacks] (
    [Id] int NOT NULL IDENTITY,
    [UserId] nvarchar(max) NOT NULL,
    [FeedbackType] nvarchar(30) NOT NULL,
    [Subject] nvarchar(100) NOT NULL,
    [Message] nvarchar(1000) NOT NULL,
    [AttachmentPath] nvarchar(260) NULL,
    [Rating] int NULL,
    [AttachmentFileName] nvarchar(max) NULL,
    [AttachmentData] varbinary(max) NULL,
    [SubmittedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_Feedbacks] PRIMARY KEY ([Id])
);

CREATE TABLE [OllamaSettings] (
    [Id] nvarchar(450) NOT NULL,
    [IsEnabled] bit NOT NULL DEFAULT CAST(0 AS bit),
    [Model] nvarchar(50) NULL,
    [ApiUrl] nvarchar(200) NULL DEFAULT N'http://localhost:11434/api/generate',
    [TimeoutSeconds] int NOT NULL DEFAULT 100,
    [Temperature] real NOT NULL DEFAULT CAST(0.7 AS real),
    [MaxTokens] int NOT NULL DEFAULT 1024,
    CONSTRAINT [PK_OllamaSettings] PRIMARY KEY ([Id])
);

CREATE TABLE [AspNetRoleClaims] (
    [Id] int NOT NULL IDENTITY,
    [RoleId] nvarchar(450) NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [AIGenerationLogs] (
    [Id] int NOT NULL IDENTITY,
    [UserId] nvarchar(450) NOT NULL,
    [GenerationDate] datetime2 NOT NULL,
    [AIType] nvarchar(50) NOT NULL,
    [PromptUsed] nvarchar(max) NOT NULL,
    [ParametersJson] nvarchar(max) NOT NULL,
    [ResponseJson] nvarchar(max) NOT NULL,
    [TokensUsed] int NOT NULL,
    [GenerationTime] time NOT NULL,
    [WasSuccessful] bit NOT NULL,
    [ErrorMessage] nvarchar(500) NOT NULL,
    CONSTRAINT [PK_AIGenerationLogs] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AIGenerationLogs_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [AspNetUserClaims] (
    [Id] int NOT NULL IDENTITY,
    [UserId] nvarchar(450) NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [AspNetUserLogins] (
    [LoginProvider] nvarchar(450) NOT NULL,
    [ProviderKey] nvarchar(450) NOT NULL,
    [ProviderDisplayName] nvarchar(max) NULL,
    [UserId] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
    CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [AspNetUserRoles] (
    [UserId] nvarchar(450) NOT NULL,
    [RoleId] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
    CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [AspNetUserTokens] (
    [UserId] nvarchar(450) NOT NULL,
    [LoginProvider] nvarchar(450) NOT NULL,
    [Name] nvarchar(450) NOT NULL,
    [Value] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
    CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [FavoriteRecipe] (
    [Id] int NOT NULL IDENTITY,
    [UserId] nvarchar(450) NOT NULL,
    [RecipeId] nvarchar(max) NOT NULL,
    [RecipeName] nvarchar(max) NOT NULL,
    [DateAdded] datetime2 NOT NULL,
    [Notes] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_FavoriteRecipe] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_FavoriteRecipe_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [MealPlanResults] (
    [PlanID] int NOT NULL IDENTITY,
    [UserID] nvarchar(450) NULL,
    [PlanData] nvarchar(max) NULL,
    [GeneratedAt] datetime2 NOT NULL,
    [IsFavorite] bit NOT NULL,
    [Notes] nvarchar(max) NOT NULL,
    [PlanJson] nvarchar(max) NOT NULL,
    [ParameterJson] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_MealPlanResults] PRIMARY KEY ([PlanID]),
    CONSTRAINT [FK_MealPlanResults_AspNetUsers_UserID] FOREIGN KEY ([UserID]) REFERENCES [AspNetUsers] ([Id])
);

CREATE TABLE [Preferences] (
    [PrefID] int NOT NULL IDENTITY,
    [UserID] nvarchar(450) NULL,
    [Likes] nvarchar(max) NOT NULL,
    [Dislikes] nvarchar(max) NOT NULL,
    [Allergies] nvarchar(max) NOT NULL,
    [DietaryRestriction] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_Preferences] PRIMARY KEY ([PrefID]),
    CONSTRAINT [FK_Preferences_AspNetUsers_UserID] FOREIGN KEY ([UserID]) REFERENCES [AspNetUsers] ([Id])
);

CREATE TABLE [UserActivityLog] (
    [Id] int NOT NULL IDENTITY,
    [UserId] nvarchar(450) NOT NULL,
    [ActivityType] nvarchar(max) NOT NULL,
    [Description] nvarchar(max) NOT NULL,
    [Timestamp] datetime2 NOT NULL,
    [IpAddress] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_UserActivityLog] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_UserActivityLog_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);

CREATE INDEX [IX_AIGenerationLogs_UserId] ON [AIGenerationLogs] ([UserId]);

CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);

CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL;

CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);

CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);

CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);

CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);

CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL;

CREATE INDEX [IX_FavoriteRecipe_UserId] ON [FavoriteRecipe] ([UserId]);

CREATE INDEX [IX_MealPlanResults_UserID] ON [MealPlanResults] ([UserID]);

CREATE INDEX [IX_Preferences_UserID] ON [Preferences] ([UserID]);

CREATE INDEX [IX_UserActivityLog_UserId] ON [UserActivityLog] ([UserId]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250618231227_DbUpdates', N'9.0.0');

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250619003732_NewUpdate2', N'9.0.0');

ALTER TABLE [Preferences] ADD [MealPlanGenerations] int NOT NULL DEFAULT 0;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250620153729_AddMealPlanGenerationsToUserPreference', N'9.0.0');

CREATE TABLE [Subscriptions] (
    [Id] int NOT NULL IDENTITY,
    [UserId] nvarchar(max) NOT NULL,
    [PlanId] nvarchar(max) NOT NULL,
    [PlanName] nvarchar(max) NOT NULL,
    [Amount] decimal(18,2) NOT NULL,
    [PaymentIntentId] nvarchar(max) NOT NULL,
    [Status] nvarchar(max) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [ExpiresAt] datetime2 NULL,
    CONSTRAINT [PK_Subscriptions] PRIMARY KEY ([Id])
);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250627183443_AddSubscriptionsTable', N'9.0.0');

CREATE TABLE [FavoriteStores] (
    [Id] int NOT NULL IDENTITY,
    [UserId] nvarchar(max) NOT NULL,
    [StoreId] nvarchar(max) NOT NULL,
    [AddedDate] datetime2 NOT NULL,
    CONSTRAINT [PK_FavoriteStores] PRIMARY KEY ([Id])
);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250628024924_AddStoreIntegration', N'9.0.0');

CREATE TABLE [UserSearchLogs] (
    [Id] int NOT NULL IDENTITY,
    [UserId] nvarchar(max) NOT NULL,
    [ZipCode] nvarchar(max) NOT NULL,
    [RequestDate] datetime2 NOT NULL,
    CONSTRAINT [PK_UserSearchLogs] PRIMARY KEY ([Id])
);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250630141127_AddUserSearchLog', N'9.0.0');

ALTER TABLE [Preferences] ADD [TrialStartDate] datetime2 NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250709031631_AddTrialStartDateToPreferences', N'9.0.0');

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250709032028_AddTrialStartDateColumnToPreferences', N'9.0.0');

COMMIT;
GO

