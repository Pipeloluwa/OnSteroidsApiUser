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
    BEGIN
        SET @NewId = NEWID();
    END

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
