CREATE OR ALTER PROCEDURE [dbo].[spUserAuth_UpdateRefreshToken]
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
CREATE OR ALTER PROCEDURE [dbo].[spUserAuth_GetByRefreshToken]
    @RefreshToken NVARCHAR(256)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT [Id], [Email], [IsAuthenticated], [RefreshToken], [RefreshTokenExpiryTime], [CreatedAt], [UpdatedAt]
    FROM [dbo].[UserAuth]
    WHERE [RefreshToken] = @RefreshToken;
END
GO
CREATE OR ALTER PROCEDURE [dbo].[spUserAuth_Logout]
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
