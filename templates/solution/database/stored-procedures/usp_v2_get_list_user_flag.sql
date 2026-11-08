USE [__DB_NAME__];
GO

CREATE OR ALTER PROCEDURE dbo.usp_v2_get_list_user_flag
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
    IF @SortColumn NOT IN (N'UserId', N'FullName', N'IsLogin', N'IsLocked', N'LastLoginDate') SET @SortColumn = N'UserId';
    SET @SearchTerm = NULLIF(LTRIM(RTRIM(@SearchTerm)), N'');
    DECLARE @search nvarchar(210) = CASE WHEN @SearchTerm IS NULL THEN NULL ELSE N'%' + @SearchTerm + N'%' END;

    SELECT @TotalCount = COUNT(*)
    FROM dbo.um_user_app u
    LEFT JOIN dbo.um_user_app_flag f ON f.user_id = u.user_id
    WHERE u.is_deleted = 0
      AND (@SearchTerm IS NULL OR u.user_id LIKE N'%' + @SearchTerm + N'%' OR u.full_name LIKE N'%' + @SearchTerm + N'%');

    DECLARE @sql nvarchar(max) = N'
        SELECT u.user_id AS UserId, u.full_name AS FullName,
               ISNULL(f.is_login, 0) AS IsLogin, ISNULL(f.is_locked, 0) AS IsLocked,
               ISNULL(f.is_first_login, 0) AS IsFirstLogin, f.last_login_date AS LastLoginDate,
               f.ip_address AS IpAddress, f.failed_password_attempt_count AS FailedPasswordAttempt,
               f.comment AS Comment
        FROM dbo.um_user_app u
        LEFT JOIN dbo.um_user_app_flag f ON f.user_id = u.user_id
        WHERE u.is_deleted = 0
          AND (@search IS NULL OR u.user_id LIKE @search OR u.full_name LIKE @search)
        ORDER BY ' + QUOTENAME(@SortColumn) + N' ' + @dir + N'
        OFFSET @offset ROWS FETCH NEXT @page_size ROWS ONLY;';

    EXEC sp_executesql @sql,
        N'@offset int, @page_size int, @search nvarchar(210)',
        @offset, @page_size, @search = @search;
END;
GO
