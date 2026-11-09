using System.ComponentModel.DataAnnotations;

namespace __PROJECT__.Domain.Entities;

public partial class um_user_app : BaseEntity
{
    [StringLength(100)]
    public string user_id { get; set; } = string.Empty;

    [StringLength(100)]
    public string? full_name { get; set; }

    [StringLength(255)]
    public string? password { get; set; }

    [StringLength(100)]
    public string? email { get; set; }

    [StringLength(20)]
    public string? phone_number { get; set; }

    [StringLength(1)]
    public string? gender_id { get; set; }

    public DateTime? last_password_change_date { get; set; }
}

public partial class um_user_group : BaseEntity
{
    [StringLength(50)]
    public string user_id { get; set; } = string.Empty;

    [StringLength(100)]
    public string? group_id { get; set; }

    public bool can_add_edit { get; set; }

    public bool can_delete { get; set; }

    public bool can_read_only { get; set; }
}

public partial class um_group_app : BaseEntity
{
    [StringLength(50)]
    public string group_id { get; set; } = string.Empty;

    [StringLength(100)]
    public string? group_name { get; set; }
}

public partial class um_user_app_flag
{
    [StringLength(100)]
    public string user_id { get; set; } = string.Empty;

    public bool? is_first_login { get; set; }

    public bool? is_locked { get; set; }

    public bool? is_login { get; set; }

    [StringLength(50)]
    public string? ip_address { get; set; }

    public DateTime? last_login_date { get; set; }

    public DateTime? last_password_changed_date { get; set; }

    public DateTime? last_locked_out_date { get; set; }

    public int? failed_password_attempt_count { get; set; }

    public DateTime? failed_password_attempt_date { get; set; }

    [StringLength(1000)]
    public string? comment { get; set; }

    [StringLength(100)]
    public string? session_id { get; set; }
}

public partial class um_user_app_session
{
    [StringLength(100)]
    public string user_id { get; set; } = string.Empty;

    [StringLength(100)]
    public string? session_id { get; set; }

    [StringLength(100)]
    public string? connection_id { get; set; }

    public DateTime? last_activity { get; set; }
}

public partial class um_user_refresh_token : BaseEntity
{
    public int id { get; set; }

    [StringLength(100)]
    public string? user_id { get; set; }

    [StringLength(255)]
    public string? refresh_token { get; set; }

    public DateTime expiry_date { get; set; }

    public bool is_revoked { get; set; }

    [StringLength(255)]
    public string? device_info { get; set; }

    [StringLength(50)]
    public string? ip_address { get; set; }

    public DateTime? last_used_date { get; set; }

    [StringLength(100)]
    public string? revoked_by { get; set; }

    [StringLength(255)]
    public string? revoked_reason { get; set; }
}

public partial class um_user_audit_trail
{
    public Guid id { get; set; }

    public bool? type { get; set; }

    [StringLength(100)]
    public string? created_by { get; set; }

    public DateTime? created_date { get; set; }
}

public partial class um_user_audit_trail_detail
{
    public Guid um_user_audit_trail_id { get; set; }

    [StringLength(500)]
    public string column_name { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? value { get; set; }

    [StringLength(1000)]
    public string? value_before { get; set; }
}
