CREATE OR ALTER PROCEDURE [dbo].[spRequest_Update]
    @Id                     UNIQUEIDENTIFIER,
    @CapsuleId              UNIQUEIDENTIFIER,
    @UserId                 UNIQUEIDENTIFIER,
    @Name                   NVARCHAR(255),
    @Url                    NVARCHAR(2048),
    @Method                 NVARCHAR(10),
    @PayloadType            NVARCHAR(20),
    @BodyType               NVARCHAR(20),
    @RawType                NVARCHAR(20),
    @RawBody                NVARCHAR(MAX),
    @RawBodyJson            NVARCHAR(MAX),
    @RawBodyXml             NVARCHAR(MAX),
    @AuthType               NVARCHAR(10),
    @AuthToken              NVARCHAR(MAX),
    @PreRequestScript       NVARCHAR(MAX),
    @PostResponseScript     NVARCHAR(MAX),
    @TestScript             NVARCHAR(MAX),
    @TestScriptEnabled      BIT,
    @EncryptionAlgorithm    NVARCHAR(20),
    @EncryptionKey          NVARCHAR(MAX),
    @AutoEncryptBody        BIT,
    @AutoEncryptHeaders     BIT,
    @EncryptedHeaders       NVARCHAR(MAX),
    @EncryptedBodyPaths     NVARCHAR(MAX),
    @EncryptionScript       NVARCHAR(MAX),
    @FollowRedirects        BIT,
    @VerifySsl              BIT,
    @EnableCookies          BIT,
    @BypassCors             BIT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE [dbo].[Request]
    SET 
        [CapsuleId]             = @CapsuleId,
        [Name]                  = @Name,
        [Url]                   = @Url,
        [Method]                = @Method,
        [PayloadType]           = @PayloadType,
        [BodyType]              = @BodyType,
        [RawType]               = @RawType,
        [RawBody]               = @RawBody,
        [RawBodyJson]           = @RawBodyJson,
        [RawBodyXml]            = @RawBodyXml,
        [AuthType]              = @AuthType,
        [AuthToken]             = @AuthToken,
        [PreRequestScript]      = @PreRequestScript,
        [PostResponseScript]    = @PostResponseScript,
        [TestScript]            = @TestScript,
        [TestScriptEnabled]     = @TestScriptEnabled,
        [EncryptionAlgorithm]   = @EncryptionAlgorithm,
        [EncryptionKey]         = @EncryptionKey,
        [AutoEncryptBody]       = @AutoEncryptBody,
        [AutoEncryptHeaders]    = @AutoEncryptHeaders,
        [EncryptedHeaders]      = @EncryptedHeaders,
        [EncryptedBodyPaths]    = @EncryptedBodyPaths,
        [EncryptionScript]      = @EncryptionScript,
        [FollowRedirects]       = @FollowRedirects,
        [VerifySsl]             = @VerifySsl,
        [EnableCookies]         = @EnableCookies,
        [BypassCors]            = @BypassCors,
        [UpdatedAt]             = GETUTCDATE()
    WHERE [Id] = @Id AND [UserId] = @UserId;

    SELECT * FROM [dbo].[Request] WHERE [Id] = @Id;
END
