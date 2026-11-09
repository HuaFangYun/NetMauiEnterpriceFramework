USE [__DB_NAME__];
GO

CREATE OR ALTER PROCEDURE dbo.usp_v2_get_user_group_list
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
    IF @SortColumn NOT IN (N'UserId', N'FullName', N'GroupName', N'IsActive') SET @SortColumn = N'UserId';
    SET @SearchTerm = NULLIF(LTRIM(RTRIM(@SearchTerm)), N'');
    DECLARE @search nvarchar(210) = CASE WHEN @SearchTerm IS NULL THEN NULL ELSE N'%' + @SearchTerm + N'%' END;

    SELECT @TotalCount = COUNT(*)
    FROM dbo.um_user_group ug
    LEFT JOIN dbo.um_user_app u ON u.user_id = ug.user_id
    LEFT JOIN dbo.um_group_app g ON g.group_id = ug.group_id
    WHERE ug.is_deleted = 0
      AND (@SearchTerm IS NULL OR ug.user_id LIKE N'%' + @SearchTerm + N'%' OR u.full_name LIKE N'%' + @SearchTerm + N'%' OR g.group_name LIKE N'%' + @SearchTerm + N'%');

    DECLARE @sql nvarchar(max) = N'
        SELECT ug.user_id AS UserId, u.full_name AS FullName, ug.group_id AS GroupId, g.group_name AS GroupName,
               ug.can_add_edit AS CanAddEdit, ug.can_delete AS CanDelete, ug.can_read_only AS CanReadOnly,
               ug.is_active AS IsActive
        FROM dbo.um_user_group ug
        LEFT JOIN dbo.um_user_app u ON u.user_id = ug.user_id
        LEFT JOIN dbo.um_group_app g ON g.group_id = ug.group_id
        WHERE ug.is_deleted = 0
          AND (@search IS NULL OR ug.user_id LIKE @search OR u.full_name LIKE @search OR g.group_name LIKE @search)
        ORDER BY ' + QUOTENAME(@SortColumn) + N' ' + @dir + N'
        OFFSET @offset ROWS FETCH NEXT @page_size ROWS ONLY;';

    EXEC sp_executesql @sql,
        N'@offset int, @page_size int, @search nvarchar(210)',
        @offset, @page_size, @search = @search;
END;
GO
