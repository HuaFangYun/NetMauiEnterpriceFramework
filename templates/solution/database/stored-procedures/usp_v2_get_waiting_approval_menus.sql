USE [__DB_NAME__];
GO

CREATE OR ALTER PROCEDURE dbo.usp_v2_get_waiting_approval_menus
    @PageNumber int = 1,
    @PageSize int = 10,
    @SearchTerm nvarchar(200) = NULL,
    @SortColumn nvarchar(100) = N'MenuUnique',
    @SortDirection nvarchar(4) = N'asc',
    @TotalCount int OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @page_size int = CASE WHEN @PageSize < 1 THEN 10 WHEN @PageSize > 200 THEN 200 ELSE @PageSize END;
    DECLARE @offset int = (CASE WHEN @PageNumber < 1 THEN 1 ELSE @PageNumber END - 1) * @page_size;
    DECLARE @dir nvarchar(4) = CASE WHEN LOWER(@SortDirection) = N'desc' THEN N'DESC' ELSE N'ASC' END;
    IF @SortColumn NOT IN (N'MenuUnique', N'MenuName', N'CreatedDate') SET @SortColumn = N'MenuUnique';
    SET @SearchTerm = NULLIF(LTRIM(RTRIM(@SearchTerm)), N'');
    DECLARE @search nvarchar(210) = CASE WHEN @SearchTerm IS NULL THEN NULL ELSE N'%' + @SearchTerm + N'%' END;

    ;WITH cte AS (
        SELECT t.id AS Id,
               MAX(CASE WHEN d.column_name = N'menu_unique' THEN d.value END) AS MenuUnique,
               MAX(CASE WHEN d.column_name = N'menu_name' THEN d.value END) AS MenuName,
               MAX(CASE WHEN d.column_name = N'menu_url' THEN d.value END) AS MenuUrl,
               MAX(CASE WHEN d.column_name = N'parent_level_id' THEN d.value END) AS ParentLevelId,
               MAX(CASE WHEN d.column_name = N'menu_unique_parent' THEN d.value END) AS MenuUniqueParent,
               MAX(CASE WHEN d.column_name = N'menu_icon' THEN d.value END) AS MenuIcon,
               MAX(CASE WHEN d.column_name = N'order_seq' THEN d.value END) AS OrderSeq,
               MAX(CASE WHEN d.column_name = N'component_type' THEN d.value END) AS ComponentType,
               t.type AS ApprovalType, t.created_by AS CreatedBy, t.created_date AS CreatedDate
        FROM dbo.tb_parameter_sytem_temporary t
        JOIN dbo.tb_parameter_sytem_temporary_detail d ON d.tb_parameter_sytem_temporary_id = t.id
        WHERE t.table_name = N'um_menu' AND t.type IN (N'A', N'M', N'D')
        GROUP BY t.id, t.type, t.created_by, t.created_date
    )
    SELECT @TotalCount = COUNT(*)
    FROM cte c
    WHERE (@SearchTerm IS NULL OR c.MenuUnique LIKE N'%' + @SearchTerm + N'%' OR c.MenuName LIKE N'%' + @SearchTerm + N'%');

    DECLARE @sql nvarchar(max) = N'
        ;WITH cte AS (
            SELECT t.id AS Id,
                   MAX(CASE WHEN d.column_name = N''menu_unique'' THEN d.value END) AS MenuUnique,
                   MAX(CASE WHEN d.column_name = N''menu_name'' THEN d.value END) AS MenuName,
                   MAX(CASE WHEN d.column_name = N''menu_url'' THEN d.value END) AS MenuUrl,
                   MAX(CASE WHEN d.column_name = N''parent_level_id'' THEN d.value END) AS ParentLevelId,
                   MAX(CASE WHEN d.column_name = N''menu_unique_parent'' THEN d.value END) AS MenuUniqueParent,
                   MAX(CASE WHEN d.column_name = N''menu_icon'' THEN d.value END) AS MenuIcon,
                   MAX(CASE WHEN d.column_name = N''order_seq'' THEN d.value END) AS OrderSeq,
                   MAX(CASE WHEN d.column_name = N''component_type'' THEN d.value END) AS ComponentType,
                   t.type AS ApprovalType, t.created_by AS CreatedBy, t.created_date AS CreatedDate
            FROM dbo.tb_parameter_sytem_temporary t
            JOIN dbo.tb_parameter_sytem_temporary_detail d ON d.tb_parameter_sytem_temporary_id = t.id
            WHERE t.table_name = N''um_menu'' AND t.type IN (N''A'', N''M'', N''D'')
            GROUP BY t.id, t.type, t.created_by, t.created_date
        )
        SELECT c.Id, c.MenuUnique, c.MenuName, c.MenuUrl, c.ParentLevelId, c.MenuUniqueParent,
               c.MenuIcon, c.OrderSeq, c.ComponentType, c.ApprovalType, u.full_name AS RequesterFullName, c.CreatedDate
        FROM cte c
        LEFT JOIN dbo.um_user_app u ON u.user_id = c.CreatedBy
        WHERE (@search IS NULL OR c.MenuUnique LIKE @search OR c.MenuName LIKE @search)
        ORDER BY ' + QUOTENAME(@SortColumn) + N' ' + @dir + N'
        OFFSET @offset ROWS FETCH NEXT @page_size ROWS ONLY;';

    EXEC sp_executesql @sql,
        N'@offset int, @page_size int, @search nvarchar(210)',
        @offset, @page_size, @search = @search;
END;
GO
