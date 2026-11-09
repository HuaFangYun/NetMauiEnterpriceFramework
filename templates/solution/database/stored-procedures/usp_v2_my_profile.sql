USE [__DB_NAME__];
GO

CREATE OR ALTER PROCEDURE dbo.usp_v2_my_profile
    @UserId nvarchar(100)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT u.user_id AS UserId, u.full_name AS FullName, u.email AS Email,
           u.phone_number AS PhoneNumber,
           u.gender_id AS GenderId, e.gender_name AS GenderName,
           ug.group_id AS GroupId, g.group_name AS GroupName,
           u.is_active AS IsActive, u.status_approval AS StatusApproval
    FROM dbo.um_user_app u
    LEFT JOIN dbo.um_user_group ug ON ug.user_id = u.user_id AND ug.is_deleted = 0
    LEFT JOIN dbo.um_group_app g ON g.group_id = ug.group_id AND g.is_deleted = 0
    LEFT JOIN dbo.enum_gender e ON e.gender_code = u.gender_id
    WHERE u.user_id = @UserId AND u.is_deleted = 0;
END;
GO
