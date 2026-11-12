using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
