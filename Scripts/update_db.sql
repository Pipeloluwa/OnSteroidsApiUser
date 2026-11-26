IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'RefreshToken' AND Object_ID = Object_ID(N'dbo.UserAuth'))
BEGIN
    ALTER TABLE [dbo].[UserAuth] ADD [RefreshToken] NVARCHAR(256) NULL;
    ALTER TABLE [dbo].[UserAuth] ADD [RefreshTokenExpiryTime] DATETIME2(7) NULL;
END
GO

-- ── Capsule Columns (LockMode & BrdSectionsJson) ────────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'LockMode' AND Object_ID = OBJECT_ID(N'dbo.Capsule'))
BEGIN
    ALTER TABLE [dbo].[Capsule] ADD [LockMode] NVARCHAR(50) NULL;
END
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'BrdSectionsJson' AND Object_ID = OBJECT_ID(N'dbo.Capsule'))
BEGIN
    ALTER TABLE [dbo].[Capsule] ADD [BrdSectionsJson] NVARCHAR(MAX) NULL;
END
GO

-- ── Request Columns (Documentation, TestRail, Custom Blocks, Body Variants, Scripts, Triggers) ─
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'Description' AND Object_ID = OBJECT_ID(N'dbo.Request'))
    ALTER TABLE [dbo].[Request] ADD [Description] NVARCHAR(MAX) NULL;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'DocNotes' AND Object_ID = OBJECT_ID(N'dbo.Request'))
    ALTER TABLE [dbo].[Request] ADD [DocNotes] NVARCHAR(MAX) NULL;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'TestRailPreconditions' AND Object_ID = OBJECT_ID(N'dbo.Request'))
    ALTER TABLE [dbo].[Request] ADD [TestRailPreconditions] NVARCHAR(MAX) NULL;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'TestRailSteps' AND Object_ID = OBJECT_ID(N'dbo.Request'))
    ALTER TABLE [dbo].[Request] ADD [TestRailSteps] NVARCHAR(MAX) NULL;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'TestRailExpected' AND Object_ID = OBJECT_ID(N'dbo.Request'))
    ALTER TABLE [dbo].[Request] ADD [TestRailExpected] NVARCHAR(MAX) NULL;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'CustomDocBlocksJson' AND Object_ID = OBJECT_ID(N'dbo.Request'))
    ALTER TABLE [dbo].[Request] ADD [CustomDocBlocksJson] NVARCHAR(MAX) NULL;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'RawBodyText' AND Object_ID = OBJECT_ID(N'dbo.Request'))
    ALTER TABLE [dbo].[Request] ADD [RawBodyText] NVARCHAR(MAX) NULL;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'UrlEncodedDataJson' AND Object_ID = OBJECT_ID(N'dbo.Request'))
    ALTER TABLE [dbo].[Request] ADD [UrlEncodedDataJson] NVARCHAR(MAX) NULL;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'GraphqlQuery' AND Object_ID = OBJECT_ID(N'dbo.Request'))
    ALTER TABLE [dbo].[Request] ADD [GraphqlQuery] NVARCHAR(MAX) NULL;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'GraphqlVariables' AND Object_ID = OBJECT_ID(N'dbo.Request'))
    ALTER TABLE [dbo].[Request] ADD [GraphqlVariables] NVARCHAR(MAX) NULL;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'PreRequestLanguage' AND Object_ID = OBJECT_ID(N'dbo.Request'))
    ALTER TABLE [dbo].[Request] ADD [PreRequestLanguage] NVARCHAR(20) NULL;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'PostResponseLanguage' AND Object_ID = OBJECT_ID(N'dbo.Request'))
    ALTER TABLE [dbo].[Request] ADD [PostResponseLanguage] NVARCHAR(20) NULL;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'TestScriptLanguage' AND Object_ID = OBJECT_ID(N'dbo.Request'))
    ALTER TABLE [dbo].[Request] ADD [TestScriptLanguage] NVARCHAR(20) NULL;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'EncryptionChannelName' AND Object_ID = OBJECT_ID(N'dbo.Request'))
    ALTER TABLE [dbo].[Request] ADD [EncryptionChannelName] NVARCHAR(255) NULL;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'EncryptionScriptLanguage' AND Object_ID = OBJECT_ID(N'dbo.Request'))
    ALTER TABLE [dbo].[Request] ADD [EncryptionScriptLanguage] NVARCHAR(20) NULL;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'TriggerConfigJson' AND Object_ID = OBJECT_ID(N'dbo.Request'))
    ALTER TABLE [dbo].[Request] ADD [TriggerConfigJson] NVARCHAR(MAX) NULL;
GO

-- ── Capsule Stored Procedures ──────────────────────────────────────────────────────────────────
CREATE OR ALTER PROCEDURE [dbo].[spCapsule_Create]
    @UserId             UNIQUEIDENTIFIER,
    @Name               NVARCHAR(100),
    @Id                 UNIQUEIDENTIFIER = NULL,
    @AutoAuthEnabled    NVARCHAR(50) = 'off',
    @AutoAuthEndpointId NVARCHAR(100) = NULL,
    @LockMode           NVARCHAR(50) = 'unlocked',
    @BrdSectionsJson    NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @NewId UNIQUEIDENTIFIER = COALESCE(@Id, NEWID());
    INSERT INTO dbo.Capsule (Id, UserId, Name, AutoAuthEnabled, AutoAuthEndpointId, LockMode, BrdSectionsJson, CreatedAt, UpdatedAt)
    VALUES (@NewId, @UserId, @Name, COALESCE(@AutoAuthEnabled, 'off'), @AutoAuthEndpointId, COALESCE(@LockMode, 'unlocked'), @BrdSectionsJson, GETUTCDATE(), GETUTCDATE());

    SELECT [Id], [UserId], [Name], [AutoAuthEnabled], [AutoAuthEndpointId], [LockMode], [BrdSectionsJson], [CreatedAt], [UpdatedAt]
    FROM dbo.Capsule
    WHERE Id = @NewId;
END
GO

CREATE OR ALTER PROCEDURE [dbo].[spCapsule_GetAllByUser]
    @UserId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SELECT [Id], [UserId], [Name], [AutoAuthEnabled], [AutoAuthEndpointId], [LockMode], [BrdSectionsJson], [CreatedAt], [UpdatedAt]
    FROM [dbo].[Capsule]
    WHERE [UserId] = @UserId
    ORDER BY [CreatedAt] DESC;
END
GO

CREATE OR ALTER PROCEDURE [dbo].[spCapsule_GetById]
    @Id     UNIQUEIDENTIFIER,
    @UserId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SELECT [Id], [UserId], [Name], [AutoAuthEnabled], [AutoAuthEndpointId], [LockMode], [BrdSectionsJson], [CreatedAt], [UpdatedAt]
    FROM [dbo].[Capsule]
    WHERE [Id] = @Id AND [UserId] = @UserId;
END
GO

CREATE OR ALTER PROCEDURE [dbo].[spCapsule_Update]
    @Id                 UNIQUEIDENTIFIER,
    @UserId             UNIQUEIDENTIFIER,
    @Name               NVARCHAR(255),
    @AutoAuthEnabled    NVARCHAR(50) = NULL,
    @AutoAuthEndpointId NVARCHAR(100) = NULL,
    @LockMode           NVARCHAR(50) = NULL,
    @BrdSectionsJson    NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE [dbo].[Capsule]
    SET [Name]               = @Name,
        [AutoAuthEnabled]    = COALESCE(@AutoAuthEnabled, [AutoAuthEnabled]),
        [AutoAuthEndpointId] = CASE WHEN @AutoAuthEnabled = 'off' THEN NULL ELSE COALESCE(@AutoAuthEndpointId, [AutoAuthEndpointId]) END,
        [LockMode]           = COALESCE(@LockMode, [LockMode]),
        [BrdSectionsJson]    = COALESCE(@BrdSectionsJson, [BrdSectionsJson]),
        [UpdatedAt]          = SYSUTCDATETIME()
    WHERE [Id] = @Id AND [UserId] = @UserId;

    SELECT [Id], [UserId], [Name], [AutoAuthEnabled], [AutoAuthEndpointId], [LockMode], [BrdSectionsJson], [CreatedAt], [UpdatedAt]
    FROM [dbo].[Capsule]
    WHERE [Id] = @Id;
END
GO

-- ── Request Stored Procedures ──────────────────────────────────────────────────────────────────
CREATE OR ALTER PROCEDURE [dbo].[spRequest_Create]
    @Id                     UNIQUEIDENTIFIER    = NULL,
    @CapsuleId              UNIQUEIDENTIFIER,
    @UserId                 UNIQUEIDENTIFIER,
    @Name                   NVARCHAR(255)       = N'New Request',
    @Description            NVARCHAR(MAX)       = NULL,
    @DocNotes               NVARCHAR(MAX)       = NULL,
    @TestRailPreconditions  NVARCHAR(MAX)       = NULL,
    @TestRailSteps          NVARCHAR(MAX)       = NULL,
    @TestRailExpected       NVARCHAR(MAX)       = NULL,
    @CustomDocBlocksJson    NVARCHAR(MAX)       = NULL,
    @Url                    NVARCHAR(2048)      = N'',
    @Method                 NVARCHAR(10)        = N'GET',
    @PayloadType            NVARCHAR(20)        = N'params',
    @BodyType               NVARCHAR(20)        = N'none',
    @RawType                NVARCHAR(20)        = N'JSON',
    @RawBody                NVARCHAR(MAX)       = NULL,
    @RawBodyJson            NVARCHAR(MAX)       = NULL,
    @RawBodyXml             NVARCHAR(MAX)       = NULL,
    @RawBodyText            NVARCHAR(MAX)       = NULL,
    @UrlEncodedDataJson     NVARCHAR(MAX)       = NULL,
    @GraphqlQuery           NVARCHAR(MAX)       = NULL,
    @GraphqlVariables       NVARCHAR(MAX)       = NULL,
    @AuthType               NVARCHAR(10)        = N'none',
    @AuthToken              NVARCHAR(MAX)       = NULL,
    @PreRequestScript       NVARCHAR(MAX)       = NULL,
    @PostResponseScript     NVARCHAR(MAX)       = NULL,
    @TestScript             NVARCHAR(MAX)       = NULL,
    @TestScriptEnabled      BIT                 = 0,
    @PreRequestLanguage     NVARCHAR(20)        = N'javascript',
    @PostResponseLanguage   NVARCHAR(20)        = N'javascript',
    @TestScriptLanguage     NVARCHAR(20)        = N'javascript',
    @EncryptionAlgorithm    NVARCHAR(20)        = N'none',
    @EncryptionKey          NVARCHAR(MAX)       = NULL,
    @AutoEncryptBody        BIT                 = 0,
    @AutoEncryptHeaders     BIT                 = 0,
    @EncryptionChannelName  NVARCHAR(255)       = NULL,
    @EncryptedHeaders       NVARCHAR(MAX)       = NULL,
    @EncryptedBodyPaths     NVARCHAR(MAX)       = NULL,
    @EncryptionScript       NVARCHAR(MAX)       = NULL,
    @EncryptionScriptLanguage NVARCHAR(20)      = N'javascript',
    @FollowRedirects        BIT                 = 1,
    @VerifySsl              BIT                 = 1,
    @EnableCookies          BIT                 = 1,
    @BypassCors             BIT                 = 1,
    @PostTriggerTabId       NVARCHAR(100)       = NULL,
    @TriggerConfigJson      NVARCHAR(MAX)       = NULL,
    @AutoAuthEnabled        BIT                 = 0
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @NewId UNIQUEIDENTIFIER = COALESCE(@Id, NEWID());
    IF EXISTS (SELECT 1 FROM [dbo].[Request] WHERE [Id] = @NewId)
        SET @NewId = NEWID();

    INSERT INTO [dbo].[Request] (
        [Id], [CapsuleId], [UserId], [Name],
        [Description], [DocNotes], [TestRailPreconditions], [TestRailSteps], [TestRailExpected], [CustomDocBlocksJson],
        [Url], [Method],
        [PayloadType], [BodyType], [RawType], [RawBody], [RawBodyJson], [RawBodyXml],
        [RawBodyText], [UrlEncodedDataJson], [GraphqlQuery], [GraphqlVariables],
        [AuthType], [AuthToken],
        [PreRequestScript], [PostResponseScript], [TestScript], [TestScriptEnabled],
        [PreRequestLanguage], [PostResponseLanguage], [TestScriptLanguage],
        [EncryptionAlgorithm], [EncryptionKey], [AutoEncryptBody], [AutoEncryptHeaders],
        [EncryptionChannelName], [EncryptedHeaders], [EncryptedBodyPaths], [EncryptionScript], [EncryptionScriptLanguage],
        [FollowRedirects], [VerifySsl], [EnableCookies], [BypassCors],
        [PostTriggerTabId], [TriggerConfigJson], [AutoAuthEnabled]
    )
    VALUES (
        @NewId, @CapsuleId, @UserId, @Name,
        @Description, @DocNotes, @TestRailPreconditions, @TestRailSteps, @TestRailExpected, @CustomDocBlocksJson,
        @Url, @Method,
        @PayloadType, @BodyType, @RawType, @RawBody, @RawBodyJson, @RawBodyXml,
        @RawBodyText, @UrlEncodedDataJson, @GraphqlQuery, @GraphqlVariables,
        @AuthType, @AuthToken,
        @PreRequestScript, @PostResponseScript, @TestScript, @TestScriptEnabled,
        COALESCE(@PreRequestLanguage, N'javascript'), COALESCE(@PostResponseLanguage, N'javascript'), COALESCE(@TestScriptLanguage, N'javascript'),
        @EncryptionAlgorithm, @EncryptionKey, @AutoEncryptBody, @AutoEncryptHeaders,
        @EncryptionChannelName, @EncryptedHeaders, @EncryptedBodyPaths, @EncryptionScript, COALESCE(@EncryptionScriptLanguage, N'javascript'),
        @FollowRedirects, @VerifySsl, @EnableCookies, @BypassCors,
        @PostTriggerTabId, @TriggerConfigJson, @AutoAuthEnabled
    );

    SELECT * FROM [dbo].[Request] WHERE [Id] = @NewId;
END
GO

CREATE OR ALTER PROCEDURE [dbo].[spRequest_Update]
    @Id                     UNIQUEIDENTIFIER,
    @CapsuleId              UNIQUEIDENTIFIER,
    @UserId                 UNIQUEIDENTIFIER,
    @Name                   NVARCHAR(255),
    @Description            NVARCHAR(MAX)       = NULL,
    @DocNotes               NVARCHAR(MAX)       = NULL,
    @TestRailPreconditions  NVARCHAR(MAX)       = NULL,
    @TestRailSteps          NVARCHAR(MAX)       = NULL,
    @TestRailExpected       NVARCHAR(MAX)       = NULL,
    @CustomDocBlocksJson    NVARCHAR(MAX)       = NULL,
    @Url                    NVARCHAR(2048),
    @Method                 NVARCHAR(10),
    @PayloadType            NVARCHAR(20),
    @BodyType               NVARCHAR(20),
    @RawType                NVARCHAR(20),
    @RawBody                NVARCHAR(MAX),
    @RawBodyJson            NVARCHAR(MAX),
    @RawBodyXml             NVARCHAR(MAX),
    @RawBodyText            NVARCHAR(MAX)       = NULL,
    @UrlEncodedDataJson     NVARCHAR(MAX)       = NULL,
    @GraphqlQuery           NVARCHAR(MAX)       = NULL,
    @GraphqlVariables       NVARCHAR(MAX)       = NULL,
    @AuthType               NVARCHAR(10),
    @AuthToken              NVARCHAR(MAX),
    @PreRequestScript       NVARCHAR(MAX),
    @PostResponseScript     NVARCHAR(MAX),
    @TestScript             NVARCHAR(MAX),
    @TestScriptEnabled      BIT,
    @PreRequestLanguage     NVARCHAR(20)        = NULL,
    @PostResponseLanguage   NVARCHAR(20)        = NULL,
    @TestScriptLanguage     NVARCHAR(20)        = NULL,
    @EncryptionAlgorithm    NVARCHAR(20),
    @EncryptionKey          NVARCHAR(MAX),
    @AutoEncryptBody        BIT,
    @AutoEncryptHeaders     BIT,
    @EncryptionChannelName  NVARCHAR(255)       = NULL,
    @EncryptedHeaders       NVARCHAR(MAX),
    @EncryptedBodyPaths     NVARCHAR(MAX),
    @EncryptionScript       NVARCHAR(MAX),
    @EncryptionScriptLanguage NVARCHAR(20)      = NULL,
    @FollowRedirects        BIT,
    @VerifySsl              BIT,
    @EnableCookies          BIT,
    @BypassCors             BIT,
    @PostTriggerTabId       NVARCHAR(100)       = NULL,
    @TriggerConfigJson      NVARCHAR(MAX)       = NULL,
    @AutoAuthEnabled        BIT                 = 0
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE [dbo].[Request]
    SET 
        [CapsuleId]                 = @CapsuleId,
        [Name]                      = @Name,
        [Description]               = @Description,
        [DocNotes]                  = @DocNotes,
        [TestRailPreconditions]     = @TestRailPreconditions,
        [TestRailSteps]             = @TestRailSteps,
        [TestRailExpected]          = @TestRailExpected,
        [CustomDocBlocksJson]       = @CustomDocBlocksJson,
        [Url]                       = @Url,
        [Method]                    = @Method,
        [PayloadType]               = @PayloadType,
        [BodyType]                  = @BodyType,
        [RawType]                   = @RawType,
        [RawBody]                   = @RawBody,
        [RawBodyJson]               = @RawBodyJson,
        [RawBodyXml]                = @RawBodyXml,
        [RawBodyText]               = @RawBodyText,
        [UrlEncodedDataJson]        = @UrlEncodedDataJson,
        [GraphqlQuery]              = @GraphqlQuery,
        [GraphqlVariables]          = @GraphqlVariables,
        [AuthType]                  = @AuthType,
        [AuthToken]                 = @AuthToken,
        [PreRequestScript]          = @PreRequestScript,
        [PostResponseScript]        = @PostResponseScript,
        [TestScript]                = @TestScript,
        [TestScriptEnabled]         = @TestScriptEnabled,
        [PreRequestLanguage]        = COALESCE(@PreRequestLanguage, [PreRequestLanguage], N'javascript'),
        [PostResponseLanguage]      = COALESCE(@PostResponseLanguage, [PostResponseLanguage], N'javascript'),
        [TestScriptLanguage]        = COALESCE(@TestScriptLanguage, [TestScriptLanguage], N'javascript'),
        [EncryptionAlgorithm]       = @EncryptionAlgorithm,
        [EncryptionKey]             = @EncryptionKey,
        [AutoEncryptBody]           = @AutoEncryptBody,
        [AutoEncryptHeaders]        = @AutoEncryptHeaders,
        [EncryptionChannelName]     = COALESCE(@EncryptionChannelName, [EncryptionChannelName]),
        [EncryptedHeaders]          = @EncryptedHeaders,
        [EncryptedBodyPaths]        = @EncryptedBodyPaths,
        [EncryptionScript]          = @EncryptionScript,
        [EncryptionScriptLanguage]  = COALESCE(@EncryptionScriptLanguage, [EncryptionScriptLanguage], N'javascript'),
        [FollowRedirects]           = @FollowRedirects,
        [VerifySsl]                 = @VerifySsl,
        [EnableCookies]             = @EnableCookies,
        [BypassCors]                = @BypassCors,
        [PostTriggerTabId]          = @PostTriggerTabId,
        [TriggerConfigJson]         = @TriggerConfigJson,
        [AutoAuthEnabled]           = @AutoAuthEnabled,
        [UpdatedAt]                 = GETUTCDATE()
    WHERE [Id] = @Id AND [UserId] = @UserId;

    SELECT * FROM [dbo].[Request] WHERE [Id] = @Id;
END
GO

CREATE OR ALTER PROCEDURE [dbo].[spRequest_Duplicate]
    @SourceId UNIQUEIDENTIFIER,
    @UserId   UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @NewId UNIQUEIDENTIFIER = NEWID();
    BEGIN TRANSACTION;

    INSERT INTO [dbo].[Request] (
        [Id], [CapsuleId], [UserId], [Name],
        [Description], [DocNotes], [TestRailPreconditions], [TestRailSteps], [TestRailExpected], [CustomDocBlocksJson],
        [Url], [Method],
        [PayloadType], [BodyType], [RawType], [RawBody], [RawBodyJson], [RawBodyXml],
        [RawBodyText], [UrlEncodedDataJson], [GraphqlQuery], [GraphqlVariables],
        [AuthType], [AuthToken],
        [PreRequestScript], [PostResponseScript], [TestScript], [TestScriptEnabled],
        [PreRequestLanguage], [PostResponseLanguage], [TestScriptLanguage],
        [EncryptionAlgorithm], [EncryptionKey], [AutoEncryptBody], [AutoEncryptHeaders],
        [EncryptionChannelName], [EncryptedHeaders], [EncryptedBodyPaths], [EncryptionScript], [EncryptionScriptLanguage],
        [FollowRedirects], [VerifySsl], [EnableCookies], [BypassCors],
        [PostTriggerTabId], [TriggerConfigJson], [AutoAuthEnabled]
    )
    SELECT
        @NewId, [CapsuleId], [UserId], [Name] + N' (Copy)',
        [Description], [DocNotes], [TestRailPreconditions], [TestRailSteps], [TestRailExpected], [CustomDocBlocksJson],
        [Url], [Method],
        [PayloadType], [BodyType], [RawType], [RawBody], [RawBodyJson], [RawBodyXml],
        [RawBodyText], [UrlEncodedDataJson], [GraphqlQuery], [GraphqlVariables],
        [AuthType], [AuthToken],
        [PreRequestScript], [PostResponseScript], [TestScript], [TestScriptEnabled],
        [PreRequestLanguage], [PostResponseLanguage], [TestScriptLanguage],
        [EncryptionAlgorithm], [EncryptionKey], [AutoEncryptBody], [AutoEncryptHeaders],
        [EncryptionChannelName], [EncryptedHeaders], [EncryptedBodyPaths], [EncryptionScript], [EncryptionScriptLanguage],
        [FollowRedirects], [VerifySsl], [EnableCookies], [BypassCors],
        [PostTriggerTabId], [TriggerConfigJson], [AutoAuthEnabled]
    FROM [dbo].[Request]
    WHERE [Id] = @SourceId AND [UserId] = @UserId;

    INSERT INTO [dbo].[RequestParam] ([RequestId], [IsEnabled], [ParamKey], [ParamValue], [SortOrder])
    SELECT @NewId, [IsEnabled], [ParamKey], [ParamValue], [SortOrder] FROM [dbo].[RequestParam] WHERE [RequestId] = @SourceId;

    INSERT INTO [dbo].[RequestHeader] ([RequestId], [IsEnabled], [HeaderKey], [HeaderValue], [SortOrder])
    SELECT @NewId, [IsEnabled], [HeaderKey], [HeaderValue], [SortOrder] FROM [dbo].[RequestHeader] WHERE [RequestId] = @SourceId;

    INSERT INTO [dbo].[RequestFormData] ([RequestId], [IsEnabled], [FieldKey], [FieldValue], [FieldType], [SortOrder])
    SELECT @NewId, [IsEnabled], [FieldKey], [FieldValue], [FieldType], [SortOrder] FROM [dbo].[RequestFormData] WHERE [RequestId] = @SourceId;

    COMMIT TRANSACTION;
    SELECT * FROM [dbo].[Request] WHERE [Id] = @NewId;
END
GO

-- ── Modernize Default Seed Scripts (st.* without legacy wrapper functions) ─────────────────────
CREATE OR ALTER PROCEDURE [dbo].[spScript_SeedDefaultsForUser]
    @UserId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @LongScript NVARCHAR(MAX) = N'// Top-level imports are supported (e.g. import CryptoJS from ''crypto-js'';)
// Access request fields & encryption targets via st.* (headers, body, params, encryptedHeaders, encryptedBodyPaths)
const parameters = {};
const shouldEncryptAllHeaders = Boolean(st.autoEncryptHeaders);
const encHeadersList = st.encryptedHeaders || [];
for (const h of st.headers || []) {
    if (h.enabled && h.key && (shouldEncryptAllHeaders || encHeadersList.includes(h.key))) {
        parameters[h.key] = h.value;
    }
}
const bodyObj = (st.body && typeof st.body === ''object'' && !Array.isArray(st.body)) ? st.body : null;
if (bodyObj) {
    const targetPaths = st.autoEncryptBody ? Object.keys(bodyObj) : (st.encryptedBodyPaths || []);
    for (const path of targetPaths) {
        const val = path.split(''.'').reduce((acc, part) => acc && acc[part], bodyObj);
        if (val !== undefined && val !== null) {
            parameters[path] = val;
        }
    }
}
if (Object.keys(parameters).length > 0 && st.getEnv(''encryptUrl'')) {
    const res = await fetch(st.getEnv(''encryptUrl''), {
        method: ''POST'',
        headers: { ''Content-Type'': ''application/json'' },
        body: JSON.stringify({ channelName: st.channelName || ''Default Channel'', parameters })
    });
    if (res.ok) {
        const result = await res.json();
        const encryptedParams = result.parameters || result;
        for (const h of st.headers || []) {
            if (h.key && encryptedParams[h.key] !== undefined) h.value = String(encryptedParams[h.key]);
        }
        if (bodyObj) {
            for (const key of Object.keys(encryptedParams)) {
                st.setBodyField(key, encryptedParams[key]);
            }
            return JSON.stringify(bodyObj, null, 2);
        }
    }
}
return st.body;';

    DECLARE @PreDefault NVARCHAR(MAX) = N'// Top-level imports are supported (e.g. import CryptoJS from ''crypto-js'';)
// Access request parameters & helpers directly via st.* (type "st." for autocomplete, e.g. st.headers, st.body, st.setHeader)';

    DECLARE @PostDefault NVARCHAR(MAX) = N'// Top-level imports are supported across all script types
// Access response & request parameters directly via st.* (type "st." for autocomplete, e.g. st.responseBody, st.linkResponseToRequest)';

    IF NOT EXISTS (SELECT 1 FROM [dbo].[Script] WHERE [UserId] = @UserId AND [Type] = 'Encryption' AND [Name] = 'Encrypt' AND [IsDefault] = 1)
        INSERT INTO [dbo].[Script] ([Id], [UserId], [Type], [Name], [Content], [IsDefault], [OriginalContent])
        VALUES (NEWID(), @UserId, 'Encryption', 'Encrypt', @LongScript, 1, @LongScript);

    IF NOT EXISTS (SELECT 1 FROM [dbo].[Script] WHERE [UserId] = @UserId AND [Type] = 'Encryption' AND [Name] = 'Decrypt' AND [IsDefault] = 1)
        INSERT INTO [dbo].[Script] ([Id], [UserId], [Type], [Name], [Content], [IsDefault], [OriginalContent])
        VALUES (NEWID(), @UserId, 'Encryption', 'Decrypt', @LongScript, 1, @LongScript);

    IF NOT EXISTS (SELECT 1 FROM [dbo].[Script] WHERE [UserId] = @UserId AND [Type] = 'PreRequest' AND [IsDefault] = 1)
        INSERT INTO [dbo].[Script] ([Id], [UserId], [Type], [Name], [Content], [IsDefault], [OriginalContent])
        VALUES (NEWID(), @UserId, 'PreRequest', 'Pre Request Script', @PreDefault, 1, @PreDefault);

    IF NOT EXISTS (SELECT 1 FROM [dbo].[Script] WHERE [UserId] = @UserId AND [Type] = 'PostRequest' AND [IsDefault] = 1)
        INSERT INTO [dbo].[Script] ([Id], [UserId], [Type], [Name], [Content], [IsDefault], [OriginalContent])
        VALUES (NEWID(), @UserId, 'PostRequest', 'Post Request Script', @PostDefault, 1, @PostDefault);

    SELECT [Id], [UserId], [Type], [Name], [Content], [IsDefault], [OriginalContent], [CreatedAt], [UpdatedAt]
    FROM [dbo].[Script]
    WHERE [UserId] = @UserId;
END
GO

-- =========================================================================
-- Licensing & Subscription Entitlement Tables & Stored Procedures
-- =========================================================================
IF OBJECT_ID(N'dbo.UserAuth', N'U') IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'EmailOtpVerifiedAt' AND Object_ID = OBJECT_ID(N'dbo.UserAuth'))
        ALTER TABLE [dbo].[UserAuth] ADD [EmailOtpVerifiedAt] DATETIME2(7) NULL;
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'SubscriptionTier' AND Object_ID = OBJECT_ID(N'dbo.UserAuth'))
        ALTER TABLE [dbo].[UserAuth] ADD [SubscriptionTier] NVARCHAR(32) NOT NULL CONSTRAINT [DF_UserAuth_SubscriptionTier] DEFAULT (N'community');
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'SubscriptionExpiresAt' AND Object_ID = OBJECT_ID(N'dbo.UserAuth'))
        ALTER TABLE [dbo].[UserAuth] ADD [SubscriptionExpiresAt] DATETIME2(7) NULL;
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'ActiveLicenseKeyId' AND Object_ID = OBJECT_ID(N'dbo.UserAuth'))
        ALTER TABLE [dbo].[UserAuth] ADD [ActiveLicenseKeyId] UNIQUEIDENTIFIER NULL;
END
GO

IF OBJECT_ID(N'dbo.UserAuth', N'U') IS NOT NULL
BEGIN
    UPDATE [dbo].[UserAuth]
    SET [EmailOtpVerifiedAt] = COALESCE([UpdatedAt], [CreatedAt], SYSUTCDATETIME())
    WHERE [EmailOtpVerifiedAt] IS NULL
      AND [PasswordHash] IS NOT NULL
      AND LEN([PasswordHash]) > 0;
END
GO

IF OBJECT_ID(N'dbo.LicenseKey', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[LicenseKey](
        [Id] [uniqueidentifier] NOT NULL CONSTRAINT [DF_LicenseKey_Id] DEFAULT (NEWID()),
        [LicenseKey] [nvarchar](128) NULL,
        [CodeHash] [nvarchar](64) NULL,
        [MaskedCode] [nvarchar](64) NULL,
        [Tier] [nvarchar](32) NOT NULL CONSTRAINT [DF_LicenseKey_Tier] DEFAULT (N'professional'),
        [LicenseType] [nvarchar](32) NOT NULL CONSTRAINT [DF_LicenseKey_LicenseType] DEFAULT (N'enterprise'),
        [IndividualType] [nvarchar](32) NULL,
        [BillingCycle] [nvarchar](32) NOT NULL CONSTRAINT [DF_LicenseKey_BillingCycle] DEFAULT (N'annual'),
        [DurationDays] [int] NULL,
        [MaxActivations] [int] NOT NULL CONSTRAINT [DF_LicenseKey_MaxActivations] DEFAULT (10),
        [ActiveActivations] [int] NOT NULL CONSTRAINT [DF_LicenseKey_ActiveActivations] DEFAULT (0),
        [AllowedEmail] [nvarchar](320) NULL,
        [Notes] [nvarchar](500) NULL,
        [IsRevoked] [bit] NOT NULL CONSTRAINT [DF_LicenseKey_IsRevoked] DEFAULT (0),
        [RevokedAt] [datetime2](7) NULL,
        [ExpiresAt] [datetime2](7) NULL,
        [CreatedAt] [datetime2](7) NOT NULL CONSTRAINT [DF_LicenseKey_CreatedAt] DEFAULT (SYSUTCDATETIME()),
        [UpdatedAt] [datetime2](7) NOT NULL CONSTRAINT [DF_LicenseKey_UpdatedAt] DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT [PK_LicenseKey] PRIMARY KEY CLUSTERED ([Id] ASC)
    );
    CREATE UNIQUE NONCLUSTERED INDEX [UQ_LicenseKey_CodeHash] ON [dbo].[LicenseKey] ([CodeHash] ASC) WHERE [CodeHash] IS NOT NULL;
    CREATE NONCLUSTERED INDEX [IX_LicenseKey_AllowedEmail] ON [dbo].[LicenseKey] ([AllowedEmail] ASC);
END
ELSE
BEGIN
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'LicenseKey' AND Object_ID = OBJECT_ID(N'dbo.LicenseKey'))
        ALTER TABLE [dbo].[LicenseKey] ADD [LicenseKey] NVARCHAR(128) NULL;
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'LicenseType' AND Object_ID = OBJECT_ID(N'dbo.LicenseKey'))
        ALTER TABLE [dbo].[LicenseKey] ADD [LicenseType] NVARCHAR(32) NOT NULL CONSTRAINT [DF_LicenseKey_LicenseType] DEFAULT (N'enterprise');
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'IndividualType' AND Object_ID = OBJECT_ID(N'dbo.LicenseKey'))
        ALTER TABLE [dbo].[LicenseKey] ADD [IndividualType] NVARCHAR(32) NULL;

    IF EXISTS (SELECT 1 FROM sys.key_constraints WHERE name = N'UQ_LicenseKey_CodeHash' AND parent_object_id = OBJECT_ID(N'dbo.LicenseKey'))
        ALTER TABLE [dbo].[LicenseKey] DROP CONSTRAINT [UQ_LicenseKey_CodeHash];

    ALTER TABLE [dbo].[LicenseKey] ALTER COLUMN [CodeHash] NVARCHAR(64) NULL;
    ALTER TABLE [dbo].[LicenseKey] ALTER COLUMN [MaskedCode] NVARCHAR(64) NULL;

    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UQ_LicenseKey_CodeHash' AND object_id = OBJECT_ID(N'dbo.LicenseKey'))
        CREATE UNIQUE NONCLUSTERED INDEX [UQ_LicenseKey_CodeHash] ON [dbo].[LicenseKey] ([CodeHash] ASC) WHERE [CodeHash] IS NOT NULL;
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_LicenseKey_AllowedEmail' AND object_id = OBJECT_ID(N'dbo.LicenseKey'))
        CREATE NONCLUSTERED INDEX [IX_LicenseKey_AllowedEmail] ON [dbo].[LicenseKey] ([AllowedEmail] ASC);
END
GO

CREATE OR ALTER TRIGGER [dbo].[TR_LicenseKey_SyncMetadata]
ON [dbo].[LicenseKey]
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    IF TRIGGER_NESTLEVEL() > 1 RETURN;

    UPDATE lk
    SET
        lk.[LicenseKey] = CASE
            WHEN i.[LicenseKey] IS NULL THEN lk.[LicenseKey]
            ELSE UPPER(LTRIM(RTRIM(i.[LicenseKey])))
        END,
        lk.[CodeHash] = CASE
            WHEN NULLIF(LTRIM(RTRIM(COALESCE(i.[LicenseKey], N''))), N'') IS NOT NULL
                 AND (NULLIF(LTRIM(RTRIM(COALESCE(i.[CodeHash], N''))), N'') IS NULL OR UPDATE([LicenseKey]))
            THEN CONVERT(NVARCHAR(64), HASHBYTES('SHA2_256', CAST(UPPER(REPLACE(LTRIM(RTRIM(i.[LicenseKey])), N' ', N'')) AS VARCHAR(128))), 2)
            ELSE i.[CodeHash]
        END,
        lk.[MaskedCode] = CASE
            WHEN NULLIF(LTRIM(RTRIM(COALESCE(i.[LicenseKey], N''))), N'') IS NOT NULL
                 AND (NULLIF(LTRIM(RTRIM(COALESCE(i.[MaskedCode], N''))), N'') IS NULL OR UPDATE([LicenseKey]))
            THEN CASE
                WHEN LEN(UPPER(REPLACE(LTRIM(RTRIM(i.[LicenseKey])), N' ', N''))) >= 13
                THEN LEFT(UPPER(REPLACE(LTRIM(RTRIM(i.[LicenseKey])), N' ', N'')), 8) + N'-••••-••••-' + RIGHT(UPPER(REPLACE(LTRIM(RTRIM(i.[LicenseKey])), N' ', N'')), 4)
                ELSE LEFT(UPPER(REPLACE(LTRIM(RTRIM(i.[LicenseKey])), N' ', N'')), 4) + N'-••••'
            END
            ELSE i.[MaskedCode]
        END,
        lk.[AllowedEmail] = NULLIF(LOWER(LTRIM(RTRIM(i.[AllowedEmail]))), N''),
        lk.[LicenseType] = LOWER(LTRIM(RTRIM(COALESCE(NULLIF(i.[LicenseType], N''), lk.[Tier], N'enterprise')))),
        lk.[IndividualType] = NULLIF(LOWER(LTRIM(RTRIM(i.[IndividualType]))), N''),
        lk.[UpdatedAt] = SYSUTCDATETIME()
    FROM [dbo].[LicenseKey] lk
    INNER JOIN inserted i ON i.[Id] = lk.[Id];
END
GO

IF OBJECT_ID(N'dbo.LicenseActivation', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[LicenseActivation](
        [Id] [uniqueidentifier] NOT NULL CONSTRAINT [DF_LicenseActivation_Id] DEFAULT (NEWID()),
        [LicenseKeyId] [uniqueidentifier] NOT NULL,
        [UserId] [uniqueidentifier] NOT NULL,
        [DeviceLabel] [nvarchar](150) NULL,
        [IsActive] [bit] NOT NULL CONSTRAINT [DF_LicenseActivation_IsActive] DEFAULT (1),
        [ActivatedAt] [datetime2](7) NOT NULL CONSTRAINT [DF_LicenseActivation_ActivatedAt] DEFAULT (SYSUTCDATETIME()),
        [LastVerifiedAt] [datetime2](7) NOT NULL CONSTRAINT [DF_LicenseActivation_LastVerifiedAt] DEFAULT (SYSUTCDATETIME()),
        [DeactivatedAt] [datetime2](7) NULL,
        CONSTRAINT [PK_LicenseActivation] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_LicenseActivation_LicenseKey] FOREIGN KEY([LicenseKeyId]) REFERENCES [dbo].[LicenseKey]([Id]),
        CONSTRAINT [FK_LicenseActivation_UserAuth] FOREIGN KEY([UserId]) REFERENCES [dbo].[UserAuth]([Id])
    );
    CREATE NONCLUSTERED INDEX [IX_LicenseActivation_UserId_IsActive] ON [dbo].[LicenseActivation] ([UserId] ASC, [IsActive] ASC);
    CREATE NONCLUSTERED INDEX [IX_LicenseActivation_LicenseKeyId_IsActive] ON [dbo].[LicenseActivation] ([LicenseKeyId] ASC, [IsActive] ASC);
END
GO

CREATE OR ALTER PROCEDURE [dbo].[spUserAuth_ChangePassword]
    @Id             UNIQUEIDENTIFIER,
    @PasswordHash   NVARCHAR(512)
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM [dbo].[UserAuth] WHERE [Id] = @Id)
    BEGIN
        RAISERROR('User not found.', 16, 1);
        RETURN;
    END

    UPDATE [dbo].[UserAuth]
    SET [PasswordHash]    = @PasswordHash,
        [IsOtpVerified]   = 0,
        [UpdatedAt]       = SYSUTCDATETIME()
    WHERE [Id] = @Id;

    SELECT u.[Id], u.[Email], u.[IsAuthenticated], u.[PasswordHash], u.[IsOtpVerified],
           CAST(1 AS BIT) AS [HasPassword],
           CASE
               WHEN lk.[Id] IS NOT NULL THEN
                   CASE
                       WHEN lk.[IsRevoked] = 1
                         OR (COALESCE(lk.[ExpiresAt], u.[SubscriptionExpiresAt]) IS NOT NULL
                             AND COALESCE(lk.[ExpiresAt], u.[SubscriptionExpiresAt]) <= SYSUTCDATETIME())
                       THEN N'community'
                       ELSE COALESCE(NULLIF(LTRIM(RTRIM(u.[SubscriptionTier])), N''), N'community')
                   END
               WHEN u.[SubscriptionExpiresAt] IS NOT NULL AND u.[SubscriptionExpiresAt] <= SYSUTCDATETIME()
                   THEN N'community'
               ELSE COALESCE(NULLIF(LTRIM(RTRIM(u.[SubscriptionTier])), N''), N'community')
           END AS [SubscriptionTier],
           COALESCE(lk.[ExpiresAt], u.[SubscriptionExpiresAt]) AS [SubscriptionExpiresAt],
           lk.[MaskedCode] AS [MaskedLicenseCode],
           u.[CreatedAt], u.[UpdatedAt]
    FROM [dbo].[UserAuth] u
    LEFT JOIN [dbo].[LicenseKey] lk ON lk.[Id] = u.[ActiveLicenseKeyId]
    WHERE u.[Id] = @Id;
END
GO

CREATE OR ALTER PROCEDURE [dbo].[spUserAuth_GetByEmail]
    @Email NVARCHAR(320)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT u.[Id], u.[Email], u.[Otp], u.[IsAuthenticated], u.[OtpExpiresAt], u.[PasswordHash], u.[IsOtpVerified],
           u.[RefreshToken], u.[RefreshTokenExpiryTime],
           CAST(CASE WHEN u.[PasswordHash] IS NOT NULL AND LEN(u.[PasswordHash]) > 0 THEN 1 ELSE 0 END AS BIT) AS [HasPassword],
           CASE
               WHEN lk.[Id] IS NOT NULL THEN
                   CASE
                       WHEN lk.[IsRevoked] = 1
                         OR (COALESCE(lk.[ExpiresAt], u.[SubscriptionExpiresAt]) IS NOT NULL
                             AND COALESCE(lk.[ExpiresAt], u.[SubscriptionExpiresAt]) <= SYSUTCDATETIME())
                       THEN N'community'
                       ELSE COALESCE(NULLIF(LTRIM(RTRIM(u.[SubscriptionTier])), N''), N'community')
                   END
               WHEN u.[SubscriptionExpiresAt] IS NOT NULL AND u.[SubscriptionExpiresAt] <= SYSUTCDATETIME()
                   THEN N'community'
               ELSE COALESCE(NULLIF(LTRIM(RTRIM(u.[SubscriptionTier])), N''), N'community')
           END AS [SubscriptionTier],
           COALESCE(lk.[ExpiresAt], u.[SubscriptionExpiresAt]) AS [SubscriptionExpiresAt],
           lk.[MaskedCode] AS [MaskedLicenseCode],
           u.[CreatedAt], u.[UpdatedAt]
    FROM [dbo].[UserAuth] u
    LEFT JOIN [dbo].[LicenseKey] lk ON lk.[Id] = u.[ActiveLicenseKeyId]
    WHERE u.[Email] = @Email;
END
GO

CREATE OR ALTER PROCEDURE [dbo].[spUserAuth_GetById]
    @Id UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT u.[Id], u.[Email], u.[Otp], u.[IsAuthenticated], u.[OtpExpiresAt], u.[PasswordHash], u.[IsOtpVerified],
           u.[RefreshToken], u.[RefreshTokenExpiryTime],
           CAST(CASE WHEN u.[PasswordHash] IS NOT NULL AND LEN(u.[PasswordHash]) > 0 THEN 1 ELSE 0 END AS BIT) AS [HasPassword],
           CASE
               WHEN lk.[Id] IS NOT NULL THEN
                   CASE
                       WHEN lk.[IsRevoked] = 1
                         OR (COALESCE(lk.[ExpiresAt], u.[SubscriptionExpiresAt]) IS NOT NULL
                             AND COALESCE(lk.[ExpiresAt], u.[SubscriptionExpiresAt]) <= SYSUTCDATETIME())
                       THEN N'community'
                       ELSE COALESCE(NULLIF(LTRIM(RTRIM(u.[SubscriptionTier])), N''), N'community')
                   END
               WHEN u.[SubscriptionExpiresAt] IS NOT NULL AND u.[SubscriptionExpiresAt] <= SYSUTCDATETIME()
                   THEN N'community'
               ELSE COALESCE(NULLIF(LTRIM(RTRIM(u.[SubscriptionTier])), N''), N'community')
           END AS [SubscriptionTier],
           COALESCE(lk.[ExpiresAt], u.[SubscriptionExpiresAt]) AS [SubscriptionExpiresAt],
           lk.[MaskedCode] AS [MaskedLicenseCode],
           u.[CreatedAt], u.[UpdatedAt]
    FROM [dbo].[UserAuth] u
    LEFT JOIN [dbo].[LicenseKey] lk ON lk.[Id] = u.[ActiveLicenseKeyId]
    WHERE u.[Id] = @Id;
END
GO

CREATE OR ALTER PROCEDURE [dbo].[spUserAuth_GetByRefreshToken]
    @RefreshToken NVARCHAR(256)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT u.[Id], u.[Email], u.[IsAuthenticated], u.[PasswordHash], u.[IsOtpVerified],
           u.[RefreshToken], u.[RefreshTokenExpiryTime],
           CAST(CASE WHEN u.[PasswordHash] IS NOT NULL AND LEN(u.[PasswordHash]) > 0 THEN 1 ELSE 0 END AS BIT) AS [HasPassword],
           CASE
               WHEN lk.[Id] IS NOT NULL THEN
                   CASE
                       WHEN lk.[IsRevoked] = 1
                         OR (COALESCE(lk.[ExpiresAt], u.[SubscriptionExpiresAt]) IS NOT NULL
                             AND COALESCE(lk.[ExpiresAt], u.[SubscriptionExpiresAt]) <= SYSUTCDATETIME())
                       THEN N'community'
                       ELSE COALESCE(NULLIF(LTRIM(RTRIM(u.[SubscriptionTier])), N''), N'community')
                   END
               WHEN u.[SubscriptionExpiresAt] IS NOT NULL AND u.[SubscriptionExpiresAt] <= SYSUTCDATETIME()
                   THEN N'community'
               ELSE COALESCE(NULLIF(LTRIM(RTRIM(u.[SubscriptionTier])), N''), N'community')
           END AS [SubscriptionTier],
           COALESCE(lk.[ExpiresAt], u.[SubscriptionExpiresAt]) AS [SubscriptionExpiresAt],
           lk.[MaskedCode] AS [MaskedLicenseCode],
           u.[CreatedAt], u.[UpdatedAt]
    FROM [dbo].[UserAuth] u
    LEFT JOIN [dbo].[LicenseKey] lk ON lk.[Id] = u.[ActiveLicenseKeyId]
    WHERE u.[RefreshToken] = @RefreshToken;
END
GO

CREATE OR ALTER PROCEDURE [dbo].[spUserAuth_Register]
    @Email NVARCHAR(320)
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM [dbo].[UserAuth] WHERE [Email] = @Email)
    BEGIN
        INSERT INTO [dbo].[UserAuth] ([Email])
        VALUES (@Email);
    END

    SELECT u.[Id], u.[Email], u.[Otp], u.[IsAuthenticated], u.[OtpExpiresAt], u.[PasswordHash], u.[IsOtpVerified],
           CAST(CASE WHEN u.[PasswordHash] IS NOT NULL AND LEN(u.[PasswordHash]) > 0 THEN 1 ELSE 0 END AS BIT) AS [HasPassword],
           CASE
               WHEN lk.[Id] IS NOT NULL THEN
                   CASE
                       WHEN lk.[IsRevoked] = 1
                         OR (COALESCE(lk.[ExpiresAt], u.[SubscriptionExpiresAt]) IS NOT NULL
                             AND COALESCE(lk.[ExpiresAt], u.[SubscriptionExpiresAt]) <= SYSUTCDATETIME())
                       THEN N'community'
                       ELSE COALESCE(NULLIF(LTRIM(RTRIM(u.[SubscriptionTier])), N''), N'community')
                   END
               WHEN u.[SubscriptionExpiresAt] IS NOT NULL AND u.[SubscriptionExpiresAt] <= SYSUTCDATETIME()
                   THEN N'community'
               ELSE COALESCE(NULLIF(LTRIM(RTRIM(u.[SubscriptionTier])), N''), N'community')
           END AS [SubscriptionTier],
           COALESCE(lk.[ExpiresAt], u.[SubscriptionExpiresAt]) AS [SubscriptionExpiresAt],
           lk.[MaskedCode] AS [MaskedLicenseCode],
           u.[CreatedAt], u.[UpdatedAt]
    FROM [dbo].[UserAuth] u
    LEFT JOIN [dbo].[LicenseKey] lk ON lk.[Id] = u.[ActiveLicenseKeyId]
    WHERE u.[Email] = @Email;
END
GO

CREATE OR ALTER PROCEDURE [dbo].[spUserAuth_SetAuthenticated]
    @Id UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE [dbo].[UserAuth]
    SET [IsAuthenticated] = 1,
        [UpdatedAt]       = SYSUTCDATETIME()
    WHERE [Id] = @Id;

    SELECT u.[Id], u.[Email], u.[IsAuthenticated], u.[PasswordHash], u.[IsOtpVerified],
           CAST(CASE WHEN u.[PasswordHash] IS NOT NULL AND LEN(u.[PasswordHash]) > 0 THEN 1 ELSE 0 END AS BIT) AS [HasPassword],
           CASE
               WHEN lk.[Id] IS NOT NULL THEN
                   CASE
                       WHEN lk.[IsRevoked] = 1
                         OR (COALESCE(lk.[ExpiresAt], u.[SubscriptionExpiresAt]) IS NOT NULL
                             AND COALESCE(lk.[ExpiresAt], u.[SubscriptionExpiresAt]) <= SYSUTCDATETIME())
                       THEN N'community'
                       ELSE COALESCE(NULLIF(LTRIM(RTRIM(u.[SubscriptionTier])), N''), N'community')
                   END
               WHEN u.[SubscriptionExpiresAt] IS NOT NULL AND u.[SubscriptionExpiresAt] <= SYSUTCDATETIME()
                   THEN N'community'
               ELSE COALESCE(NULLIF(LTRIM(RTRIM(u.[SubscriptionTier])), N''), N'community')
           END AS [SubscriptionTier],
           COALESCE(lk.[ExpiresAt], u.[SubscriptionExpiresAt]) AS [SubscriptionExpiresAt],
           lk.[MaskedCode] AS [MaskedLicenseCode],
           u.[CreatedAt], u.[UpdatedAt]
    FROM [dbo].[UserAuth] u
    LEFT JOIN [dbo].[LicenseKey] lk ON lk.[Id] = u.[ActiveLicenseKeyId]
    WHERE u.[Id] = @Id;
END
GO

CREATE OR ALTER PROCEDURE [dbo].[spUserAuth_SetPassword]
    @Email          NVARCHAR(320),
    @PasswordHash   NVARCHAR(512)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @IsOtpVerified BIT;
    SELECT @IsOtpVerified = [IsOtpVerified] FROM [dbo].[UserAuth] WHERE [Email] = @Email;

    IF @IsOtpVerified IS NULL
    BEGIN
        RAISERROR('User not found.', 16, 1);
        RETURN;
    END

    IF @IsOtpVerified = 0
    BEGIN
        RAISERROR('Email OTP verification is required before setting a password.', 16, 1);
        RETURN;
    END

    UPDATE [dbo].[UserAuth]
    SET [PasswordHash]       = @PasswordHash,
        [IsOtpVerified]      = 0,
        [IsAuthenticated]    = 1,
        [EmailOtpVerifiedAt] = COALESCE([EmailOtpVerifiedAt], SYSUTCDATETIME()),
        [Otp]                = NULL,
        [OtpExpiresAt]       = NULL,
        [UpdatedAt]          = SYSUTCDATETIME()
    WHERE [Email] = @Email;

    SELECT u.[Id], u.[Email], u.[IsAuthenticated], u.[PasswordHash], u.[IsOtpVerified],
           CAST(1 AS BIT) AS [HasPassword],
           CASE
               WHEN lk.[Id] IS NOT NULL THEN
                   CASE
                       WHEN lk.[IsRevoked] = 1
                         OR (COALESCE(lk.[ExpiresAt], u.[SubscriptionExpiresAt]) IS NOT NULL
                             AND COALESCE(lk.[ExpiresAt], u.[SubscriptionExpiresAt]) <= SYSUTCDATETIME())
                       THEN N'community'
                       ELSE COALESCE(NULLIF(LTRIM(RTRIM(u.[SubscriptionTier])), N''), N'community')
                   END
               WHEN u.[SubscriptionExpiresAt] IS NOT NULL AND u.[SubscriptionExpiresAt] <= SYSUTCDATETIME()
                   THEN N'community'
               ELSE COALESCE(NULLIF(LTRIM(RTRIM(u.[SubscriptionTier])), N''), N'community')
           END AS [SubscriptionTier],
           COALESCE(lk.[ExpiresAt], u.[SubscriptionExpiresAt]) AS [SubscriptionExpiresAt],
           lk.[MaskedCode] AS [MaskedLicenseCode],
           u.[CreatedAt], u.[UpdatedAt]
    FROM [dbo].[UserAuth] u
    LEFT JOIN [dbo].[LicenseKey] lk ON lk.[Id] = u.[ActiveLicenseKeyId]
    WHERE u.[Email] = @Email;
END
GO

CREATE OR ALTER PROCEDURE [dbo].[spUserAuth_VerifyOtp]
    @Email          NVARCHAR(320),
    @Otp            NVARCHAR(10)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @StoredOtp      NVARCHAR(10);
    DECLARE @OtpExpiresAt   DATETIME2(7);

    SELECT @StoredOtp = [Otp], @OtpExpiresAt = [OtpExpiresAt]
    FROM [dbo].[UserAuth]
    WHERE [Email] = @Email;

    IF @StoredOtp IS NULL
    BEGIN
        RAISERROR('User not found or OTP not generated.', 16, 1);
        RETURN;
    END

    IF @StoredOtp <> @Otp
    BEGIN
        RAISERROR('Invalid OTP.', 16, 1);
        RETURN;
    END

    IF @OtpExpiresAt < SYSUTCDATETIME()
    BEGIN
        RAISERROR('OTP has expired.', 16, 1);
        RETURN;
    END

    UPDATE [dbo].[UserAuth]
    SET [IsOtpVerified]      = 1,
        [IsAuthenticated]    = CASE WHEN [PasswordHash] IS NOT NULL AND LEN([PasswordHash]) > 0 THEN 1 ELSE 0 END,
        [EmailOtpVerifiedAt] = COALESCE([EmailOtpVerifiedAt], SYSUTCDATETIME()),
        [Otp]                = NULL,
        [OtpExpiresAt]       = NULL,
        [UpdatedAt]          = SYSUTCDATETIME()
    WHERE [Email] = @Email;

    SELECT u.[Id], u.[Email], u.[IsAuthenticated], u.[PasswordHash], u.[IsOtpVerified],
           CAST(CASE WHEN u.[PasswordHash] IS NOT NULL AND LEN(u.[PasswordHash]) > 0 THEN 1 ELSE 0 END AS BIT) AS [HasPassword],
           CASE
               WHEN lk.[Id] IS NOT NULL THEN
                   CASE
                       WHEN lk.[IsRevoked] = 1
                         OR (COALESCE(lk.[ExpiresAt], u.[SubscriptionExpiresAt]) IS NOT NULL
                             AND COALESCE(lk.[ExpiresAt], u.[SubscriptionExpiresAt]) <= SYSUTCDATETIME())
                       THEN N'community'
                       ELSE COALESCE(NULLIF(LTRIM(RTRIM(u.[SubscriptionTier])), N''), N'community')
                   END
               WHEN u.[SubscriptionExpiresAt] IS NOT NULL AND u.[SubscriptionExpiresAt] <= SYSUTCDATETIME()
                   THEN N'community'
               ELSE COALESCE(NULLIF(LTRIM(RTRIM(u.[SubscriptionTier])), N''), N'community')
           END AS [SubscriptionTier],
           COALESCE(lk.[ExpiresAt], u.[SubscriptionExpiresAt]) AS [SubscriptionExpiresAt],
           lk.[MaskedCode] AS [MaskedLicenseCode],
           u.[CreatedAt], u.[UpdatedAt]
    FROM [dbo].[UserAuth] u
    LEFT JOIN [dbo].[LicenseKey] lk ON lk.[Id] = u.[ActiveLicenseKeyId]
    WHERE u.[Email] = @Email;
END
GO

CREATE OR ALTER PROCEDURE [dbo].[spLicense_Create]
    @Id             UNIQUEIDENTIFIER,
    @LicenseKey     NVARCHAR(128) = NULL,
    @CodeHash       NVARCHAR(64) = NULL,
    @MaskedCode     NVARCHAR(64) = NULL,
    @Tier           NVARCHAR(32),
    @LicenseType    NVARCHAR(32) = N'enterprise',
    @IndividualType NVARCHAR(32) = NULL,
    @BillingCycle   NVARCHAR(32) = N'annual',
    @DurationDays   INT = NULL,
    @MaxActivations INT = 1,
    @AllowedEmail   NVARCHAR(320) = NULL,
    @Notes          NVARCHAR(500) = NULL,
    @ExpiresAt      DATETIME2(7) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @CleanTier NVARCHAR(32) = LOWER(LTRIM(RTRIM(@Tier)));
    DECLARE @CleanLicType NVARCHAR(32) = LOWER(LTRIM(RTRIM(COALESCE(
        NULLIF(@LicenseType, N''),
        CASE WHEN @CleanTier = N'enterprise' THEN N'enterprise' ELSE N'individual' END))));
    IF @CleanTier = N'professional' AND @CleanLicType = N'professional'
        SET @CleanLicType = N'individual';
    DECLARE @CleanIndType NVARCHAR(32) = NULLIF(LOWER(LTRIM(RTRIM(COALESCE(@IndividualType, N'')))), N'');
    IF @CleanIndType IS NULL AND (@CleanTier = N'professional' OR @CleanLicType = N'individual')
        SET @CleanIndType = N'personal';
    DECLARE @IsPersonal BIT = CASE
        WHEN @CleanIndType = N'personal' THEN 1
        WHEN @CleanLicType = N'individual' AND @CleanIndType IS NULL THEN 1
        WHEN @CleanTier = N'professional' AND COALESCE(@CleanIndType, N'') <> N'organization' THEN 1
        ELSE 0 END;

    IF @IsPersonal = 1 AND (@CleanTier = N'enterprise' OR @CleanLicType = N'enterprise')
    BEGIN
        RAISERROR('Personal individual licenses cannot be issued for Enterprise tier.', 16, 1);
        RETURN;
    END

    INSERT INTO [dbo].[LicenseKey] (
        [Id], [LicenseKey], [CodeHash], [MaskedCode], [Tier], [LicenseType], [IndividualType],
        [BillingCycle], [DurationDays], [MaxActivations], [ActiveActivations],
        [AllowedEmail], [Notes], [IsRevoked], [ExpiresAt],
        [CreatedAt], [UpdatedAt]
    )
    VALUES (
        @Id, @LicenseKey, @CodeHash, @MaskedCode, LOWER(LTRIM(RTRIM(@Tier))), @CleanLicType, @CleanIndType,
        LOWER(LTRIM(RTRIM(@BillingCycle))), @DurationDays,
        CASE WHEN @IsPersonal = 1 THEN 1 ELSE @MaxActivations END, 0,
        NULLIF(LOWER(LTRIM(RTRIM(COALESCE(@AllowedEmail, N'')))), N''), @Notes, 0, @ExpiresAt,
        SYSUTCDATETIME(), SYSUTCDATETIME()
    );

    SELECT [Id], [LicenseKey], [CodeHash], [MaskedCode], [Tier], [LicenseType], [IndividualType],
           [BillingCycle], [DurationDays], [MaxActivations], [ActiveActivations],
           [AllowedEmail], [Notes], [IsRevoked], [ExpiresAt],
           [CreatedAt], [UpdatedAt]
    FROM [dbo].[LicenseKey]
    WHERE [Id] = @Id;
END
GO

CREATE OR ALTER PROCEDURE [dbo].[spLicense_GetStatus]
    @UserId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @UserEmail NVARCHAR(320);
    DECLARE @EmailOtpVerifiedAt DATETIME2(7);
    DECLARE @RawTier NVARCHAR(32);
    DECLARE @UserExpiresAt DATETIME2(7);
    DECLARE @KeyId UNIQUEIDENTIFIER;
    DECLARE @KeyTier NVARCHAR(32);
    DECLARE @LicenseType NVARCHAR(32);
    DECLARE @IndividualType NVARCHAR(32);
    DECLARE @AllowedEmail NVARCHAR(320);
    DECLARE @BillingCycle NVARCHAR(32);
    DECLARE @MaskedCode NVARCHAR(64);
    DECLARE @KeyExpiresAt DATETIME2(7);
    DECLARE @ActiveActivations INT;
    DECLARE @MaxActivations INT;
    DECLARE @IsRevoked BIT = 0;
    DECLARE @ActivatedAt DATETIME2(7);
    DECLARE @AccessSource NVARCHAR(32) = N'free';

    SELECT
        @UserEmail = LOWER(LTRIM(RTRIM(COALESCE(u.[Email], N'')))),
        @EmailOtpVerifiedAt = u.[EmailOtpVerifiedAt],
        @RawTier = COALESCE(NULLIF(LOWER(LTRIM(RTRIM(u.[SubscriptionTier]))), N''), N'community'),
        @UserExpiresAt = u.[SubscriptionExpiresAt],
        @KeyId = lk.[Id],
        @KeyTier = NULLIF(LOWER(LTRIM(RTRIM(COALESCE(lk.[Tier], N'')))), N''),
        @LicenseType = NULLIF(LOWER(LTRIM(RTRIM(COALESCE(lk.[LicenseType], N'')))), N''),
        @IndividualType = NULLIF(LOWER(LTRIM(RTRIM(COALESCE(lk.[IndividualType], N'')))), N''),
        @AllowedEmail = NULLIF(LOWER(LTRIM(RTRIM(COALESCE(lk.[AllowedEmail], N'')))), N''),
        @BillingCycle = lk.[BillingCycle],
        @MaskedCode = lk.[MaskedCode],
        @KeyExpiresAt = lk.[ExpiresAt],
        @ActiveActivations = lk.[ActiveActivations],
        @MaxActivations = lk.[MaxActivations],
        @IsRevoked = COALESCE(lk.[IsRevoked], 0)
    FROM [dbo].[UserAuth] u
    LEFT JOIN [dbo].[LicenseKey] lk ON lk.[Id] = u.[ActiveLicenseKeyId]
    WHERE u.[Id] = @UserId;

    DECLARE @IsEmailOtpVerified BIT = CASE WHEN @EmailOtpVerifiedAt IS NOT NULL THEN 1 ELSE 0 END;
    DECLARE @AtPos INT = CHARINDEX(N'@', COALESCE(@UserEmail, N''));
    DECLARE @UserDomain NVARCHAR(320) = CASE WHEN @AtPos > 0 THEN SUBSTRING(@UserEmail, @AtPos + 1, 320) ELSE N'' END;

    DECLARE @IsAllowedEmailValid BIT = 1;
    DECLARE @IsCurrentKeyDomainPattern BIT = 0;
    IF @KeyId IS NOT NULL AND @AllowedEmail IS NOT NULL
    BEGIN
        DECLARE @CurNormPattern NVARCHAR(320) = CASE
            WHEN LEFT(@AllowedEmail, 2) = N'*@' THEN SUBSTRING(@AllowedEmail, 2, 320)
            ELSE @AllowedEmail END;
        DECLARE @CurPatAt INT = CHARINDEX(N'@', @CurNormPattern);
        IF @CurPatAt <= 1
        BEGIN
            SET @IsCurrentKeyDomainPattern = 1;
            DECLARE @CurDom NVARCHAR(320) = SUBSTRING(@CurNormPattern, CASE WHEN LEFT(@CurNormPattern, 1) IN (N'@', N'.') THEN 2 ELSE 1 END, 320);
            IF LEFT(@CurDom, 1) = N'.' SET @CurDom = SUBSTRING(@CurDom, 2, 320);
            IF @UserDomain = N'' OR @CurDom = N''
               OR (@UserDomain <> @CurDom AND RIGHT(@UserDomain, LEN(N'.' + @CurDom)) <> N'.' + @CurDom)
                SET @IsAllowedEmailValid = 0;
        END
        ELSE IF @UserEmail <> @CurNormPattern
        BEGIN
            SET @IsAllowedEmailValid = 0;
        END
    END

    DECLARE @CurrentEffectiveExp DATETIME2(7) = COALESCE(@KeyExpiresAt, @UserExpiresAt);
    DECLARE @CurrentNeedsResolve BIT = CASE
        WHEN @KeyId IS NULL THEN 1
        WHEN @IsRevoked = 1 OR @IsAllowedEmailValid = 0 THEN 1
        WHEN @CurrentEffectiveExp IS NOT NULL AND @CurrentEffectiveExp <= SYSUTCDATETIME() THEN 1
        ELSE 0 END;

    IF @IsEmailOtpVerified = 1 AND @CurrentNeedsResolve = 1 AND @UserDomain <> N''
    BEGIN
        DECLARE @MatchedDomainKeyId UNIQUEIDENTIFIER = NULL;
        SELECT TOP 1 @MatchedDomainKeyId = dl.[Id]
        FROM [dbo].[LicenseKey] dl
        WHERE dl.[IsRevoked] = 0
          AND (dl.[ExpiresAt] IS NULL OR dl.[ExpiresAt] > SYSUTCDATETIME())
          AND LOWER(LTRIM(RTRIM(COALESCE(dl.[IndividualType], N'')))) <> N'personal'
          AND LOWER(LTRIM(RTRIM(COALESCE(dl.[LicenseType], N'')))) <> N'individual'
          AND dl.[AllowedEmail] IS NOT NULL
          AND LTRIM(RTRIM(dl.[AllowedEmail])) <> N''
          AND CHARINDEX(N'@', CASE
                WHEN LEFT(LOWER(LTRIM(RTRIM(dl.[AllowedEmail]))), 2) = N'*@'
                THEN SUBSTRING(LOWER(LTRIM(RTRIM(dl.[AllowedEmail]))), 2, 320)
                ELSE LOWER(LTRIM(RTRIM(dl.[AllowedEmail])))
              END) <= 1
          AND (
              @UserDomain = SUBSTRING(REPLACE(REPLACE(LOWER(LTRIM(RTRIM(dl.[AllowedEmail]))), N'*@', N'@'), N'@', N''), CASE WHEN LEFT(REPLACE(REPLACE(LOWER(LTRIM(RTRIM(dl.[AllowedEmail]))), N'*@', N'@'), N'@', N''), 1) = N'.' THEN 2 ELSE 1 END, 320)
              OR RIGHT(@UserDomain, LEN(N'.' + SUBSTRING(REPLACE(REPLACE(LOWER(LTRIM(RTRIM(dl.[AllowedEmail]))), N'*@', N'@'), N'@', N''), CASE WHEN LEFT(REPLACE(REPLACE(LOWER(LTRIM(RTRIM(dl.[AllowedEmail]))), N'*@', N'@'), N'@', N''), 1) = N'.' THEN 2 ELSE 1 END, 320)))
                 = N'.' + SUBSTRING(REPLACE(REPLACE(LOWER(LTRIM(RTRIM(dl.[AllowedEmail]))), N'*@', N'@'), N'@', N''), CASE WHEN LEFT(REPLACE(REPLACE(LOWER(LTRIM(RTRIM(dl.[AllowedEmail]))), N'*@', N'@'), N'@', N''), 1) = N'.' THEN 2 ELSE 1 END, 320)
          )
          AND (
              EXISTS (SELECT 1 FROM [dbo].[LicenseActivation] a WHERE a.[LicenseKeyId] = dl.[Id] AND a.[UserId] = @UserId AND a.[IsActive] = 1)
              OR dl.[ActiveActivations] < dl.[MaxActivations]
          )
        ORDER BY CASE WHEN LOWER(dl.[Tier]) = N'enterprise' THEN 0 ELSE 1 END, dl.[CreatedAt] DESC;

        IF @MatchedDomainKeyId IS NOT NULL
        BEGIN
            DECLARE @DomTier NVARCHAR(32);
            DECLARE @DomDur INT;
            DECLARE @DomExp DATETIME2(7);
            SELECT
                @DomTier = LOWER(LTRIM(RTRIM(COALESCE([Tier], N'professional')))),
                @LicenseType = NULLIF(LOWER(LTRIM(RTRIM(COALESCE([LicenseType], N'')))), N''),
                @IndividualType = NULLIF(LOWER(LTRIM(RTRIM(COALESCE([IndividualType], N'')))), N''),
                @AllowedEmail = NULLIF(LOWER(LTRIM(RTRIM(COALESCE([AllowedEmail], N'')))), N''),
                @BillingCycle = [BillingCycle],
                @MaskedCode = [MaskedCode],
                @DomDur = [DurationDays],
                @DomExp = [ExpiresAt],
                @MaxActivations = [MaxActivations],
                @IsRevoked = [IsRevoked]
            FROM [dbo].[LicenseKey]
            WHERE [Id] = @MatchedDomainKeyId;

            IF @DomExp IS NULL AND @DomDur IS NOT NULL AND @DomDur > 0
            BEGIN
                SET @DomExp = DATEADD(DAY, @DomDur, SYSUTCDATETIME());
                UPDATE [dbo].[LicenseKey] SET [ExpiresAt] = @DomExp, [UpdatedAt] = SYSUTCDATETIME() WHERE [Id] = @MatchedDomainKeyId;
            END

            IF NOT EXISTS (SELECT 1 FROM [dbo].[LicenseActivation] WHERE [LicenseKeyId] = @MatchedDomainKeyId AND [UserId] = @UserId AND [IsActive] = 1)
            BEGIN
                INSERT INTO [dbo].[LicenseActivation] ([Id], [LicenseKeyId], [UserId], [DeviceLabel], [IsActive], [ActivatedAt], [LastVerifiedAt])
                VALUES (NEWID(), @MatchedDomainKeyId, @UserId, N'Verified Domain Auto-Seat', 1, SYSUTCDATETIME(), SYSUTCDATETIME());
            END

            UPDATE [dbo].[LicenseKey]
            SET [ActiveActivations] = (SELECT COUNT(1) FROM [dbo].[LicenseActivation] WHERE [LicenseKeyId] = @MatchedDomainKeyId AND [IsActive] = 1),
                [UpdatedAt] = SYSUTCDATETIME()
            WHERE [Id] = @MatchedDomainKeyId;

            UPDATE [dbo].[UserAuth]
            SET [SubscriptionTier] = @DomTier,
                [SubscriptionExpiresAt] = @DomExp,
                [ActiveLicenseKeyId] = @MatchedDomainKeyId,
                [UpdatedAt] = SYSUTCDATETIME()
            WHERE [Id] = @UserId;

            SELECT @ActiveActivations = [ActiveActivations] FROM [dbo].[LicenseKey] WHERE [Id] = @MatchedDomainKeyId;
            SET @KeyId = @MatchedDomainKeyId;
            SET @KeyTier = @DomTier;
            SET @RawTier = @DomTier;
            SET @KeyExpiresAt = @DomExp;
            SET @UserExpiresAt = @DomExp;
            SET @IsAllowedEmailValid = 1;
            SET @IsCurrentKeyDomainPattern = 1;
            SET @IsRevoked = 0;
        END
    END

    IF @KeyId IS NOT NULL
    BEGIN
        SELECT TOP 1 @ActivatedAt = a.[ActivatedAt]
        FROM [dbo].[LicenseActivation] a
        WHERE a.[UserId] = @UserId AND a.[LicenseKeyId] = @KeyId AND a.[IsActive] = 1
        ORDER BY a.[ActivatedAt] DESC;

        IF @ActivatedAt IS NOT NULL
        BEGIN
            UPDATE [dbo].[LicenseActivation]
            SET [LastVerifiedAt] = SYSUTCDATETIME()
            WHERE [UserId] = @UserId AND [LicenseKeyId] = @KeyId AND [IsActive] = 1;
        END
    END

    DECLARE @EffectiveExpiresAt DATETIME2(7) = COALESCE(@KeyExpiresAt, @UserExpiresAt);
    DECLARE @IsExpired BIT = CASE WHEN @EffectiveExpiresAt IS NOT NULL AND @EffectiveExpiresAt <= SYSUTCDATETIME() THEN 1 ELSE 0 END;
    DECLARE @CandidateTier NVARCHAR(32) = COALESCE(@KeyTier, @RawTier, N'community');
    DECLARE @IsPersonalLicense BIT = CASE
        WHEN @IndividualType = N'personal' THEN 1
        WHEN @LicenseType = N'individual' AND @IndividualType IS NULL THEN 1
        WHEN @CandidateTier = N'professional' AND COALESCE(@IndividualType, N'') <> N'organization' THEN 1
        ELSE 0 END;

    DECLARE @EffectiveTier NVARCHAR(32) = CASE
        WHEN @IsEmailOtpVerified = 0 THEN N'community'
        WHEN @IsRevoked = 1 OR @IsExpired = 1 OR @IsAllowedEmailValid = 0 THEN N'community'
        WHEN @IsPersonalLicense = 1 AND (@KeyTier = N'enterprise' OR @LicenseType = N'enterprise') THEN N'community'
        WHEN @IsPersonalLicense = 1 AND @CandidateTier = N'enterprise' THEN N'professional'
        ELSE @CandidateTier
    END;

    IF @EffectiveTier = N'professional' AND @IsPersonalLicense = 1
    BEGIN
        SET @LicenseType = COALESCE(@LicenseType, N'individual');
        SET @IndividualType = COALESCE(@IndividualType, N'personal');
    END

    DECLARE @Status NVARCHAR(32) = CASE
        WHEN @IsRevoked = 1 THEN N'revoked'
        WHEN @IsExpired = 1 THEN N'expired'
        WHEN @EffectiveTier IN (N'professional', N'enterprise') THEN N'active'
        ELSE N'free'
    END;

    DECLARE @IsActive BIT = CASE WHEN @EffectiveTier IN (N'professional', N'enterprise') AND @Status = N'active' THEN 1 ELSE 0 END;
    IF @IsActive = 1
    BEGIN
        SET @AccessSource = CASE WHEN @IsCurrentKeyDomainPattern = 1 THEN N'domain' ELSE N'key' END;
    END

    SELECT
        @EffectiveTier AS [SubscriptionTier],
        @Status AS [Status],
        @LicenseType AS [LicenseType],
        @IndividualType AS [IndividualType],
        @IsPersonalLicense AS [IsPersonalLicense],
        @IsEmailOtpVerified AS [IsEmailOtpVerified],
        @AccessSource AS [AccessSource],
        @AllowedEmail AS [LicensedEmailPattern],
        @BillingCycle AS [BillingCycle],
        @MaskedCode AS [MaskedCode],
        @ActivatedAt AS [ActivatedAt],
        @EffectiveExpiresAt AS [ExpiresAt],
        @ActiveActivations AS [ActiveActivations],
        @MaxActivations AS [MaxActivations],
        @IsActive AS [IsActive],
        @IsExpired AS [IsExpired];
END
GO

CREATE OR ALTER PROCEDURE [dbo].[spLicense_Activate]
    @UserId         UNIQUEIDENTIFIER,
    @UserEmail      NVARCHAR(320),
    @CodeHash       NVARCHAR(64),
    @NormalizedCode NVARCHAR(128) = NULL,
    @DeviceLabel    NVARCHAR(150) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @UserOtpVerifiedAt DATETIME2(7);
    SELECT @UserOtpVerifiedAt = [EmailOtpVerifiedAt]
    FROM [dbo].[UserAuth]
    WHERE [Id] = @UserId;

    IF NOT EXISTS (SELECT 1 FROM [dbo].[UserAuth] WHERE [Id] = @UserId)
    BEGIN
        RAISERROR('Authenticated user account not found.', 16, 1);
        RETURN;
    END

    IF @UserOtpVerifiedAt IS NULL
    BEGIN
        RAISERROR('Your email address must be verified via OTP before activating or using a license.', 16, 1);
        RETURN;
    END

    BEGIN TRANSACTION;

    DECLARE @KeyId UNIQUEIDENTIFIER;
    DECLARE @Tier NVARCHAR(32);
    DECLARE @LicenseType NVARCHAR(32);
    DECLARE @IndividualType NVARCHAR(32);
    DECLARE @BillingCycle NVARCHAR(32);
    DECLARE @DurationDays INT;
    DECLARE @MaxActivations INT;
    DECLARE @AllowedEmail NVARCHAR(320);
    DECLARE @IsRevoked BIT;
    DECLARE @KeyExpiresAt DATETIME2(7);
    DECLARE @PreviousKeyId UNIQUEIDENTIFIER;
    DECLARE @StoredRawKey NVARCHAR(128);

    SELECT TOP 1
        @KeyId = lk.[Id],
        @Tier = LOWER(LTRIM(RTRIM(COALESCE(lk.[Tier], N'professional')))),
        @LicenseType = NULLIF(LOWER(LTRIM(RTRIM(COALESCE(lk.[LicenseType], N'')))), N''),
        @IndividualType = NULLIF(LOWER(LTRIM(RTRIM(COALESCE(lk.[IndividualType], N'')))), N''),
        @BillingCycle = lk.[BillingCycle],
        @DurationDays = lk.[DurationDays],
        @MaxActivations = lk.[MaxActivations],
        @AllowedEmail = lk.[AllowedEmail],
        @IsRevoked = lk.[IsRevoked],
        @KeyExpiresAt = lk.[ExpiresAt],
        @StoredRawKey = UPPER(REPLACE(LTRIM(RTRIM(COALESCE(lk.[LicenseKey], N''))), N' ', N''))
    FROM [dbo].[LicenseKey] lk WITH (UPDLOCK, HOLDLOCK)
    WHERE lk.[CodeHash] = @CodeHash
       OR (@NormalizedCode IS NOT NULL AND UPPER(REPLACE(LTRIM(RTRIM(COALESCE(lk.[LicenseKey], N''))), N' ', N'')) = @NormalizedCode);

    IF @KeyId IS NULL
    BEGIN
        ROLLBACK TRANSACTION;
        RAISERROR('Invalid registration code. Please verify the key and try again.', 16, 1);
        RETURN;
    END

    IF @IsRevoked = 1
    BEGIN
        ROLLBACK TRANSACTION;
        RAISERROR('This registration code has been revoked.', 16, 1);
        RETURN;
    END

    DECLARE @IsPersonal BIT = CASE
        WHEN @IndividualType = N'personal' THEN 1
        WHEN @LicenseType = N'individual' AND @IndividualType IS NULL THEN 1
        WHEN @Tier = N'professional' AND COALESCE(@IndividualType, N'') <> N'organization' THEN 1
        ELSE 0 END;

    IF @IsPersonal = 1 AND (@Tier = N'enterprise' OR @LicenseType = N'enterprise' OR LEFT(COALESCE(@NormalizedCode, @StoredRawKey, N''), 8) = N'ONST-ENT')
    BEGIN
        ROLLBACK TRANSACTION;
        RAISERROR('Personal individual licenses cannot be used to bypass Enterprise access.', 16, 1);
        RETURN;
    END

    DECLARE @NormalizedUserEmail NVARCHAR(320) = LOWER(LTRIM(RTRIM(COALESCE(@UserEmail, N''))));
    DECLARE @UserAtPos INT = CHARINDEX(N'@', @NormalizedUserEmail);
    DECLARE @UserDomain NVARCHAR(320) = CASE WHEN @UserAtPos > 0 THEN SUBSTRING(@NormalizedUserEmail, @UserAtPos + 1, 320) ELSE N'' END;
    DECLARE @CleanAllowedEmail NVARCHAR(320) = LOWER(LTRIM(RTRIM(COALESCE(@AllowedEmail, N''))));

    IF @CleanAllowedEmail <> N''
    BEGIN
        DECLARE @NormPattern NVARCHAR(320) = CASE
            WHEN LEFT(@CleanAllowedEmail, 2) = N'*@' THEN SUBSTRING(@CleanAllowedEmail, 2, 320)
            ELSE @CleanAllowedEmail END;
        DECLARE @PatAt INT = CHARINDEX(N'@', @NormPattern);
        IF @PatAt <= 1
        BEGIN
            DECLARE @AllowedDom NVARCHAR(320) = SUBSTRING(@NormPattern, CASE WHEN LEFT(@NormPattern, 1) IN (N'@', N'.') THEN 2 ELSE 1 END, 320);
            IF LEFT(@AllowedDom, 1) = N'.' SET @AllowedDom = SUBSTRING(@AllowedDom, 2, 320);
            IF @UserDomain = N'' OR @AllowedDom = N''
               OR (@UserDomain <> @AllowedDom AND RIGHT(@UserDomain, LEN(N'.' + @AllowedDom)) <> N'.' + @AllowedDom)
            BEGIN
                ROLLBACK TRANSACTION;
                RAISERROR('This license key is restricted to an authorized organization email domain.', 16, 1);
                RETURN;
            END
        END
        ELSE IF @NormalizedUserEmail <> @NormPattern
        BEGIN
            ROLLBACK TRANSACTION;
            RAISERROR('This registration code is assigned to a different email address.', 16, 1);
            RETURN;
        END
    END

    DECLARE @ComputedExpiresAt DATETIME2(7) = @KeyExpiresAt;
    IF @ComputedExpiresAt IS NULL AND @DurationDays IS NOT NULL AND @DurationDays > 0
    BEGIN
        SET @ComputedExpiresAt = DATEADD(DAY, @DurationDays, SYSUTCDATETIME());
        UPDATE [dbo].[LicenseKey]
        SET [ExpiresAt] = @ComputedExpiresAt,
            [UpdatedAt] = SYSUTCDATETIME()
        WHERE [Id] = @KeyId;
    END

    IF @ComputedExpiresAt IS NOT NULL AND @ComputedExpiresAt <= SYSUTCDATETIME()
    BEGIN
        ROLLBACK TRANSACTION;
        RAISERROR('This registration code has expired.', 16, 1);
        RETURN;
    END

    SELECT @PreviousKeyId = [ActiveLicenseKeyId]
    FROM [dbo].[UserAuth] WITH (UPDLOCK)
    WHERE [Id] = @UserId;

    DECLARE @HasExistingSeat BIT = CASE
        WHEN EXISTS (
            SELECT 1 FROM [dbo].[LicenseActivation]
            WHERE [LicenseKeyId] = @KeyId AND [UserId] = @UserId AND [IsActive] = 1
        ) THEN 1 ELSE 0 END;

    IF @HasExistingSeat = 0
    BEGIN
        DECLARE @CurrentActiveSeats INT = (
            SELECT COUNT(1) FROM [dbo].[LicenseActivation]
            WHERE [LicenseKeyId] = @KeyId AND [IsActive] = 1
        );

        IF @CurrentActiveSeats >= @MaxActivations
        BEGIN
            ROLLBACK TRANSACTION;
            RAISERROR('All available seats for this registration code are already in use.', 16, 1);
            RETURN;
        END

        IF @PreviousKeyId IS NOT NULL AND @PreviousKeyId <> @KeyId
        BEGIN
            UPDATE [dbo].[LicenseActivation]
            SET [IsActive] = 0,
                [DeactivatedAt] = SYSUTCDATETIME()
            WHERE [UserId] = @UserId AND [LicenseKeyId] = @PreviousKeyId AND [IsActive] = 1;

            UPDATE [dbo].[LicenseKey]
            SET [ActiveActivations] = (
                    SELECT COUNT(1) FROM [dbo].[LicenseActivation]
                    WHERE [LicenseKeyId] = @PreviousKeyId AND [IsActive] = 1
                ),
                [UpdatedAt] = SYSUTCDATETIME()
            WHERE [Id] = @PreviousKeyId;
        END

        INSERT INTO [dbo].[LicenseActivation] (
            [Id], [LicenseKeyId], [UserId], [DeviceLabel], [IsActive], [ActivatedAt], [LastVerifiedAt]
        )
        VALUES (
            NEWID(), @KeyId, @UserId, @DeviceLabel, 1, SYSUTCDATETIME(), SYSUTCDATETIME()
        );

        UPDATE [dbo].[LicenseKey]
        SET [ActiveActivations] = (
                SELECT COUNT(1) FROM [dbo].[LicenseActivation]
                WHERE [LicenseKeyId] = @KeyId AND [IsActive] = 1
            ),
            [UpdatedAt] = SYSUTCDATETIME()
        WHERE [Id] = @KeyId;
    END
    ELSE
    BEGIN
        UPDATE [dbo].[LicenseActivation]
        SET [DeviceLabel] = COALESCE(@DeviceLabel, [DeviceLabel]),
            [LastVerifiedAt] = SYSUTCDATETIME()
        WHERE [LicenseKeyId] = @KeyId AND [UserId] = @UserId AND [IsActive] = 1;
    END

    UPDATE [dbo].[UserAuth]
    SET [SubscriptionTier] = @Tier,
        [SubscriptionExpiresAt] = @ComputedExpiresAt,
        [ActiveLicenseKeyId] = @KeyId,
        [UpdatedAt] = SYSUTCDATETIME()
    WHERE [Id] = @UserId;

    COMMIT TRANSACTION;

    EXEC [dbo].[spLicense_GetStatus] @UserId = @UserId;
END
GO

CREATE OR ALTER PROCEDURE [dbo].[spLicense_Deactivate]
    @UserId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRANSACTION;

    DECLARE @KeyId UNIQUEIDENTIFIER;
    SELECT @KeyId = [ActiveLicenseKeyId]
    FROM [dbo].[UserAuth] WITH (UPDLOCK)
    WHERE [Id] = @UserId;

    IF @KeyId IS NOT NULL
    BEGIN
        UPDATE [dbo].[LicenseActivation]
        SET [IsActive] = 0,
            [DeactivatedAt] = SYSUTCDATETIME()
        WHERE [UserId] = @UserId AND [LicenseKeyId] = @KeyId AND [IsActive] = 1;

        UPDATE [dbo].[LicenseKey]
        SET [ActiveActivations] = (
                SELECT COUNT(1) FROM [dbo].[LicenseActivation]
                WHERE [LicenseKeyId] = @KeyId AND [IsActive] = 1
            ),
            [UpdatedAt] = SYSUTCDATETIME()
        WHERE [Id] = @KeyId;
    END

    UPDATE [dbo].[UserAuth]
    SET [SubscriptionTier] = N'community',
        [SubscriptionExpiresAt] = NULL,
        [ActiveLicenseKeyId] = NULL,
        [UpdatedAt] = SYSUTCDATETIME()
    WHERE [Id] = @UserId;

    COMMIT TRANSACTION;

    EXEC [dbo].[spLicense_GetStatus] @UserId = @UserId;
END
GO

CREATE OR ALTER PROCEDURE [dbo].[spLicense_Revoke]
    @LicenseKeyId UNIQUEIDENTIFIER = NULL,
    @CodeHash     NVARCHAR(64) = NULL
AS
BEGIN
    SET XACT_ABORT ON;
    BEGIN TRANSACTION;

    DECLARE @TargetId UNIQUEIDENTIFIER;
    SELECT @TargetId = [Id]
    FROM [dbo].[LicenseKey]
    WHERE (@LicenseKeyId IS NOT NULL AND [Id] = @LicenseKeyId)
       OR (@CodeHash IS NOT NULL AND [CodeHash] = @CodeHash);

    IF @TargetId IS NOT NULL
    BEGIN
        UPDATE [dbo].[LicenseKey]
        SET [IsRevoked] = 1,
            [RevokedAt] = SYSUTCDATETIME(),
            [ActiveActivations] = 0,
            [UpdatedAt] = SYSUTCDATETIME()
        WHERE [Id] = @TargetId;

        UPDATE [dbo].[LicenseActivation]
        SET [IsActive] = 0,
            [DeactivatedAt] = SYSUTCDATETIME()
        WHERE [LicenseKeyId] = @TargetId AND [IsActive] = 1;

        UPDATE [dbo].[UserAuth]
        SET [SubscriptionTier] = N'community',
            [UpdatedAt] = SYSUTCDATETIME()
        WHERE [ActiveLicenseKeyId] = @TargetId;
    END

    COMMIT TRANSACTION;
END
GO
