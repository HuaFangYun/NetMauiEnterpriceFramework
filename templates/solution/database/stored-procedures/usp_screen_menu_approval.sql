USE [__DB_NAME__];
GO

-- Applies the main tb_screen_menu request. Child/import multi-table payload
-- is handled by the C# ScreenMenu handler and can be extended here later.
CREATE OR ALTER PROCEDURE dbo.usp_screen_menu_approval
    @temporaryId uniqueidentifier,
    @type nvarchar(20),
    @approvalId nvarchar(100)
AS
BEGIN
    SET NOCOUNT ON;
    EXEC dbo.usp_parameter_sytem_approval @temporaryId = @temporaryId, @type = @type, @approvalId = @approvalId;
END;
GO
