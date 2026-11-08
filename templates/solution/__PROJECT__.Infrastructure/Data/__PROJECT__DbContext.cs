using Microsoft.EntityFrameworkCore;

using __PROJECT__.Domain.Entities;

namespace __PROJECT__.Infrastructure.Data;

public class __PROJECT__DbContext : DbContext
{
    public __PROJECT__DbContext(DbContextOptions<__PROJECT__DbContext> options)
        : base(options)
    {
    }

    public DbSet<enum_approval_type> enum_approval_type => Set<enum_approval_type>();
    public DbSet<enum_gender> enum_gender => Set<enum_gender>();
    public DbSet<enum_menu_icon> enum_menu_icon => Set<enum_menu_icon>();
    public DbSet<enum_menu_parent_level> enum_menu_parent_level => Set<enum_menu_parent_level>();
    public DbSet<enum_status_approval> enum_status_approval => Set<enum_status_approval>();
    public DbSet<enum_track_list> enum_track_list => Set<enum_track_list>();

    public DbSet<tb_parameter_system> tb_parameter_system => Set<tb_parameter_system>();
    public DbSet<tb_parameter_system_detail> tb_parameter_system_detail => Set<tb_parameter_system_detail>();
    public DbSet<tb_parameter_sytem_temporary> tb_parameter_sytem_temporary => Set<tb_parameter_sytem_temporary>();
    public DbSet<tb_parameter_sytem_temporary_detail> tb_parameter_sytem_temporary_detail => Set<tb_parameter_sytem_temporary_detail>();

    public DbSet<temp_user_app_pending> temp_user_app_pending => Set<temp_user_app_pending>();
    public DbSet<temp_group_app_pending> temp_group_app_pending => Set<temp_group_app_pending>();
    public DbSet<temp_reset_password> temp_reset_password => Set<temp_reset_password>();

    public DbSet<um_user_app> um_user_app => Set<um_user_app>();
    public DbSet<um_user_group> um_user_group => Set<um_user_group>();
    public DbSet<um_group_app> um_group_app => Set<um_group_app>();
    public DbSet<um_user_app_flag> um_user_app_flag => Set<um_user_app_flag>();
    public DbSet<um_user_app_session> um_user_app_session => Set<um_user_app_session>();
    public DbSet<um_user_refresh_token> um_user_refresh_token => Set<um_user_refresh_token>();
    public DbSet<um_user_audit_trail> um_user_audit_trail => Set<um_user_audit_trail>();
    public DbSet<um_user_audit_trail_detail> um_user_audit_trail_detail => Set<um_user_audit_trail_detail>();

    public DbSet<um_menu> um_menu => Set<um_menu>();
    public DbSet<um_group_menu> um_group_menu => Set<um_group_menu>();

    public DbSet<tb_screen_menu> tb_screen_menu => Set<tb_screen_menu>();
    public DbSet<tb_screen_menu_child> tb_screen_menu_child => Set<tb_screen_menu_child>();
    public DbSet<tb_screen_menu_import_excel_process> tb_screen_menu_import_excel_process => Set<tb_screen_menu_import_excel_process>();
    public DbSet<tb_report_set> tb_report_set => Set<tb_report_set>();

    public DbSet<tb_notification> tb_notification => Set<tb_notification>();
    public DbSet<tb_notification_type> tb_notification_type => Set<tb_notification_type>();

    public DbSet<rf_default_value> rf_default_value => Set<rf_default_value>();
    public DbSet<rf_files_upload_type> rf_files_upload_type => Set<rf_files_upload_type>();
    public DbSet<rf_group_access_report_set> rf_group_access_report_set => Set<rf_group_access_report_set>();
    public DbSet<rf_message> rf_message => Set<rf_message>();

    public DbSet<proc_seq> proc_seq => Set<proc_seq>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureEnums(modelBuilder);
        ConfigureParameters(modelBuilder);
        ConfigureUsers(modelBuilder);
        ConfigureMenus(modelBuilder);
        ConfigureScreensAndReports(modelBuilder);
        ConfigureNotifications(modelBuilder);
        ConfigureReferences(modelBuilder);

        modelBuilder.Entity<proc_seq>().HasKey(e => e.id);
    }

    private static void ConfigureEnums(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<enum_approval_type>().HasKey(e => e.approval_type_code);
        modelBuilder.Entity<enum_gender>().HasKey(e => e.gender_code);
        modelBuilder.Entity<enum_menu_icon>().HasKey(e => e.menu_icon_code);
        modelBuilder.Entity<enum_menu_parent_level>().HasKey(e => e.parent_level_code);
        modelBuilder.Entity<enum_status_approval>().HasKey(e => e.status_code);
        modelBuilder.Entity<enum_status_approval>().Property(e => e.status_code).ValueGeneratedNever();
        modelBuilder.Entity<enum_track_list>().HasKey(e => e.tr_code);
    }

    private static void ConfigureParameters(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<tb_parameter_system>().HasKey(e => e.table_name);

        modelBuilder.Entity<tb_parameter_system_detail>()
            .HasKey(e => new { e.table_name, e.field_name, e.order_seq });

        modelBuilder.Entity<tb_parameter_sytem_temporary>().HasKey(e => e.id);

        modelBuilder.Entity<tb_parameter_sytem_temporary_detail>()
            .HasKey(e => new { e.tb_parameter_sytem_temporary_id, e.column_name });

        modelBuilder.Entity<tb_parameter_system_detail>()
            .HasOne<tb_parameter_system>()
            .WithMany()
            .HasForeignKey(e => e.table_name)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<tb_parameter_sytem_temporary_detail>()
            .HasOne<tb_parameter_sytem_temporary>()
            .WithMany()
            .HasForeignKey(e => e.tb_parameter_sytem_temporary_id)
            .OnDelete(DeleteBehavior.Cascade);
    }

    private static void ConfigureUsers(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<um_user_app>().HasKey(e => e.user_id);
        modelBuilder.Entity<um_user_group>().HasKey(e => e.user_id);
        modelBuilder.Entity<um_group_app>().HasKey(e => e.group_id);
        modelBuilder.Entity<um_user_app_flag>().HasKey(e => e.user_id);
        modelBuilder.Entity<um_user_app_session>().HasKey(e => e.user_id);
        modelBuilder.Entity<um_user_refresh_token>().HasKey(e => e.id);
        modelBuilder.Entity<um_user_audit_trail>().HasKey(e => e.id);

        modelBuilder.Entity<um_user_audit_trail_detail>()
            .HasKey(e => new { e.um_user_audit_trail_id, e.column_name });

        modelBuilder.Entity<temp_user_app_pending>().HasKey(e => e.id);
        modelBuilder.Entity<temp_group_app_pending>().HasKey(e => e.id);
        modelBuilder.Entity<temp_reset_password>().HasKey(e => e.user_id);

        modelBuilder.Entity<um_user_app_flag>()
            .HasOne<um_user_app>()
            .WithOne()
            .HasForeignKey<um_user_app_flag>(e => e.user_id)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<um_user_refresh_token>()
            .HasOne<um_user_app>()
            .WithMany()
            .HasForeignKey(e => e.user_id)
            .OnDelete(DeleteBehavior.Cascade);
    }

    private static void ConfigureMenus(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<um_menu>().HasKey(e => e.id);
        modelBuilder.Entity<um_menu>().HasIndex(e => e.menu_unique).IsUnique();
        modelBuilder.Entity<um_group_menu>().HasKey(e => e.id);

        modelBuilder.Entity<um_group_menu>()
            .HasOne<um_menu>()
            .WithMany()
            .HasForeignKey(e => e.menu_id)
            .HasPrincipalKey(e => e.id)
            .OnDelete(DeleteBehavior.Cascade);
    }

    private static void ConfigureScreensAndReports(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<tb_screen_menu>().HasKey(e => e.id);
        modelBuilder.Entity<tb_screen_menu_child>().HasKey(e => e.screen_menu_child_id);
        modelBuilder.Entity<tb_screen_menu_import_excel_process>().HasKey(e => e.screen_menu_import_excel_process_id);
        modelBuilder.Entity<tb_report_set>().HasKey(e => e.id);

        modelBuilder.Entity<tb_screen_menu>()
            .HasOne<tb_screen_menu_child>()
            .WithMany()
            .HasForeignKey(e => e.screen_menu_child_id)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<tb_screen_menu>()
            .HasOne<tb_screen_menu_import_excel_process>()
            .WithMany()
            .HasForeignKey(e => e.screen_menu_import_excel_process_id)
            .OnDelete(DeleteBehavior.SetNull);
    }

    private static void ConfigureNotifications(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<tb_notification>().HasKey(e => e.id);
        modelBuilder.Entity<tb_notification_type>().HasKey(e => e.id);

        modelBuilder.Entity<tb_notification>()
            .HasOne<tb_notification_type>()
            .WithMany()
            .HasForeignKey(e => e.notification_type_id)
            .OnDelete(DeleteBehavior.SetNull);
    }

    private static void ConfigureReferences(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<rf_default_value>().HasKey(e => e.def_code);
        modelBuilder.Entity<rf_files_upload_type>().HasKey(e => e.files_upload_type_code);
        modelBuilder.Entity<rf_group_access_report_set>().HasKey(e => e.id);
        modelBuilder.Entity<rf_message>().HasKey(e => e.message_code);
    }
}
