using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OnSteroidsApiUser.Domain.Models.AppSettingsModels;

namespace OnSteroidsApiUser.Infrastructure.Dapper;

/// <summary>
/// Idempotently aligns SQL Server columns and stored procedures on application startup
/// so new frontend fields persist immediately when the configured DB login permits DDL.
/// </summary>
public static class SqlSchemaInitializer
{
    private static readonly string[] UpgradeBatches =
    [
        """
        IF OBJECT_ID(N'dbo.Capsule', N'U') IS NOT NULL
        BEGIN
            IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'LockMode' AND Object_ID = OBJECT_ID(N'dbo.Capsule'))
                ALTER TABLE [dbo].[Capsule] ADD [LockMode] NVARCHAR(50) NULL;
            IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'BrdSectionsJson' AND Object_ID = OBJECT_ID(N'dbo.Capsule'))
                ALTER TABLE [dbo].[Capsule] ADD [BrdSectionsJson] NVARCHAR(MAX) NULL;
        END

        IF OBJECT_ID(N'dbo.Request', N'U') IS NOT NULL
        BEGIN
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
        END
        """,
        """
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
        """,
        """
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
        """,
        """
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
        """,
        """
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
        """,
        """
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
        """,
        """
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
        """,
        """
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
        """,
        """
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
        """
    ];

    public static async Task EnsureSchemaUpdatedAsync(IOptions<AppSettings> appSettings, ILogger logger)
    {
        var connStr = appSettings.Value.SqlSettings?.ConnectionString;
        if (string.IsNullOrWhiteSpace(connStr))
        {
            return;
        }

        try
        {
            using var connection = new SqlConnection(connStr);
            await connection.OpenAsync();
            foreach (var sql in UpgradeBatches)
            {
                await connection.ExecuteAsync(sql);
            }
            DapperConnection.InvalidateProcedureCache();
            logger.LogInformation("Verified SQL Server schema & stored procedures for OnSteroidsApiUser.");
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Skipping automatic SQL schema upgrade (check permissions or run Scripts/update_db.sql manually).");
        }
    }
}
