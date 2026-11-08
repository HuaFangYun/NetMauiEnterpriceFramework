USE [__DB_NAME__];
GO

CREATE OR ALTER PROCEDURE dbo.usp_v2_get_waiting_approval_users
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
    IF @SortColumn NOT IN (N'UserId', N'FullName', N'Email', N'CreatedDate') SET @SortColumn = N'UserId';
    SET @SearchTerm = NULLIF(LTRIM(RTRIM(@SearchTerm)), N'');
    DECLARE @search nvarchar(210) = CASE WHEN @SearchTerm IS NULL THEN NULL ELSE N'%' + @SearchTerm + N'%' END;

    SELECT @TotalCount = COUNT(*)
    FROM dbo.temp_user_app_pending p
    LEFT JOIN dbo.um_user_app u ON u.user_id = p.created_by
    WHERE (@SearchTerm IS NULL OR p.user_id LIKE N'%' + @SearchTerm + N'%' OR p.full_name LIKE N'%' + @SearchTerm + N'%' OR p.email LIKE N'%' + @SearchTerm + N'%');

    DECLARE @sql nvarchar(max) = N'
        SELECT p.id AS Id, p.user_id AS UserId, p.full_name AS FullName, p.email AS Email,
               p.group_id AS GroupId, g.group_name AS GroupName, p.approval_type_id AS ApprovalType,
               u.full_name AS RequesterFullName, p.created_date AS CreatedDate
        FROM dbo.temp_user_app_pending p
        LEFT JOIN dbo.um_user_app u ON u.user_id = p.created_by
        LEFT JOIN dbo.um_group_app g ON g.group_id = p.group_id
        WHERE (@search IS NULL OR p.user_id LIKE @search OR p.full_name LIKE @search OR p.email LIKE @search)
        ORDER BY ' + QUOTENAME(@SortColumn) + N' ' + @dir + N'
        OFFSET @offset ROWS FETCH NEXT @page_size ROWS ONLY;';

    EXEC sp_executesql @sql,
        N'@offset int, @page_size int, @search nvarchar(210)',
        @offset, @page_size, @search = @search;
END;
GO
