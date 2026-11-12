using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace __PROJECT__.Domain.Entities;

public partial class tb_parameter_system
{
    [StringLength(100)]
    public string table_name { get; set; } = string.Empty;

    [StringLength(100)]
    public string? description { get; set; }

    [StringLength(500)]
    public string? param_maker_url { get; set; }

    [StringLength(500)]
    public string? param_approval_url { get; set; }

    public bool? is_param_maker { get; set; }

    public bool? is_param_approval { get; set; }

    public bool? is_param_upload { get; set; }

    [StringLength(100)]
    public string? icon { get; set; }
}
