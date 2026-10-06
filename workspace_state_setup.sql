-- =============================================
-- UserWorkspaceState Table & Stored Procedures
-- =============================================

-- 1. Create Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'UserWorkspaceState')
BEGIN
    CREATE TABLE [dbo].[UserWorkspaceState] (
        [Id]                    UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
        [UserId]                UNIQUEIDENTIFIER NOT NULL,
        [ActiveCapsuleId]       NVARCHAR(100)    NULL,
        [ActiveCapsuleName]     NVARCHAR(255)    NULL,
        [ActiveTabId]           NVARCHAR(100)    NULL,
        [OpenTabIds]            NVARCHAR(MAX)    NULL,
        [TabStates]             NVARCHAR(MAX)    NULL,
        [Responses]             NVARCHAR(MAX)    NULL,
        [AutoAuthEnabled]       NVARCHAR(50)     NULL DEFAULT 'off',
        [AutoAuthEndpointId]    NVARCHAR(100)    NULL,
        [UpdatedAt]             DATETIME2        NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT [PK_UserWorkspaceState] PRIMARY KEY CLUSTERED ([Id]),
        CONSTRAINT [UQ_UserWorkspaceState_UserId] UNIQUE ([UserId])
    );
END
GO

-- 2. Upsert Stored Procedure
CREATE OR ALTER PROCEDURE [dbo].[spUserWorkspaceState_Upsert]
    @UserId                 UNIQUEIDENTIFIER,
    @ActiveCapsuleId        NVARCHAR(100)    = NULL,
    @ActiveCapsuleName      NVARCHAR(255)    = NULL,
    @ActiveTabId            NVARCHAR(100)    = NULL,
    @OpenTabIds             NVARCHAR(MAX)    = NULL,
    @TabStates              NVARCHAR(MAX)    = NULL,
    @Responses              NVARCHAR(MAX)    = NULL,
    @AutoAuthEnabled        NVARCHAR(50)     = NULL,
    @AutoAuthEndpointId     NVARCHAR(100)    = NULL
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
            [UpdatedAt]          = GETUTCDATE()
        WHERE [UserId] = @UserId;
    END
    ELSE
    BEGIN
        INSERT INTO [dbo].[UserWorkspaceState]
            ([UserId], [ActiveCapsuleId], [ActiveCapsuleName], [ActiveTabId],
             [OpenTabIds], [TabStates], [Responses],
             [AutoAuthEnabled], [AutoAuthEndpointId], [UpdatedAt])
        VALUES
            (@UserId, @ActiveCapsuleId, @ActiveCapsuleName, @ActiveTabId,
             @OpenTabIds, @TabStates, @Responses,
             @AutoAuthEnabled, @AutoAuthEndpointId, GETUTCDATE());
    END

    SELECT * FROM [dbo].[UserWorkspaceState] WHERE [UserId] = @UserId;
END
GO

-- 3. Get By User Stored Procedure
CREATE OR ALTER PROCEDURE [dbo].[spUserWorkspaceState_GetByUser]
    @UserId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM [dbo].[UserWorkspaceState] WHERE [UserId] = @UserId;
END
GO

-- 4. Delete Stored Procedure
CREATE OR ALTER PROCEDURE [dbo].[spUserWorkspaceState_Delete]
    @UserId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM [dbo].[UserWorkspaceState] WHERE [UserId] = @UserId;
END
GO
