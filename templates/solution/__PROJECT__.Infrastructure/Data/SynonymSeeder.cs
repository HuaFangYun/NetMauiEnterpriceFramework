using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace __PROJECT__.Infrastructure.Data;

public static class SynonymSeeder
{
    public static async Task EnsureAsync(
        __PROJECT__DbContext context,
        string auditConnectionString,
        CancellationToken cancellationToken = default)
    {
        var auditBuilder = new SqlConnectionStringBuilder(auditConnectionString);
        var auditDatabase = auditBuilder.InitialCatalog;
        if (string.IsNullOrWhiteSpace(auditDatabase))
        {
            return;
        }

        var safeDatabase = auditDatabase.Replace("]", "]]", StringComparison.Ordinal);
        var sql = $"""
            IF NOT EXISTS (SELECT 1 FROM sys.synonyms WHERE name = N'tb_audit_trail')
                EXEC(N'CREATE SYNONYM [dbo].[tb_audit_trail] FOR [{safeDatabase}].[dbo].[tb_audit_trail]');
            IF NOT EXISTS (SELECT 1 FROM sys.synonyms WHERE name = N'tb_audit_trail_detail')
                EXEC(N'CREATE SYNONYM [dbo].[tb_audit_trail_detail] FOR [{safeDatabase}].[dbo].[tb_audit_trail_detail]');
            IF NOT EXISTS (SELECT 1 FROM sys.synonyms WHERE name = N'tb_log')
                EXEC(N'CREATE SYNONYM [dbo].[tb_log] FOR [{safeDatabase}].[dbo].[tb_log]');
            """;

        await context.Database.ExecuteSqlRawAsync(sql, cancellationToken);
    }
}
