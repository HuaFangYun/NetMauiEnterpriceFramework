USE [__DB_NAME__];
GO

CREATE OR ALTER PROCEDURE dbo.usp_v2_get_parameter_pending_approval
    @TableName nvarchar(100) = NULL,
    @PageNumber int = 1,
    @PageSize int = 10,
    @SearchTerm nvarchar(200) = NULL,
    @SortColumn nvarchar(100) = N'TableName',
    @SortDirection nvarchar(4) = N'asc',
    @TotalCount int OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @page_size int = CASE WHEN @PageSize < 1 THEN 10 WHEN @PageSize > 200 THEN 200 ELSE @PageSize END;
    DECLARE @offset int = (CASE WHEN @PageNumber < 1 THEN 1 ELSE @PageNumber END - 1) * @page_size;
    DECLARE @dir nvarchar(4) = CASE WHEN LOWER(@SortDirection) = N'desc' THEN N'DESC' ELSE N'ASC' END;
    IF @SortColumn NOT IN (N'TableName', N'CreatedDate') SET @SortColumn = N'TableName';
    SET @SearchTerm = NULLIF(LTRIM(RTRIM(@SearchTerm)), N'');
    SET @TableName = NULLIF(LTRIM(RTRIM(@TableName)), N'');
    DECLARE @search nvarchar(210) = CASE WHEN @SearchTerm IS NULL THEN NULL ELSE N'%' + @SearchTerm + N'%' END;

    ;WITH cte AS (
        SELECT t.id AS Id, t.table_name AS TableName, t.type AS ApprovalType,
               MAX(CASE WHEN d.column_name = N'__key' THEN d.value END) AS KeyValue,
               STRING_AGG(CASE WHEN d.column_name <> N'__key' THEN CONCAT(d.column_name, N'=', ISNULL(d.value, N'')) END, N', ') AS Summary,
               t.created_by AS CreatedBy, t.created_date AS CreatedDate
        FROM dbo.tb_parameter_sytem_temporary t
        LEFT JOIN dbo.tb_parameter_sytem_temporary_detail d ON d.tb_parameter_sytem_temporary_id = t.id
        WHERE t.type IN (N'A', N'M', N'D')
          AND (@TableName IS NULL OR t.table_name = @TableName)
        GROUP BY t.id, t.table_name, t.type, t.created_by, t.created_date
    )
    SELECT @TotalCount = COUNT(*)
    FROM cte c
    WHERE (@SearchTerm IS NULL OR c.TableName LIKE N'%' + @SearchTerm + N'%' OR c.KeyValue LIKE N'%' + @SearchTerm + N'%');

    DECLARE @sql nvarchar(max) = N'
        ;WITH cte AS (
            SELECT t.id AS Id, t.table_name AS TableName, t.type AS ApprovalType,
                   MAX(CASE WHEN d.column_name = N''__key'' THEN d.value END) AS KeyValue,
                   STRING_AGG(CASE WHEN d.column_name <> N''__key'' THEN CONCAT(d.column_name, N''='', ISNULL(d.value, N'''')) END, N'', '') AS Summary,
                   t.created_by AS CreatedBy, t.created_date AS CreatedDate
            FROM dbo.tb_parameter_sytem_temporary t
            LEFT JOIN dbo.tb_parameter_sytem_temporary_detail d ON d.tb_parameter_sytem_temporary_id = t.id
            WHERE t.type IN (N''A'', N''M'', N''D'')
              AND (@tableName IS NULL OR t.table_name = @tableName)
            GROUP BY t.id, t.table_name, t.type, t.created_by, t.created_date
        )
        SELECT c.Id, c.TableName, c.ApprovalType, c.KeyValue, c.Summary,
               u.full_name AS RequesterFullName, c.CreatedDate
        FROM cte c
        LEFT JOIN dbo.um_user_app u ON u.user_id = c.CreatedBy
        WHERE (@search IS NULL OR c.TableName LIKE @search OR c.KeyValue LIKE @search)
        ORDER BY ' + QUOTENAME(@SortColumn) + N' ' + @dir + N'
        OFFSET @offset ROWS FETCH NEXT @page_size ROWS ONLY;';

    EXEC sp_executesql @sql,
        N'@offset int, @page_size int, @search nvarchar(210), @tableName nvarchar(100)',
        @offset, @page_size,
        @search = @search,
        @tableName = @TableName;
END;
GO
