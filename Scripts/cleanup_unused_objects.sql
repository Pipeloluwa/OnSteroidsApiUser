-- ============================================================================
-- Migration: Clean and remove unused stored procedures and objects
-- Database: OnSteroids
-- ============================================================================

-- Drop unused procedure: spUserAuth_Delete (not implemented in any repository or service)
IF OBJECT_ID(N'dbo.spUserAuth_Delete', N'P') IS NOT NULL
BEGIN
    DROP PROCEDURE [dbo].[spUserAuth_Delete];
    PRINT 'Dropped dbo.spUserAuth_Delete';
END
GO

-- Drop unused procedure: spUserAuth_UpdateEmail (not implemented in any repository or service)
IF OBJECT_ID(N'dbo.spUserAuth_UpdateEmail', N'P') IS NOT NULL
BEGIN
    DROP PROCEDURE [dbo].[spUserAuth_UpdateEmail];
    PRINT 'Dropped dbo.spUserAuth_UpdateEmail';
END
GO
