IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'RefreshToken' AND Object_ID = Object_ID(N'dbo.UserAuth'))
BEGIN
    ALTER TABLE [dbo].[UserAuth] ADD [RefreshToken] NVARCHAR(256) NULL;
    ALTER TABLE [dbo].[UserAuth] ADD [RefreshTokenExpiryTime] DATETIME2(7) NULL;
END
