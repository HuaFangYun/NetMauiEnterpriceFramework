using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace __PROJECT__.Domain.Entities;

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
