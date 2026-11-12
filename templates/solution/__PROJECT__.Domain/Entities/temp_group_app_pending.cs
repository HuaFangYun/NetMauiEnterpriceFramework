using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace __PROJECT__.Domain.Entities;

public partial class temp_group_app_pending
{
    public long id { get; set; }

    [StringLength(100)]
    public string group_id { get; set; } = string.Empty;

    [StringLength(100)]
    public string group_name { get; set; } = string.Empty;

    [StringLength(1)]
    public string? approval_type_id { get; set; }

    [StringLength(100)]
    public string? created_by { get; set; }

    public DateTime? created_date { get; set; }

    public bool? is_active { get; set; }
}
