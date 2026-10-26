
GO
/****** Object:  StoredProcedure [dbo].[spApiLogs_Insert]    Script Date: 10/26/2026 8:06:53 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE   PROCEDURE [dbo].[spApiLogs_Insert]
    @TraceId nvarchar(100),
    @Method nvarchar(20),
    @Path nvarchar(255),
    @RequestBody nvarchar(max),
    @StatusCode int,
    @ResponseBody nvarchar(max)
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO [dbo].[ApiLogs] 
    (TraceId, Method, Path, RequestBody, StatusCode, ResponseBody) 
    VALUES 
    (@TraceId, @Method, @Path, @RequestBody, @StatusCode, @ResponseBody);
END

GO
/****** Object:  StoredProcedure [dbo].[spCapsule_Create]    Script Date: 10/26/2026 8:06:53 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROCEDURE [dbo].[spCapsule_Create]
    @UserId             UNIQUEIDENTIFIER,
    @Name               NVARCHAR(100),
    @Id                 UNIQUEIDENTIFIER = NULL,
    @AutoAuthEnabled    NVARCHAR(50) = 'off',
    @AutoAuthEndpointId NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @NewId UNIQUEIDENTIFIER = COALESCE(@Id, NEWID());
    INSERT INTO dbo.Capsule (Id, UserId, Name, AutoAuthEnabled, AutoAuthEndpointId, CreatedAt, UpdatedAt)
    VALUES (@NewId, @UserId, @Name, COALESCE(@AutoAuthEnabled, 'off'), @AutoAuthEndpointId, GETUTCDATE(), GETUTCDATE());

    SELECT [Id], [UserId], [Name], [AutoAuthEnabled], [AutoAuthEndpointId], [CreatedAt], [UpdatedAt]
    FROM dbo.Capsule
    WHERE Id = @NewId;
END
GO
/****** Object:  StoredProcedure [dbo].[spCapsule_Delete]    Script Date: 10/26/2026 8:06:53 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[spCapsule_Delete]
    @Id             UNIQUEIDENTIFIER,
    @UserId         UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRANSACTION;

    -- Delete variables associated with the capsule
    DELETE FROM [dbo].[Variable]
    WHERE [CapsuleId] = @Id;

    -- Delete child rows for all requests in this capsule
    DELETE FROM [dbo].[RequestParam]
    WHERE [RequestId] IN (SELECT [Id] FROM [dbo].[Request] WHERE [CapsuleId] = @Id);

    DELETE FROM [dbo].[RequestHeader]
    WHERE [RequestId] IN (SELECT [Id] FROM [dbo].[Request] WHERE [CapsuleId] = @Id);

    DELETE FROM [dbo].[RequestFormData]
    WHERE [RequestId] IN (SELECT [Id] FROM [dbo].[Request] WHERE [CapsuleId] = @Id);

    -- Delete all requests in this capsule
    DELETE FROM [dbo].[Request]
    WHERE [CapsuleId] = @Id;

    -- Delete the capsule
    DELETE FROM [dbo].[Capsule]
    WHERE [Id] = @Id AND [UserId] = @UserId;

    COMMIT TRANSACTION;
END

GO
/****** Object:  StoredProcedure [dbo].[spCapsule_GetAllByUser]    Script Date: 10/26/2026 8:06:53 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROCEDURE [dbo].[spCapsule_GetAllByUser]
    @UserId         UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [Id], [UserId], [Name], [AutoAuthEnabled], [AutoAuthEndpointId], [CreatedAt], [UpdatedAt]
    FROM [dbo].[Capsule]
    WHERE [UserId] = @UserId
    ORDER BY [CreatedAt] DESC;
END
GO
/****** Object:  StoredProcedure [dbo].[spCapsule_GetById]    Script Date: 10/26/2026 8:06:53 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROCEDURE [dbo].[spCapsule_GetById]
    @Id             UNIQUEIDENTIFIER,
    @UserId         UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [Id], [UserId], [Name], [AutoAuthEnabled], [AutoAuthEndpointId], [CreatedAt], [UpdatedAt]
    FROM [dbo].[Capsule]
    WHERE [Id] = @Id AND [UserId] = @UserId;
END
GO
/****** Object:  StoredProcedure [dbo].[spCapsule_Update]    Script Date: 10/26/2026 8:06:53 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROCEDURE [dbo].[spCapsule_Update]
    @Id                 UNIQUEIDENTIFIER,
    @UserId             UNIQUEIDENTIFIER,
    @Name               NVARCHAR(255),
    @AutoAuthEnabled    NVARCHAR(50) = NULL,
    @AutoAuthEndpointId NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE [dbo].[Capsule]
    SET [Name]               = @Name,
        [AutoAuthEnabled]    = COALESCE(@AutoAuthEnabled, [AutoAuthEnabled]),
        [AutoAuthEndpointId] = CASE WHEN @AutoAuthEnabled = 'off' THEN NULL ELSE COALESCE(@AutoAuthEndpointId, [AutoAuthEndpointId]) END,
        [UpdatedAt]          = SYSUTCDATETIME()
    WHERE [Id] = @Id AND [UserId] = @UserId;

    SELECT [Id], [UserId], [Name], [AutoAuthEnabled], [AutoAuthEndpointId], [CreatedAt], [UpdatedAt]
    FROM [dbo].[Capsule]
    WHERE [Id] = @Id;
END
GO
/****** Object:  StoredProcedure [dbo].[spRequest_Create]    Script Date: 10/26/2026 8:06:53 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROCEDURE [dbo].[spRequest_Create]
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
    @BypassCors             BIT                 = 1,
    @PostTriggerTabId       NVARCHAR(100)       = NULL,
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
        [Id], [CapsuleId], [UserId], [Name], [Url], [Method],
        [PayloadType], [BodyType], [RawType], [RawBody], [RawBodyJson], [RawBodyXml],
        [AuthType], [AuthToken],
        [PreRequestScript], [PostResponseScript], [TestScript], [TestScriptEnabled],
        [EncryptionAlgorithm], [EncryptionKey], [AutoEncryptBody], [AutoEncryptHeaders],
        [EncryptedHeaders], [EncryptedBodyPaths], [EncryptionScript],
        [FollowRedirects], [VerifySsl], [EnableCookies], [BypassCors],
        [PostTriggerTabId], [AutoAuthEnabled]
    )
    VALUES (
        @NewId, @CapsuleId, @UserId, @Name, @Url, @Method,
        @PayloadType, @BodyType, @RawType, @RawBody, @RawBodyJson, @RawBodyXml,
        @AuthType, @AuthToken,
        @PreRequestScript, @PostResponseScript, @TestScript, @TestScriptEnabled,
        @EncryptionAlgorithm, @EncryptionKey, @AutoEncryptBody, @AutoEncryptHeaders,
        @EncryptedHeaders, @EncryptedBodyPaths, @EncryptionScript,
        @FollowRedirects, @VerifySsl, @EnableCookies, @BypassCors,
        @PostTriggerTabId, @AutoAuthEnabled
    );

    SELECT * FROM [dbo].[Request] WHERE [Id] = @NewId;
END
GO
/****** Object:  StoredProcedure [dbo].[spRequest_Delete]    Script Date: 10/26/2026 8:06:53 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER OFF
GO

-- â”€â”€â”€ DELETE â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
CREATE   PROCEDURE [dbo].[spRequest_Delete]
    @Id             UNIQUEIDENTIFIER,
    @UserId         UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    -- Child rows (Params, Headers, FormData) cascade via FK
    DELETE FROM [dbo].[Request]
    WHERE [Id] = @Id AND [UserId] = @UserId;
END

GO
/****** Object:  StoredProcedure [dbo].[spRequest_Duplicate]    Script Date: 10/26/2026 8:06:53 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE   PROCEDURE [dbo].[spRequest_Duplicate]
    @SourceId       UNIQUEIDENTIFIER,
    @UserId         UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @NewId UNIQUEIDENTIFIER = NEWID();

    BEGIN TRANSACTION;

    -- Clone the request
    INSERT INTO [dbo].[Request] (
        [Id], [CapsuleId], [UserId], [Name], [Url], [Method],
        [PayloadType], [BodyType], [RawType], [RawBody], [RawBodyJson], [RawBodyXml],
        [AuthType], [AuthToken],
        [PreRequestScript], [PostResponseScript],
        [EncryptionAlgorithm], [EncryptionKey], [AutoEncryptBody], [AutoEncryptHeaders],
        [EncryptedHeaders], [EncryptedBodyPaths], [EncryptionScript],
        [FollowRedirects], [VerifySsl], [EnableCookies], [BypassCors]
    )
    SELECT
        @NewId, [CapsuleId], [UserId], [Name] + N' (Copy)', [Url], [Method],
        [PayloadType], [BodyType], [RawType], [RawBody], [RawBodyJson], [RawBodyXml],
        [AuthType], [AuthToken],
        [PreRequestScript], [PostResponseScript],
        [EncryptionAlgorithm], [EncryptionKey], [AutoEncryptBody], [AutoEncryptHeaders],
        [EncryptedHeaders], [EncryptedBodyPaths], [EncryptionScript],
        [FollowRedirects], [VerifySsl], [EnableCookies], [BypassCors]
    FROM [dbo].[Request]
    WHERE [Id] = @SourceId AND [UserId] = @UserId;

    -- Clone params
    INSERT INTO [dbo].[RequestParam] ([RequestId], [IsEnabled], [ParamKey], [ParamValue], [SortOrder])
    SELECT @NewId, [IsEnabled], [ParamKey], [ParamValue], [SortOrder]
    FROM [dbo].[RequestParam]
    WHERE [RequestId] = @SourceId;

    -- Clone headers
    INSERT INTO [dbo].[RequestHeader] ([RequestId], [IsEnabled], [HeaderKey], [HeaderValue], [SortOrder])
    SELECT @NewId, [IsEnabled], [HeaderKey], [HeaderValue], [SortOrder]
    FROM [dbo].[RequestHeader]
    WHERE [RequestId] = @SourceId;

    -- Clone form data
    INSERT INTO [dbo].[RequestFormData] ([RequestId], [IsEnabled], [FieldKey], [FieldValue], [FieldType], [SortOrder])
    SELECT @NewId, [IsEnabled], [FieldKey], [FieldValue], [FieldType], [SortOrder]
    FROM [dbo].[RequestFormData]
    WHERE [RequestId] = @SourceId;

    COMMIT TRANSACTION;

    SELECT * FROM [dbo].[Request] WHERE [Id] = @NewId;
END

GO
/****** Object:  StoredProcedure [dbo].[spRequest_GetAllByCapsule]    Script Date: 10/26/2026 8:06:53 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER OFF
GO

-- â”€â”€â”€ GET ALL BY CAPSULE â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
CREATE   PROCEDURE [dbo].[spRequest_GetAllByCapsule]
    @CapsuleId      UNIQUEIDENTIFIER,
    @UserId         UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT * FROM [dbo].[Request]
    WHERE [CapsuleId] = @CapsuleId AND [UserId] = @UserId
    ORDER BY [CreatedAt] ASC;
END

GO
/****** Object:  StoredProcedure [dbo].[spRequest_GetById]    Script Date: 10/26/2026 8:06:53 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER OFF
GO

-- â”€â”€â”€ GET BY ID â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
CREATE   PROCEDURE [dbo].[spRequest_GetById]
    @Id             UNIQUEIDENTIFIER,
    @UserId         UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT * FROM [dbo].[Request]
    WHERE [Id] = @Id AND [UserId] = @UserId;
END

GO
/****** Object:  StoredProcedure [dbo].[spRequest_Update]    Script Date: 10/26/2026 8:06:53 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROCEDURE [dbo].[spRequest_Update]
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
    @BypassCors             BIT,
    @PostTriggerTabId       NVARCHAR(100) = NULL,
    @AutoAuthEnabled        BIT = 0
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
        [PostTriggerTabId]      = @PostTriggerTabId,
        [AutoAuthEnabled]       = @AutoAuthEnabled,
        [UpdatedAt]             = GETUTCDATE()
    WHERE [Id] = @Id AND [UserId] = @UserId;

    SELECT * FROM [dbo].[Request] WHERE [Id] = @Id;
END
GO
/****** Object:  StoredProcedure [dbo].[spRequestFormData_GetByRequest]    Script Date: 10/26/2026 8:06:53 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER OFF
GO

-- ============================================================================
-- FORM DATA
-- ============================================================================

-- â”€â”€â”€ GET BY REQUEST â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
CREATE   PROCEDURE [dbo].[spRequestFormData_GetByRequest]
    @RequestId      UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [Id], [RequestId], [IsEnabled], [FieldKey], [FieldValue], [FieldType], [SortOrder], [CreatedAt], [UpdatedAt]
    FROM [dbo].[RequestFormData]
    WHERE [RequestId] = @RequestId
    ORDER BY [SortOrder] ASC;
END

GO
/****** Object:  StoredProcedure [dbo].[spRequestFormData_Sync]    Script Date: 10/26/2026 8:06:53 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE   PROCEDURE [dbo].[spRequestFormData_Sync]
    @RequestId      UNIQUEIDENTIFIER,
    @JsonData       NVARCHAR(MAX) -- format: [{"isEnabled": true, "key": "...", "value": "...", "type": "text", "sortOrder": 0}]
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRANSACTION;

    DELETE FROM [dbo].[RequestFormData] WHERE [RequestId] = @RequestId;

    IF @JsonData IS NOT NULL AND LTRIM(RTRIM(@JsonData)) <> '' AND @JsonData <> '[]'
    BEGIN
        INSERT INTO [dbo].[RequestFormData] ([RequestId], [IsEnabled], [FieldKey], [FieldValue], [FieldType], [SortOrder])
        SELECT 
            @RequestId,
            CASE 
                WHEN LOWER([isEnabled]) IN ('false', '0') THEN CAST(0 AS BIT)
                ELSE CAST(1 AS BIT)
            END,
            COALESCE([key], N''),
            COALESCE([value], N''),
            COALESCE([type], N'text'),
            COALESCE([sortOrder], 0)
        FROM OPENJSON(@JsonData) WITH (
            [isEnabled] NVARCHAR(20)  '$.isEnabled',
            [key]       NVARCHAR(500) '$.key',
            [value]     NVARCHAR(MAX) '$.value',
            [type]      NVARCHAR(10)  '$.type',
            [sortOrder] INT           '$.sortOrder'
        );
    END

    COMMIT TRANSACTION;
    
    EXEC [dbo].[spRequestFormData_GetByRequest] @RequestId = @RequestId;
END;

GO
/****** Object:  StoredProcedure [dbo].[spRequestHeader_GetByRequest]    Script Date: 10/26/2026 8:06:53 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER OFF
GO

-- ============================================================================
-- HEADERS
-- ============================================================================

-- â”€â”€â”€ GET BY REQUEST â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
CREATE   PROCEDURE [dbo].[spRequestHeader_GetByRequest]
    @RequestId      UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [Id], [RequestId], [IsEnabled], [HeaderKey], [HeaderValue], [SortOrder], [CreatedAt], [UpdatedAt]
    FROM [dbo].[RequestHeader]
    WHERE [RequestId] = @RequestId
    ORDER BY [SortOrder] ASC;
END

GO
/****** Object:  StoredProcedure [dbo].[spRequestHeader_Sync]    Script Date: 10/26/2026 8:06:53 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE   PROCEDURE [dbo].[spRequestHeader_Sync]
    @RequestId      UNIQUEIDENTIFIER,
    @JsonData       NVARCHAR(MAX) -- format: [{"isEnabled": true, "key": "...", "value": "...", "sortOrder": 0}]
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRANSACTION;

    DELETE FROM [dbo].[RequestHeader] WHERE [RequestId] = @RequestId;

    IF @JsonData IS NOT NULL AND LTRIM(RTRIM(@JsonData)) <> '' AND @JsonData <> '[]'
    BEGIN
        INSERT INTO [dbo].[RequestHeader] ([RequestId], [IsEnabled], [HeaderKey], [HeaderValue], [SortOrder])
        SELECT 
            @RequestId,
            CASE 
                WHEN LOWER([isEnabled]) IN ('false', '0') THEN CAST(0 AS BIT)
                ELSE CAST(1 AS BIT)
            END,
            COALESCE([key], N''),
            COALESCE([value], N''),
            COALESCE([sortOrder], 0)
        FROM OPENJSON(@JsonData) WITH (
            [isEnabled] NVARCHAR(20)  '$.isEnabled',
            [key]       NVARCHAR(500) '$.key',
            [value]     NVARCHAR(MAX) '$.value',
            [sortOrder] INT           '$.sortOrder'
        );
    END

    COMMIT TRANSACTION;
    
    EXEC [dbo].[spRequestHeader_GetByRequest] @RequestId = @RequestId;
END;

GO
/****** Object:  StoredProcedure [dbo].[spRequestHistory_Clear]    Script Date: 10/26/2026 8:06:53 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER OFF
GO

-- â”€â”€â”€ CLEAR ALL HISTORY FOR USER â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
CREATE   PROCEDURE [dbo].[spRequestHistory_Clear]
    @UserId         UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    DELETE FROM [dbo].[RequestHistory] 
    WHERE [UserId] = @UserId;
END

GO
/****** Object:  StoredProcedure [dbo].[spRequestHistory_Create]    Script Date: 10/26/2026 8:06:53 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER OFF
GO
-- ============================================================================
-- Post-on-Steroids: Request History Stored Procedures
-- ============================================================================

-- â”€â”€â”€ CREATE â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
CREATE   PROCEDURE [dbo].[spRequestHistory_Create]
    @UserId             UNIQUEIDENTIFIER,
    @Method             NVARCHAR(10),
    @Url                NVARCHAR(2048),
    @RequestSnapshot    NVARCHAR(MAX) = NULL,
    @ResponseStatus     INT = NULL,
    @ResponseTime       INT = NULL,
    @ResponseSize       BIGINT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @NewId UNIQUEIDENTIFIER = NEWID();

    INSERT INTO [dbo].[RequestHistory] (
        [Id], [UserId], [Method], [Url], 
        [RequestSnapshot], [ResponseStatus], [ResponseTime], [ResponseSize]
    )
    VALUES (
        @NewId, @UserId, @Method, @Url, 
        @RequestSnapshot, @ResponseStatus, @ResponseTime, @ResponseSize
    );

    SELECT * FROM [dbo].[RequestHistory] WHERE [Id] = @NewId;
END

GO
/****** Object:  StoredProcedure [dbo].[spRequestHistory_Delete]    Script Date: 10/26/2026 8:06:53 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER OFF
GO

-- â”€â”€â”€ DELETE SPECIFIC ITEM â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
CREATE   PROCEDURE [dbo].[spRequestHistory_Delete]
    @Id             UNIQUEIDENTIFIER,
    @UserId         UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    DELETE FROM [dbo].[RequestHistory] 
    WHERE [Id] = @Id AND [UserId] = @UserId;
END

GO
/****** Object:  StoredProcedure [dbo].[spRequestHistory_GetAllByUser]    Script Date: 10/26/2026 8:06:53 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER OFF
GO

-- â”€â”€â”€ GET ALL BY USER â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
CREATE   PROCEDURE [dbo].[spRequestHistory_GetAllByUser]
    @UserId         UNIQUEIDENTIFIER,
    @Limit          INT = 100
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP (@Limit) * 
    FROM [dbo].[RequestHistory]
    WHERE [UserId] = @UserId
    ORDER BY [CreatedAt] DESC;
END

GO
/****** Object:  StoredProcedure [dbo].[spRequestParam_GetByRequest]    Script Date: 10/26/2026 8:06:53 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER OFF
GO
-- ============================================================================
-- Post-on-Steroids: Request Child Tables Stored Procedures
-- (Params, Headers, FormData)
-- ============================================================================

-- ============================================================================
-- PARAMS
-- ============================================================================

-- â”€â”€â”€ GET BY REQUEST â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
CREATE   PROCEDURE [dbo].[spRequestParam_GetByRequest]
    @RequestId      UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [Id], [RequestId], [IsEnabled], [ParamKey], [ParamValue], [SortOrder], [CreatedAt], [UpdatedAt]
    FROM [dbo].[RequestParam]
    WHERE [RequestId] = @RequestId
    ORDER BY [SortOrder] ASC;
END

GO
/****** Object:  StoredProcedure [dbo].[spRequestParam_Sync]    Script Date: 10/26/2026 8:06:53 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROCEDURE [dbo].[spRequestParam_Sync]
    @RequestId      UNIQUEIDENTIFIER,
    @JsonData       NVARCHAR(MAX) -- format: [{"isEnabled": true, "key": "...", "value": "...", "sortOrder": 0}]
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRANSACTION;

    -- Remove existing
    DELETE FROM [dbo].[RequestParam] WHERE [RequestId] = @RequestId;

    -- Insert new
    IF @JsonData IS NOT NULL AND LTRIM(RTRIM(@JsonData)) <> '' AND @JsonData <> '[]'
    BEGIN
        INSERT INTO [dbo].[RequestParam] ([RequestId], [IsEnabled], [ParamKey], [ParamValue], [SortOrder])
        SELECT 
            @RequestId,
            CASE 
                WHEN LOWER([isEnabled]) IN ('false', '0') THEN CAST(0 AS BIT)
                ELSE CAST(1 AS BIT)
            END,
            COALESCE([key], N''),
            COALESCE([value], N''),
            COALESCE([sortOrder], 0)
        FROM OPENJSON(@JsonData) WITH (
            [isEnabled] NVARCHAR(20)  '$.isEnabled',
            [key]       NVARCHAR(500) '$.key',
            [value]     NVARCHAR(MAX) '$.value',
            [sortOrder] INT           '$.sortOrder'
        );
    END

    COMMIT TRANSACTION;
    
    -- Return current state
    EXEC [dbo].[spRequestParam_GetByRequest] @RequestId = @RequestId;
END;

GO
/****** Object:  StoredProcedure [dbo].[spScript_Create]    Script Date: 10/26/2026 8:06:53 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[spScript_Create]
    @Id UNIQUEIDENTIFIER,
    @UserId UNIQUEIDENTIFIER,
    @Type VARCHAR(50),
    @Name NVARCHAR(255),
    @Content NVARCHAR(MAX),
    @IsDefault BIT,
    @OriginalContent NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO [dbo].[Script] ([Id], [UserId], [Type], [Name], [Content], [IsDefault], [OriginalContent])
    VALUES (@Id, @UserId, @Type, @Name, @Content, @IsDefault, @OriginalContent);
    
    SELECT [Id], [UserId], [Type], [Name], [Content], [IsDefault], [OriginalContent], [CreatedAt], [UpdatedAt]
    FROM [dbo].[Script]
    WHERE [Id] = @Id;
END;

GO
/****** Object:  StoredProcedure [dbo].[spScript_Delete]    Script Date: 10/26/2026 8:06:53 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[spScript_Delete]
    @Id UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM [dbo].[Script]
    WHERE [Id] = @Id AND [IsDefault] = 0;
END;

GO
/****** Object:  StoredProcedure [dbo].[spScript_GetByUserId]    Script Date: 10/26/2026 8:06:53 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[spScript_GetByUserId]
    @UserId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SELECT [Id], [UserId], [Type], [Name], [Content], [IsDefault], [OriginalContent], [CreatedAt], [UpdatedAt]
    FROM [dbo].[Script]
    WHERE [UserId] = @UserId;
END;

GO
/****** Object:  StoredProcedure [dbo].[spScript_Reset]    Script Date: 10/26/2026 8:06:53 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[spScript_Reset]
    @Id UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE [dbo].[Script]
    SET [Content] = [OriginalContent],
        [UpdatedAt] = GETUTCDATE()
    WHERE [Id] = @Id AND [IsDefault] = 1;

    SELECT [Id], [UserId], [Type], [Name], [Content], [IsDefault], [OriginalContent], [CreatedAt], [UpdatedAt]
    FROM [dbo].[Script]
    WHERE [Id] = @Id;
END;

GO
/****** Object:  StoredProcedure [dbo].[spScript_SeedDefaultsForUser]    Script Date: 10/26/2026 8:06:53 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[spScript_SeedDefaultsForUser]
    @UserId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @LongScript NVARCHAR(MAX) = 'async function encryptScript(headers, body, params, encryptedHeaders, encryptedBodyPaths) {
    function getNestedValue(obj, path) {
        return path.split(''.'').reduce((acc, part) => acc && acc[part], obj);
    }
    function setNestedValue(obj, path, value) {
        const parts = path.split(''.'');
        const last = parts.pop();
        const target = parts.reduce((acc, part) => {
            if (!acc[part]) acc[part] = {};
            return acc[part];
        }, obj);
        if (target) target[last] = value;
    }
    function getPrimitivePaths(obj, currentPath = '''') {
        let paths = [];
        for (let key in obj) {
            if (obj.hasOwnProperty(key)) {
                const path = currentPath ? ${currentPath}. : key;
                if (obj[key] !== null && typeof obj[key] === ''object'') {
                    paths = paths.concat(getPrimitivePaths(obj[key], path));
                } else {
                    paths.push(path);
                }
            }
        }
        return paths;
    }
    const parameters = {};
    const shouldEncryptAllHeaders = typeof autoEncryptHeaders !== ''undefined'' ? autoEncryptHeaders : false;
    const encHeadersList = encryptedHeaders || [];
    for (let h of headers) {
        if (h.enabled && h.key) {
            if (shouldEncryptAllHeaders || encHeadersList.includes(h.key)) {
                parameters[h.key] = h.value;
            }
        }
    }
    let bodyObj = null;
    if (body) {
        try {
            bodyObj = JSON.parse(body);
        } catch (e) {}
    }
    if (bodyObj) {
        const shouldEncryptAllBody = typeof autoEncryptBody !== ''undefined'' ? autoEncryptBody : false;
        const encBodyPathsList = encryptedBodyPaths || [];
        if (shouldEncryptAllBody) {
            const allPaths = getPrimitivePaths(bodyObj);
            for (let path of allPaths) {
                const val = getNestedValue(bodyObj, path);
                if (val !== undefined && val !== null) {
                    parameters[path] = val;
                }
            }
        } else {
            for (let path of encBodyPathsList) {
                const val = getNestedValue(bodyObj, path);
                if (val !== undefined && val !== null) {
                    parameters[path] = val;
                }
            }
        }
    }
    if (Object.keys(parameters).length > 0) {
        try {
            const chName = typeof channelName !== ''undefined'' ? channelName : ''Default Channel'';
            const response = await fetch(''https://localhost:7131/api/v1/auth/encrypt'', {
                method: ''POST'',
                headers: {
                    ''Content-Type'': ''application/json''
                },
                body: JSON.stringify({
                    channelName: chName,
                    parameters: parameters
                })
            });
            if (response.ok) {
                const result = await response.json();
                const encryptedParams = result.parameters || result;
                for (let h of headers) {
                    if (h.key && encryptedParams[h.key] !== undefined) {
                        h.value = String(encryptedParams[h.key]);
                    }
                }
                if (bodyObj) {
                    for (let key in encryptedParams) {
                        if (encryptedParams.hasOwnProperty(key)) {
                            if (key.includes(''.'') || getNestedValue(bodyObj, key) !== undefined) {
                                setNestedValue(bodyObj, key, encryptedParams[key]);
                            }
                        }
                    }
                    body = JSON.stringify(bodyObj, null, 2);
                }
            }
        } catch (error) {
            console.error(''Error during payload encryption:'', error);
        }
    }
    return body;
}';

    IF NOT EXISTS (SELECT 1 FROM [dbo].[Script] WHERE [UserId] = @UserId AND [Type] = 'Encryption' AND [Name] = 'Encrypt' AND [IsDefault] = 1)
    BEGIN
        INSERT INTO [dbo].[Script] ([Id], [UserId], [Type], [Name], [Content], [IsDefault], [OriginalContent])
        VALUES (NEWID(), @UserId, 'Encryption', 'Encrypt', @LongScript, 1, @LongScript);
    END

    IF NOT EXISTS (SELECT 1 FROM [dbo].[Script] WHERE [UserId] = @UserId AND [Type] = 'Encryption' AND [Name] = 'Decrypt' AND [IsDefault] = 1)
    BEGIN
        INSERT INTO [dbo].[Script] ([Id], [UserId], [Type], [Name], [Content], [IsDefault], [OriginalContent])
        VALUES (NEWID(), @UserId, 'Encryption', 'Decrypt', @LongScript, 1, @LongScript);
    END

    IF NOT EXISTS (SELECT 1 FROM [dbo].[Script] WHERE [UserId] = @UserId AND [Type] = 'PreRequest' AND [IsDefault] = 1)
    BEGIN
        INSERT INTO [dbo].[Script] ([Id], [UserId], [Type], [Name], [Content], [IsDefault], [OriginalContent])
        VALUES (NEWID(), @UserId, 'PreRequest', 'Pre Request Script', 'function preScript(headers, body, params){
    //only code written within this code block will be executed
}', 1, 'function preScript(headers, body, params){
    //only code written within this code block will be executed
}');
    END

    IF NOT EXISTS (SELECT 1 FROM [dbo].[Script] WHERE [UserId] = @UserId AND [Type] = 'PostRequest' AND [IsDefault] = 1)
    BEGIN
        INSERT INTO [dbo].[Script] ([Id], [UserId], [Type], [Name], [Content], [IsDefault], [OriginalContent])
        VALUES (NEWID(), @UserId, 'PostRequest', 'Post Request Script', 'function postScript(responseHeader, responseBody){
    //only code written within this code block will be executed
}', 1, 'function postScript(responseHeader, responseBody){
    //only code written within this code block will be executed
}');
    END

    SELECT [Id], [UserId], [Type], [Name], [Content], [IsDefault], [OriginalContent], [CreatedAt], [UpdatedAt]
    FROM [dbo].[Script]
    WHERE [UserId] = @UserId;
END;

GO
/****** Object:  StoredProcedure [dbo].[spScript_Update]    Script Date: 10/26/2026 8:06:53 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[spScript_Update]
    @Id UNIQUEIDENTIFIER,
    @Name NVARCHAR(255),
    @Content NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE [dbo].[Script]
    SET [Name] = @Name,
        [Content] = @Content,
        [UpdatedAt] = GETUTCDATE()
    WHERE [Id] = @Id;

    SELECT [Id], [UserId], [Type], [Name], [Content], [IsDefault], [OriginalContent], [CreatedAt], [UpdatedAt]
    FROM [dbo].[Script]
    WHERE [Id] = @Id;
END;

GO
/****** Object:  StoredProcedure [dbo].[spUserAuth_ChangePassword]    Script Date: 10/26/2026 8:06:53 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE   PROCEDURE [dbo].[spUserAuth_ChangePassword]
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

    SELECT [Id], [Email], [IsAuthenticated], [PasswordHash], [IsOtpVerified],
           CAST(1 AS BIT) AS [HasPassword],
           [CreatedAt], [UpdatedAt]
    FROM [dbo].[UserAuth]
    WHERE [Id] = @Id;
END

GO
/****** Object:  StoredProcedure [dbo].[spUserAuth_GetByEmail]    Script Date: 10/26/2026 8:06:53 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE   PROCEDURE [dbo].[spUserAuth_GetByEmail]
    @Email NVARCHAR(320)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [Id], [Email], [Otp], [IsAuthenticated], [OtpExpiresAt], [PasswordHash], [IsOtpVerified],
           [RefreshToken], [RefreshTokenExpiryTime],
           CAST(CASE WHEN [PasswordHash] IS NOT NULL AND LEN([PasswordHash]) > 0 THEN 1 ELSE 0 END AS BIT) AS [HasPassword],
           [CreatedAt], [UpdatedAt]
    FROM [dbo].[UserAuth]
    WHERE [Email] = @Email;
END

GO
/****** Object:  StoredProcedure [dbo].[spUserAuth_GetById]    Script Date: 10/26/2026 8:06:53 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE   PROCEDURE [dbo].[spUserAuth_GetById]
    @Id UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [Id], [Email], [Otp], [IsAuthenticated], [OtpExpiresAt], [PasswordHash], [IsOtpVerified],
           [RefreshToken], [RefreshTokenExpiryTime],
           CAST(CASE WHEN [PasswordHash] IS NOT NULL AND LEN([PasswordHash]) > 0 THEN 1 ELSE 0 END AS BIT) AS [HasPassword],
           [CreatedAt], [UpdatedAt]
    FROM [dbo].[UserAuth]
    WHERE [Id] = @Id;
END

GO
/****** Object:  StoredProcedure [dbo].[spUserAuth_GetByRefreshToken]    Script Date: 10/26/2026 8:06:53 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE   PROCEDURE [dbo].[spUserAuth_GetByRefreshToken]
    @RefreshToken NVARCHAR(256)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [Id], [Email], [IsAuthenticated], [PasswordHash], [IsOtpVerified],
           [RefreshToken], [RefreshTokenExpiryTime],
           CAST(CASE WHEN [PasswordHash] IS NOT NULL AND LEN([PasswordHash]) > 0 THEN 1 ELSE 0 END AS BIT) AS [HasPassword],
           [CreatedAt], [UpdatedAt]
    FROM [dbo].[UserAuth]
    WHERE [RefreshToken] = @RefreshToken;
END

GO
/****** Object:  StoredProcedure [dbo].[spUserAuth_Logout]    Script Date: 10/26/2026 8:06:53 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROCEDURE [dbo].[spUserAuth_Logout]
    @Id UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE [dbo].[UserAuth]
    SET [IsAuthenticated] = 0,
        [RefreshToken] = NULL,
        [RefreshTokenExpiryTime] = NULL,
        [UpdatedAt] = SYSUTCDATETIME()
    WHERE [Id] = @Id;
END

GO
/****** Object:  StoredProcedure [dbo].[spUserAuth_Register]    Script Date: 10/26/2026 8:06:53 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE   PROCEDURE [dbo].[spUserAuth_Register]
    @Email NVARCHAR(320)
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM [dbo].[UserAuth] WHERE [Email] = @Email)
    BEGIN
        INSERT INTO [dbo].[UserAuth] ([Email])
        VALUES (@Email);
    END

    SELECT [Id], [Email], [Otp], [IsAuthenticated], [OtpExpiresAt], [PasswordHash], [IsOtpVerified],
           CAST(CASE WHEN [PasswordHash] IS NOT NULL AND LEN([PasswordHash]) > 0 THEN 1 ELSE 0 END AS BIT) AS [HasPassword],
           [CreatedAt], [UpdatedAt]
    FROM [dbo].[UserAuth]
    WHERE [Email] = @Email;
END

GO
/****** Object:  StoredProcedure [dbo].[spUserAuth_SendOtp]    Script Date: 10/26/2026 8:06:53 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE   PROCEDURE [dbo].[spUserAuth_SendOtp]
    @Email          NVARCHAR(320),
    @Otp            NVARCHAR(10),
    @OtpExpiresAt   DATETIME2(7)
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM [dbo].[UserAuth] WHERE [Email] = @Email)
    BEGIN
        RAISERROR('User not found.', 16, 1);
        RETURN;
    END

    UPDATE [dbo].[UserAuth]
    SET [Otp]             = @Otp,
        [OtpExpiresAt]    = @OtpExpiresAt,
        [IsOtpVerified]   = 0,
        [IsAuthenticated] = 0,
        [UpdatedAt]       = SYSUTCDATETIME()
    WHERE [Email] = @Email;

    SELECT [Id], [Email], [Otp], [IsAuthenticated], [OtpExpiresAt], [PasswordHash], [IsOtpVerified],
           CAST(CASE WHEN [PasswordHash] IS NOT NULL AND LEN([PasswordHash]) > 0 THEN 1 ELSE 0 END AS BIT) AS [HasPassword],
           [CreatedAt], [UpdatedAt]
    FROM [dbo].[UserAuth]
    WHERE [Email] = @Email;
END

GO
/****** Object:  StoredProcedure [dbo].[spUserAuth_SetAuthenticated]    Script Date: 10/26/2026 8:06:53 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE   PROCEDURE [dbo].[spUserAuth_SetAuthenticated]
    @Id UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE [dbo].[UserAuth]
    SET [IsAuthenticated] = 1,
        [UpdatedAt]       = SYSUTCDATETIME()
    WHERE [Id] = @Id;

    SELECT [Id], [Email], [IsAuthenticated], [PasswordHash], [IsOtpVerified],
           CAST(CASE WHEN [PasswordHash] IS NOT NULL AND LEN([PasswordHash]) > 0 THEN 1 ELSE 0 END AS BIT) AS [HasPassword],
           [CreatedAt], [UpdatedAt]
    FROM [dbo].[UserAuth]
    WHERE [Id] = @Id;
END

GO
/****** Object:  StoredProcedure [dbo].[spUserAuth_SetPassword]    Script Date: 10/26/2026 8:06:53 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE   PROCEDURE [dbo].[spUserAuth_SetPassword]
    @Email          NVARCHAR(320),
    @PasswordHash   NVARCHAR(512)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @IsOtpVerified BIT;

    SELECT @IsOtpVerified = [IsOtpVerified]
    FROM [dbo].[UserAuth]
    WHERE [Email] = @Email;

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
    SET [PasswordHash]    = @PasswordHash,
        [IsOtpVerified]   = 0,
        [IsAuthenticated] = 1,
        [Otp]             = NULL,
        [OtpExpiresAt]    = NULL,
        [UpdatedAt]       = SYSUTCDATETIME()
    WHERE [Email] = @Email;

    SELECT [Id], [Email], [IsAuthenticated], [PasswordHash], [IsOtpVerified],
           CAST(1 AS BIT) AS [HasPassword],
           [CreatedAt], [UpdatedAt]
    FROM [dbo].[UserAuth]
    WHERE [Email] = @Email;
END

GO
/****** Object:  StoredProcedure [dbo].[spUserAuth_UpdateRefreshToken]    Script Date: 10/26/2026 8:06:53 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROCEDURE [dbo].[spUserAuth_UpdateRefreshToken]
    @Id UNIQUEIDENTIFIER,
    @RefreshToken NVARCHAR(256),
    @RefreshTokenExpiryTime DATETIME2(7)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE [dbo].[UserAuth]
    SET [RefreshToken] = @RefreshToken,
        [RefreshTokenExpiryTime] = @RefreshTokenExpiryTime,
        [UpdatedAt] = SYSUTCDATETIME()
    WHERE [Id] = @Id;
END

GO
/****** Object:  StoredProcedure [dbo].[spUserAuth_VerifyOtp]    Script Date: 10/26/2026 8:06:53 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE   PROCEDURE [dbo].[spUserAuth_VerifyOtp]
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
    SET [IsOtpVerified]   = 1,
        [IsAuthenticated] = CASE WHEN [PasswordHash] IS NOT NULL AND LEN([PasswordHash]) > 0 THEN 1 ELSE 0 END,
        [Otp]             = NULL,
        [OtpExpiresAt]    = NULL,
        [UpdatedAt]       = SYSUTCDATETIME()
    WHERE [Email] = @Email;

    SELECT [Id], [Email], [IsAuthenticated], [PasswordHash], [IsOtpVerified],
           CAST(CASE WHEN [PasswordHash] IS NOT NULL AND LEN([PasswordHash]) > 0 THEN 1 ELSE 0 END AS BIT) AS [HasPassword],
           [CreatedAt], [UpdatedAt]
    FROM [dbo].[UserAuth]
    WHERE [Email] = @Email;
END

GO
/****** Object:  StoredProcedure [dbo].[spUserWorkspaceState_Delete]    Script Date: 10/26/2026 8:06:53 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER OFF
GO

-- 4. Delete Stored Procedure
CREATE   PROCEDURE [dbo].[spUserWorkspaceState_Delete]
    @UserId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM [dbo].[UserWorkspaceState] WHERE [UserId] = @UserId;
END

GO
/****** Object:  StoredProcedure [dbo].[spUserWorkspaceState_GetByUser]    Script Date: 10/26/2026 8:06:53 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER OFF
GO

-- 3. Get By User Stored Procedure
CREATE   PROCEDURE [dbo].[spUserWorkspaceState_GetByUser]
    @UserId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM [dbo].[UserWorkspaceState] WHERE [UserId] = @UserId;
END

GO
/****** Object:  StoredProcedure [dbo].[spUserWorkspaceState_Upsert]    Script Date: 10/26/2026 8:06:53 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[spUserWorkspaceState_Upsert]
    @UserId                 UNIQUEIDENTIFIER,
    @ActiveCapsuleId        NVARCHAR(100)    = NULL,
    @ActiveCapsuleName      NVARCHAR(255)    = NULL,
    @ActiveTabId            NVARCHAR(100)    = NULL,
    @OpenTabIds             NVARCHAR(MAX)    = NULL,
    @TabStates              NVARCHAR(MAX)    = NULL,
    @Responses              NVARCHAR(MAX)    = NULL,
    @AutoAuthEnabled        NVARCHAR(50)     = NULL,
    @AutoAuthEndpointId     NVARCHAR(100)    = NULL,
    @AutoSaveEnabled        BIT              = 0
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM [dbo].[UserWorkspaceState] WHERE [UserId] = @UserId)
    BEGIN
        UPDATE [dbo].[UserWorkspaceState]
        SET
            [ActiveCapsuleId]    = @ActiveCapsuleId,
            [ActiveCapsuleName]  = @ActiveCapsuleName,
            [ActiveTabId]        = @ActiveTabId,
            [OpenTabIds]         = @OpenTabIds,
            [TabStates]          = @TabStates,
            [Responses]          = @Responses,
            [AutoAuthEnabled]    = @AutoAuthEnabled,
            [AutoAuthEndpointId] = @AutoAuthEndpointId,
            [AutoSaveEnabled]    = @AutoSaveEnabled,
            [UpdatedAt]          = GETUTCDATE()
        WHERE [UserId] = @UserId;
    END
    ELSE
    BEGIN
        INSERT INTO [dbo].[UserWorkspaceState]
            ([UserId], [ActiveCapsuleId], [ActiveCapsuleName], [ActiveTabId],
             [OpenTabIds], [TabStates], [Responses],
             [AutoAuthEnabled], [AutoAuthEndpointId], [AutoSaveEnabled], [UpdatedAt])
        VALUES
            (@UserId, @ActiveCapsuleId, @ActiveCapsuleName, @ActiveTabId,
             @OpenTabIds, @TabStates, @Responses,
             @AutoAuthEnabled, @AutoAuthEndpointId, @AutoSaveEnabled, GETUTCDATE());
    END

    SELECT * FROM [dbo].[UserWorkspaceState] WHERE [UserId] = @UserId;
END
GO
/****** Object:  StoredProcedure [dbo].[spVariable_Create]    Script Date: 10/26/2026 8:06:53 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[spVariable_Create]
    @UserId UNIQUEIDENTIFIER,
    @CapsuleId UNIQUEIDENTIFIER = NULL,
    @VariableKey NVARCHAR(255),
    @VariableValue NVARCHAR(MAX),
    @Type NVARCHAR(50) = 'global',
    @IsEnabled BIT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @NewId UNIQUEIDENTIFIER = NEWID();
    DECLARE @CreatedAt DATETIME2 = SYSUTCDATETIME();
    INSERT INTO [dbo].[Variable] (Id, UserId, CapsuleId, VariableKey, VariableValue, Type, IsEnabled, CreatedAt, UpdatedAt)
    VALUES (@NewId, @UserId, @CapsuleId, @VariableKey, @VariableValue, @Type, @IsEnabled, @CreatedAt, @CreatedAt);
    SELECT * FROM [dbo].[Variable] WHERE Id = @NewId;
END

GO
/****** Object:  StoredProcedure [dbo].[spVariable_Delete]    Script Date: 10/26/2026 8:06:53 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER OFF
GO

-- â”€â”€â”€ DELETE â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
CREATE   PROCEDURE [dbo].[spVariable_Delete]
    @Id             UNIQUEIDENTIFIER,
    @UserId         UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    DELETE FROM [dbo].[Variable] 
    WHERE [Id] = @Id AND [UserId] = @UserId;
END

GO
/****** Object:  StoredProcedure [dbo].[spVariable_GetAllByUser]    Script Date: 10/26/2026 8:06:53 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[spVariable_GetAllByUser]
    @UserId UNIQUEIDENTIFIER,
    @CapsuleId UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;
    IF @CapsuleId IS NOT NULL
    BEGIN
        SELECT * FROM [dbo].[Variable] WHERE [UserId] = @UserId AND [CapsuleId] = @CapsuleId;
    END
    ELSE
    BEGIN
        SELECT * FROM [dbo].[Variable] WHERE [UserId] = @UserId AND [CapsuleId] IS NULL;
    END
END

GO
/****** Object:  StoredProcedure [dbo].[spVariable_Sync]    Script Date: 10/26/2026 8:06:53 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[spVariable_Sync]
    @UserId         UNIQUEIDENTIFIER,
    @CapsuleId      UNIQUEIDENTIFIER = NULL,
    @JsonData       NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRANSACTION;

    IF @CapsuleId IS NOT NULL
    BEGIN
        DELETE FROM [dbo].[Variable] WHERE [UserId] = @UserId AND [CapsuleId] = @CapsuleId;
    END
    ELSE
    BEGIN
        DELETE FROM [dbo].[Variable] WHERE [UserId] = @UserId AND [CapsuleId] IS NULL;
    END

    IF @JsonData IS NOT NULL AND LTRIM(RTRIM(@JsonData)) <> '' AND @JsonData <> '[]'
    BEGIN
        ;WITH ParsedVars AS (
            SELECT 
                TRY_CAST([id] AS UNIQUEIDENTIFIER) AS ParsedId,
                [key] AS VarKey,
                COALESCE([value], N'') AS VarValue,
                COALESCE([type], N'global') AS VarType,
                CASE 
                    WHEN LOWER([enabled]) IN ('false', '0') THEN CAST(0 AS BIT)
                    ELSE CAST(1 AS BIT)
                END AS VarEnabled,
                ROW_NUMBER() OVER (PARTITION BY [key] ORDER BY (SELECT NULL)) AS KeyRank,
                ROW_NUMBER() OVER (PARTITION BY TRY_CAST([id] AS UNIQUEIDENTIFIER) ORDER BY (SELECT NULL)) AS IdRank
            FROM OPENJSON(@JsonData) WITH (
                [id]      NVARCHAR(50)   '$.id',
                [key]     NVARCHAR(255)  '$.key',
                [value]   NVARCHAR(MAX)  '$.value',
                [type]    NVARCHAR(50)   '$.type',
                [enabled] NVARCHAR(20)   '$.enabled'
            )
            WHERE [key] IS NOT NULL AND LTRIM(RTRIM([key])) <> ''
        )
        INSERT INTO [dbo].[Variable] ([Id], [UserId], [CapsuleId], [VariableKey], [VariableValue], [Type], [IsEnabled], [CreatedAt], [UpdatedAt])
        SELECT 
            CASE 
                WHEN ParsedId IS NOT NULL 
                     AND IdRank = 1
                     AND NOT EXISTS (SELECT 1 FROM [dbo].[Variable] WHERE [Id] = ParsedId)
                THEN ParsedId
                ELSE NEWID()
            END,
            @UserId,
            @CapsuleId,
            VarKey,
            VarValue,
            VarType,
            VarEnabled,
            SYSUTCDATETIME(),
            SYSUTCDATETIME()
        FROM ParsedVars
        WHERE KeyRank = 1;
    END

    COMMIT TRANSACTION;

    EXEC [dbo].[spVariable_GetAllByUser] @UserId = @UserId, @CapsuleId = @CapsuleId;
END;

GO
/****** Object:  StoredProcedure [dbo].[spVariable_Update]    Script Date: 10/26/2026 8:06:53 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[spVariable_Update]
    @Id             UNIQUEIDENTIFIER,
    @UserId         UNIQUEIDENTIFIER,
    @CapsuleId      UNIQUEIDENTIFIER = NULL,
    @VariableKey    NVARCHAR(255) = NULL,
    @VariableValue  NVARCHAR(MAX) = NULL,
    @Type           NVARCHAR(50) = NULL,
    @IsEnabled      BIT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE [dbo].[Variable]
    SET [VariableKey]   = COALESCE(@VariableKey, [VariableKey]),
        [VariableValue] = COALESCE(@VariableValue, [VariableValue]),
        [Type]          = COALESCE(@Type, [Type]),
        [IsEnabled]     = COALESCE(@IsEnabled, [IsEnabled]),
        [UpdatedAt]     = SYSUTCDATETIME()
    WHERE [Id] = @Id AND [UserId] = @UserId AND (CapsuleId = @CapsuleId OR (@CapsuleId IS NULL AND CapsuleId IS NULL));
    SELECT [Id], [UserId], [CapsuleId], [VariableKey], [VariableValue], [Type], [IsEnabled], [CreatedAt], [UpdatedAt]
    FROM [dbo].[Variable]
    WHERE [Id] = @Id;
END

GO
