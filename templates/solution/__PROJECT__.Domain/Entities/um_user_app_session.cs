using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace __PROJECT__.Domain.Entities;

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
