USE [__DB_NAME__];
GO

CREATE OR ALTER PROCEDURE dbo.usp_v2_get_user_list
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
    IF @SortColumn NOT IN (N'UserId', N'FullName', N'Email', N'GroupName', N'IsActive') SET @SortColumn = N'UserId';
    SET @SearchTerm = NULLIF(LTRIM(RTRIM(@SearchTerm)), N'');
    DECLARE @search nvarchar(210) = CASE WHEN @SearchTerm IS NULL THEN NULL ELSE N'%' + @SearchTerm + N'%' END;

    SELECT @TotalCount = COUNT(*)
    FROM dbo.um_user_app u
    LEFT JOIN dbo.um_user_group ug ON ug.user_id = u.user_id AND ug.is_deleted = 0
    LEFT JOIN dbo.um_group_app g ON g.group_id = ug.group_id AND g.is_deleted = 0
    WHERE u.is_deleted = 0
      AND (@SearchTerm IS NULL OR u.user_id LIKE N'%' + @SearchTerm + N'%'
           OR u.full_name LIKE N'%' + @SearchTerm + N'%'
           OR u.email LIKE N'%' + @SearchTerm + N'%');

    DECLARE @sql nvarchar(max) = N'
        SELECT u.user_id AS UserId, u.full_name AS FullName, u.email AS Email,
               u.phone_number AS PhoneNumber, u.kode_cabang AS KodeCabang,
               u.gender_id AS GenderId, e.gender_name AS GenderName,
               u.nomor_induk_karyawan AS NomorIndukKaryawan,
               ug.group_id AS GroupId, g.group_name AS GroupName,
               u.is_active AS IsActive, ISNULL(f.is_locked, 0) AS IsLocked,
               f.last_login_date AS LastLoginDate
        FROM dbo.um_user_app u
        LEFT JOIN dbo.um_user_group ug ON ug.user_id = u.user_id AND ug.is_deleted = 0
        LEFT JOIN dbo.um_group_app g ON g.group_id = ug.group_id AND g.is_deleted = 0
        LEFT JOIN dbo.um_user_app_flag f ON f.user_id = u.user_id
        LEFT JOIN dbo.enum_gender e ON e.gender_code = u.gender_id
        WHERE u.is_deleted = 0
          AND (@search IS NULL OR u.user_id LIKE @search OR u.full_name LIKE @search OR u.email LIKE @search)
        ORDER BY ' + QUOTENAME(@SortColumn) + N' ' + @dir + N'
        OFFSET @offset ROWS FETCH NEXT @page_size ROWS ONLY;';

    EXEC sp_executesql @sql,
        N'@offset int, @page_size int, @search nvarchar(210)',
        @offset, @page_size, @search = @search;
END;
GO
