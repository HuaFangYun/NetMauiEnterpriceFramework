using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace __PROJECT__.Domain.Entities;

public partial class enum_menu_parent_level
{
    [StringLength(1)]
    public string parent_level_code { get; set; } = string.Empty;

    [StringLength(100)]
    public string? parent_level_name { get; set; }
}
