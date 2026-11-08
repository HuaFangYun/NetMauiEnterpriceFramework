USE [__DB_NAME__];
GO

CREATE OR ALTER PROCEDURE dbo.usp_v2_um_user_group_approval
    @temporaryId uniqueidentifier,
    @type nvarchar(20),
    @approvalId nvarchar(100)
AS
BEGIN
    SET NOCOUNT ON;
    EXEC dbo.usp_parameter_sytem_approval @temporaryId = @temporaryId, @type = @type, @approvalId = @approvalId;
END;
GO
