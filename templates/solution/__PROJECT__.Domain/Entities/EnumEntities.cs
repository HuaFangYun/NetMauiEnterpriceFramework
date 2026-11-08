using System.ComponentModel.DataAnnotations;

namespace __PROJECT__.Domain.Entities;

public partial class enum_approval_type
{
    [StringLength(2)]
    public string approval_type_code { get; set; } = string.Empty;

    [StringLength(100)]
    public string? approval_type_name { get; set; }

    public bool? is_active { get; set; }
}

public partial class enum_gender
{
    [StringLength(1)]
    public string gender_code { get; set; } = string.Empty;

    [StringLength(100)]
    public string? gender_name { get; set; }

    [StringLength(50)]
    public string? gender_T24_code { get; set; }

    [StringLength(50)]
    public string? gender_dukcapil_code { get; set; }
}

public partial class enum_menu_icon
{
    [StringLength(255)]
    public string menu_icon_code { get; set; } = string.Empty;

    [StringLength(100)]
    public string? menu_icon_name { get; set; }

    [StringLength(255)]
    public string? menu_icon_name_alternate { get; set; }
}

public partial class enum_menu_parent_level
{
    [StringLength(1)]
    public string parent_level_code { get; set; } = string.Empty;

    [StringLength(100)]
    public string? parent_level_name { get; set; }
}

public partial class enum_status_approval
{
    public int status_code { get; set; }

    [StringLength(100)]
    public string? status_name { get; set; }
}

public partial class enum_track_list
{
    [StringLength(100)]
    public string tr_code { get; set; } = string.Empty;

    [StringLength(100)]
    public string? tr_name { get; set; }

    public bool? is_active { get; set; }

    public int? level_seq { get; set; }

    [StringLength(100)]
    public string? tr_short_name { get; set; }

    [StringLength(100)]
    public string? tr_filter_name { get; set; }
}
