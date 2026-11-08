using Microsoft.EntityFrameworkCore;

using __PROJECT__.Domain.Entities;

namespace __PROJECT__.Infrastructure.Data;

public class AuditLogDbContext : DbContext
{
    public AuditLogDbContext(DbContextOptions<AuditLogDbContext> options)
        : base(options)
    {
    }

    public DbSet<tb_audit_trail> tb_audit_trail => Set<tb_audit_trail>();
    public DbSet<tb_audit_trail_detail> tb_audit_trail_detail => Set<tb_audit_trail_detail>();
    public DbSet<tb_log> tb_log => Set<tb_log>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<tb_audit_trail>().HasKey(e => e.id);
        modelBuilder.Entity<tb_audit_trail_detail>()
            .HasKey(e => new { e.tb_audit_trail_id, e.column_name });
        modelBuilder.Entity<tb_log>().HasKey(e => e.id);
    }
}
