using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace __PROJECT__.Domain.Entities;

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
