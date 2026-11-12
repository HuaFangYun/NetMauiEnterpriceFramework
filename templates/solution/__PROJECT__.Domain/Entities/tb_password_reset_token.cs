using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace __PROJECT__.Domain.Entities;

public partial class tb_password_reset_token
{
    public Guid id { get; set; }

    [StringLength(100)]
    public string user_id { get; set; } = string.Empty;

    [StringLength(128)]
    public string token_hash { get; set; } = string.Empty;

    public DateTime expires_at { get; set; }

    public DateTime? used_at { get; set; }

    [StringLength(50)]
    public string? created_by_ip { get; set; }

    public DateTime? created_date { get; set; }
}
