USE [__DB_NAME__];
GO

CREATE OR ALTER PROCEDURE dbo.usp_parameter_sytem_detail_save
    @temporaryId uniqueidentifier,
    @detailsJson nvarchar(max)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF NOT EXISTS (SELECT 1 FROM dbo.tb_parameter_sytem_temporary WHERE id = @temporaryId)
        THROW 51002, N'Pending request not found.', 1;

    BEGIN TRANSACTION;

    INSERT INTO dbo.tb_parameter_sytem_temporary_detail
        (tb_parameter_sytem_temporary_id, column_name, value, value_before)
    SELECT @temporaryId, column_name, value, value_before
    FROM OPENJSON(@detailsJson)
    WITH (
        column_name nvarchar(100) '$.column_name',
        value nvarchar(max) '$.value',
        value_before nvarchar(max) '$.value_before'
    );

    COMMIT TRANSACTION;
END;
GO
