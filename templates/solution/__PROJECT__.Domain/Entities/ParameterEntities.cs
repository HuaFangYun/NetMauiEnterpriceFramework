using System.ComponentModel.DataAnnotations;

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

public partial class tb_parameter_sytem_temporary
{
    public Guid id { get; set; }

    [StringLength(100)]
    public string? table_name { get; set; }

    [StringLength(50)]
    public string? type { get; set; }

    public DateTime? created_date { get; set; }

    [StringLength(100)]
    public string? created_by { get; set; }
}

public partial class tb_parameter_sytem_temporary_detail
{
    public Guid tb_parameter_sytem_temporary_id { get; set; }

    [StringLength(1000)]
    public string column_name { get; set; } = string.Empty;

    public string? value { get; set; }

    public string? value_before { get; set; }
}
