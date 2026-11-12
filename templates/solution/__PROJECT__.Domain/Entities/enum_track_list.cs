using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace __PROJECT__.Domain.Entities;

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
