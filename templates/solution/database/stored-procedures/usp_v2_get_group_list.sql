USE [__DB_NAME__];
GO

CREATE OR ALTER PROCEDURE dbo.usp_v2_get_group_list
    @PageNumber int = 1,
    @PageSize int = 10,
    @SearchTerm nvarchar(200) = NULL,
    @SortColumn nvarchar(100) = N'GroupId',
    @SortDirection nvarchar(4) = N'asc',
    @TotalCount int OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @page_size int = CASE WHEN @PageSize < 1 THEN 10 WHEN @PageSize > 200 THEN 200 ELSE @PageSize END;
    DECLARE @offset int = (CASE WHEN @PageNumber < 1 THEN 1 ELSE @PageNumber END - 1) * @page_size;
    DECLARE @dir nvarchar(4) = CASE WHEN LOWER(@SortDirection) = N'desc' THEN N'DESC' ELSE N'ASC' END;
    IF @SortColumn NOT IN (N'GroupId', N'GroupName', N'UserCount', N'IsActive') SET @SortColumn = N'GroupId';
    SET @SearchTerm = NULLIF(LTRIM(RTRIM(@SearchTerm)), N'');
    DECLARE @search nvarchar(210) = CASE WHEN @SearchTerm IS NULL THEN NULL ELSE N'%' + @SearchTerm + N'%' END;

    SELECT @TotalCount = COUNT(*)
    FROM dbo.um_group_app g
    WHERE g.is_deleted = 0
      AND (@SearchTerm IS NULL OR g.group_id LIKE N'%' + @SearchTerm + N'%' OR g.group_name LIKE N'%' + @SearchTerm + N'%');

    DECLARE @sql nvarchar(max) = N'
        SELECT g.group_id AS GroupId, g.group_name AS GroupName,
               (SELECT COUNT(*) FROM dbo.um_user_group ug WHERE ug.group_id = g.group_id AND ug.is_deleted = 0) AS UserCount,
               g.is_active AS IsActive, g.status_approval AS StatusApproval, g.created_date AS CreatedDate
        FROM dbo.um_group_app g
        WHERE g.is_deleted = 0
          AND (@search IS NULL OR g.group_id LIKE @search OR g.group_name LIKE @search)
        ORDER BY ' + QUOTENAME(@SortColumn) + N' ' + @dir + N'
        OFFSET @offset ROWS FETCH NEXT @page_size ROWS ONLY;';

    EXEC sp_executesql @sql,
        N'@offset int, @page_size int, @search nvarchar(210)',
        @offset, @page_size, @search = @search;
END;
GO
