using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace __PROJECT__.Domain.Entities;

public partial class tb_audit_trail_detail
{
    public Guid tb_audit_trail_id { get; set; }

    public string column_name { get; set; } = string.Empty;

    public string? value { get; set; }

    public string? value_before { get; set; }
}
