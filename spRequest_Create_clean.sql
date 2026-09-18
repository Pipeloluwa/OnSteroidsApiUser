CREATE OR ALTER PROCEDURE [dbo].[spRequest_Create]
    @Id                     UNIQUEIDENTIFIER    = NULL,
    @CapsuleId              UNIQUEIDENTIFIER,
    @UserId                 UNIQUEIDENTIFIER,
    @Name                   NVARCHAR(255)       = N'New Request',
    @Url                    NVARCHAR(2048)      = N'',
    @Method                 NVARCHAR(10)        = N'GET',
    @PayloadType            NVARCHAR(20)        = N'params',
    @BodyType               NVARCHAR(20)        = N'none',
    @RawType                NVARCHAR(20)        = N'JSON',
    @RawBody                NVARCHAR(MAX)       = NULL,
    @RawBodyJson            NVARCHAR(MAX)       = NULL,
    @RawBodyXml             NVARCHAR(MAX)       = NULL,
    @AuthType               NVARCHAR(10)        = N'none',
    @AuthToken              NVARCHAR(MAX)       = NULL,
    @PreRequestScript       NVARCHAR(MAX)       = NULL,
    @PostResponseScript     NVARCHAR(MAX)       = NULL,
    @TestScript             NVARCHAR(MAX)       = NULL,
    @TestScriptEnabled      BIT                 = 0,
    @EncryptionAlgorithm    NVARCHAR(20)        = N'none',
    @EncryptionKey          NVARCHAR(MAX)       = NULL,
    @AutoEncryptBody        BIT                 = 0,
    @AutoEncryptHeaders     BIT                 = 0,
    @EncryptedHeaders       NVARCHAR(MAX)       = NULL,
    @EncryptedBodyPaths     NVARCHAR(MAX)       = NULL,
    @EncryptionScript       NVARCHAR(MAX)       = NULL,
    @FollowRedirects        BIT                 = 1,
    @VerifySsl              BIT                 = 1,
    @EnableCookies          BIT                 = 1,
    @BypassCors             BIT                 = 1
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @NewId UNIQUEIDENTIFIER = COALESCE(@Id, NEWID());

    IF EXISTS (SELECT 1 FROM [dbo].[Request] WHERE [Id] = @NewId)
    BEGIN
        SET @NewId = NEWID();
    END

    INSERT INTO [dbo].[Request] (
        [Id], [CapsuleId], [UserId], [Name], [Url], [Method],
        [PayloadType], [BodyType], [RawType], [RawBody], [RawBodyJson], [RawBodyXml],
        [AuthType], [AuthToken],
        [PreRequestScript], [PostResponseScript], [TestScript], [TestScriptEnabled],
        [EncryptionAlgorithm], [EncryptionKey], [AutoEncryptBody], [AutoEncryptHeaders],
        [EncryptedHeaders], [EncryptedBodyPaths], [EncryptionScript],
        [FollowRedirects], [VerifySsl], [EnableCookies], [BypassCors]
    )
    VALUES (
        @NewId, @CapsuleId, @UserId, @Name, @Url, @Method,
        @PayloadType, @BodyType, @RawType, @RawBody, @RawBodyJson, @RawBodyXml,
        @AuthType, @AuthToken,
        @PreRequestScript, @PostResponseScript, @TestScript, @TestScriptEnabled,
        @EncryptionAlgorithm, @EncryptionKey, @AutoEncryptBody, @AutoEncryptHeaders,
        @EncryptedHeaders, @EncryptedBodyPaths, @EncryptionScript,
        @FollowRedirects, @VerifySsl, @EnableCookies, @BypassCors
    );

    SELECT * FROM [dbo].[Request] WHERE [Id] = @NewId;
END
