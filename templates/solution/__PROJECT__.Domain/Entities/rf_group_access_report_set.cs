using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace __PROJECT__.Domain.Entities;

public partial class rf_group_access_report_set : BaseEntity
{
    [StringLength(10)]
    public string id { get; set; } = string.Empty;

    [StringLength(500)]
    public string? group_id { get; set; }

    [StringLength(500)]
    public string? report_set_id { get; set; }
}
