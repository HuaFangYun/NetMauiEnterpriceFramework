using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace __PROJECT__.Domain.Entities;

public partial class um_user_refresh_token : BaseEntity
{
    public int id { get; set; }

    [StringLength(100)]
    public string? user_id { get; set; }

    [StringLength(255)]
    public string? refresh_token { get; set; }

    public DateTime expiry_date { get; set; }

    public bool is_revoked { get; set; }

    public bool? remember_me { get; set; }

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
