using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace __PROJECT__.Infrastructure.Data;

public class __PROJECT__DbContextFactory : IDesignTimeDbContextFactory<__PROJECT__DbContext>
{
    public __PROJECT__DbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<__PROJECT__DbContext>()
            .UseSqlServer("Server=localhost,1433;Database=__DB_NAME__;Integrated Security=True;TrustServerCertificate=True;Encrypt=False")
            .Options;

        return new __PROJECT__DbContext(options);
    }
}

public class AuditLogDbContextFactory : IDesignTimeDbContextFactory<AuditLogDbContext>
{
    public AuditLogDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AuditLogDbContext>()
            .UseSqlServer("Server=localhost,1433;Database=__AUDIT_DB_NAME__;Integrated Security=True;TrustServerCertificate=True;Encrypt=False")
            .Options;

        return new AuditLogDbContext(options);
    }
}
