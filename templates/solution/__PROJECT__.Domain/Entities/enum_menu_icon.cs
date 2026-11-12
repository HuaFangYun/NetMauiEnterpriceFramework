using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace __PROJECT__.Domain.Entities;

public partial class enum_menu_icon
{
    [StringLength(255)]
    public string menu_icon_code { get; set; } = string.Empty;

    [StringLength(100)]
    public string? menu_icon_name { get; set; }

    [StringLength(255)]
    public string? menu_icon_name_alternate { get; set; }
}
