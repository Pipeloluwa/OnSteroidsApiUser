USE [OnSteroids];
GO

IF COL_LENGTH('dbo.UserAuth', 'PasswordHash') IS NULL
BEGIN
    ALTER TABLE [dbo].[UserAuth] ADD [PasswordHash] NVARCHAR(512) NULL;
END
GO

IF COL_LENGTH('dbo.UserAuth', 'IsOtpVerified') IS NULL
BEGIN
    ALTER TABLE [dbo].[UserAuth] ADD [IsOtpVerified] BIT NOT NULL CONSTRAINT DF_UserAuth_IsOtpVerified DEFAULT 0;
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

    SELECT [Id], [Email], [Otp], [IsAuthenticated], [OtpExpiresAt], [PasswordHash], [IsOtpVerified],
           CAST(CASE WHEN [PasswordHash] IS NOT NULL AND LEN([PasswordHash]) > 0 THEN 1 ELSE 0 END AS BIT) AS [HasPassword],
           [CreatedAt], [UpdatedAt]
    FROM [dbo].[UserAuth]
    WHERE [Email] = @Email;
END
GO

CREATE OR ALTER PROCEDURE [dbo].[spUserAuth_GetByEmail]
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

CREATE OR ALTER PROCEDURE [dbo].[spUserAuth_GetById]
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

CREATE OR ALTER PROCEDURE [dbo].[spUserAuth_GetByRefreshToken]
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

CREATE OR ALTER PROCEDURE [dbo].[spUserAuth_SendOtp]
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

CREATE OR ALTER PROCEDURE [dbo].[spUserAuth_SetPassword]
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

CREATE OR ALTER PROCEDURE [dbo].[spUserAuth_SetAuthenticated]
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

    SELECT [Id], [Email], [IsAuthenticated], [PasswordHash], [IsOtpVerified],
           CAST(1 AS BIT) AS [HasPassword],
           [CreatedAt], [UpdatedAt]
    FROM [dbo].[UserAuth]
    WHERE [Id] = @Id;
END
GO
