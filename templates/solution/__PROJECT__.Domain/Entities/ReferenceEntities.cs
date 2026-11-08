using System.ComponentModel.DataAnnotations;

namespace __PROJECT__.Domain.Entities;

public partial class rf_default_value : BaseEntity
{
    [StringLength(10)]
    public string def_code { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? def_name { get; set; }

    [StringLength(1000)]
    public string? def_value { get; set; }
}

public partial class rf_files_upload_type : BaseEntity
{
    [StringLength(10)]
    public string files_upload_type_code { get; set; } = string.Empty;

    [StringLength(50)]
    public string? files_upload_type_T24_code { get; set; }

    [StringLength(100)]
    public string? files_upload_type_name { get; set; }
}

public partial class rf_group_access_report_set : BaseEntity
{
    [StringLength(10)]
    public string id { get; set; } = string.Empty;

    [StringLength(500)]
    public string? group_id { get; set; }

    [StringLength(500)]
    public string? report_set_id { get; set; }
}

public partial class rf_message : BaseEntity
{
    [StringLength(10)]
    public string message_code { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? message_name { get; set; }
}
