USE [__DB_NAME__];
GO

IF EXISTS (SELECT 1 FROM sys.synonyms WHERE name = N'tb_audit_trail')
    DROP SYNONYM [dbo].[tb_audit_trail];
GO

CREATE SYNONYM [dbo].[tb_audit_trail]
    FOR [__AUDIT_DB_NAME__].[dbo].[tb_audit_trail];
GO

IF EXISTS (SELECT 1 FROM sys.synonyms WHERE name = N'tb_audit_trail_detail')
    DROP SYNONYM [dbo].[tb_audit_trail_detail];
GO

CREATE SYNONYM [dbo].[tb_audit_trail_detail]
    FOR [__AUDIT_DB_NAME__].[dbo].[tb_audit_trail_detail];
GO

IF EXISTS (SELECT 1 FROM sys.synonyms WHERE name = N'tb_log')
    DROP SYNONYM [dbo].[tb_log];
GO

CREATE SYNONYM [dbo].[tb_log]
    FOR [__AUDIT_DB_NAME__].[dbo].[tb_log];
GO
