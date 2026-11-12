using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace __PROJECT__.Domain.Entities;

public partial class tb_parameter_system_detail
{
    [StringLength(100)]
    public string table_name { get; set; } = string.Empty;

    [StringLength(500)]
    public string field_name { get; set; } = string.Empty;

    public int order_seq { get; set; }

    [StringLength(500)]
    public string? field_desc { get; set; }

    public bool? field_key { get; set; }

    [StringLength(100)]
    public string? field_type { get; set; }

    [StringLength(2000)]
    public string? field_reff { get; set; }

    public bool? is_mandatory { get; set; }

    public bool? is_group_by { get; set; }

    public bool? is_orderable { get; set; }

    [StringLength(2000)]
    public string? add_class { get; set; }
}
