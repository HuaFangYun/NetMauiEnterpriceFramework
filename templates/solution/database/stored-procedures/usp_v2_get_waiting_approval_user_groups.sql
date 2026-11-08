USE [__DB_NAME__];
GO

CREATE OR ALTER PROCEDURE dbo.usp_v2_get_waiting_approval_user_groups
    @PageNumber int = 1,
    @PageSize int = 10,
    @SearchTerm nvarchar(200) = NULL,
    @SortColumn nvarchar(100) = N'UserId',
    @SortDirection nvarchar(4) = N'asc',
    @TotalCount int OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @page_size int = CASE WHEN @PageSize < 1 THEN 10 WHEN @PageSize > 200 THEN 200 ELSE @PageSize END;
    DECLARE @offset int = (CASE WHEN @PageNumber < 1 THEN 1 ELSE @PageNumber END - 1) * @page_size;
    DECLARE @dir nvarchar(4) = CASE WHEN LOWER(@SortDirection) = N'desc' THEN N'DESC' ELSE N'ASC' END;
    IF @SortColumn NOT IN (N'UserId', N'FullName', N'GroupName', N'CreatedDate') SET @SortColumn = N'UserId';
    SET @SearchTerm = NULLIF(LTRIM(RTRIM(@SearchTerm)), N'');
    DECLARE @search nvarchar(210) = CASE WHEN @SearchTerm IS NULL THEN NULL ELSE N'%' + @SearchTerm + N'%' END;

    ;WITH cte AS (
        SELECT t.id AS Id,
               MAX(CASE WHEN d.column_name = N'user_id' THEN d.value END) AS UserId,
               MAX(CASE WHEN d.column_name = N'group_id' THEN d.value END) AS GroupId,
               MAX(CASE WHEN d.column_name = N'can_add_edit' THEN d.value END) AS CanAddEdit,
               MAX(CASE WHEN d.column_name = N'can_delete' THEN d.value END) AS CanDelete,
               MAX(CASE WHEN d.column_name = N'can_read_only' THEN d.value END) AS CanReadOnly,
               MAX(CASE WHEN d.column_name = N'responsible_to_user_id' THEN d.value END) AS ResponsibleToUserId,
               t.type AS ApprovalType, t.created_by AS CreatedBy, t.created_date AS CreatedDate
        FROM dbo.tb_parameter_sytem_temporary t
        JOIN dbo.tb_parameter_sytem_temporary_detail d ON d.tb_parameter_sytem_temporary_id = t.id
        WHERE t.table_name = N'um_user_group' AND t.type IN (N'A', N'M', N'D')
        GROUP BY t.id, t.type, t.created_by, t.created_date
    )
    SELECT @TotalCount = COUNT(*)
    FROM cte c
    LEFT JOIN dbo.um_user_app u ON u.user_id = c.UserId
    LEFT JOIN dbo.um_group_app g ON g.group_id = c.GroupId
    WHERE (@SearchTerm IS NULL OR c.UserId LIKE N'%' + @SearchTerm + N'%' OR u.full_name LIKE N'%' + @SearchTerm + N'%' OR g.group_name LIKE N'%' + @SearchTerm + N'%');

    DECLARE @sql nvarchar(max) = N'
        ;WITH cte AS (
            SELECT t.id AS Id,
                   MAX(CASE WHEN d.column_name = N''user_id'' THEN d.value END) AS UserId,
                   MAX(CASE WHEN d.column_name = N''group_id'' THEN d.value END) AS GroupId,
                   MAX(CASE WHEN d.column_name = N''can_add_edit'' THEN d.value END) AS CanAddEdit,
                   MAX(CASE WHEN d.column_name = N''can_delete'' THEN d.value END) AS CanDelete,
                   MAX(CASE WHEN d.column_name = N''can_read_only'' THEN d.value END) AS CanReadOnly,
                   MAX(CASE WHEN d.column_name = N''responsible_to_user_id'' THEN d.value END) AS ResponsibleToUserId,
                   t.type AS ApprovalType, t.created_by AS CreatedBy, t.created_date AS CreatedDate
            FROM dbo.tb_parameter_sytem_temporary t
            JOIN dbo.tb_parameter_sytem_temporary_detail d ON d.tb_parameter_sytem_temporary_id = t.id
            WHERE t.table_name = N''um_user_group'' AND t.type IN (N''A'', N''M'', N''D'')
            GROUP BY t.id, t.type, t.created_by, t.created_date
        )
        SELECT c.Id, c.UserId, u.full_name AS FullName, c.GroupId, g.group_name AS GroupName,
               c.CanAddEdit, c.CanDelete, c.CanReadOnly, c.ResponsibleToUserId,
               c.ApprovalType, u2.full_name AS RequesterFullName, c.CreatedDate
        FROM cte c
        LEFT JOIN dbo.um_user_app u ON u.user_id = c.UserId
        LEFT JOIN dbo.um_group_app g ON g.group_id = c.GroupId
        LEFT JOIN dbo.um_user_app u2 ON u2.user_id = c.CreatedBy
        WHERE (@search IS NULL OR c.UserId LIKE @search OR u.full_name LIKE @search OR g.group_name LIKE @search)
        ORDER BY ' + QUOTENAME(@SortColumn) + N' ' + @dir + N'
        OFFSET @offset ROWS FETCH NEXT @page_size ROWS ONLY;';

    EXEC sp_executesql @sql,
        N'@offset int, @page_size int, @search nvarchar(210)',
        @offset, @page_size, @search = @search;
END;
GO
