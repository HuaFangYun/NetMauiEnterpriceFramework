using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace __PROJECT__.Domain.Entities;

public partial class um_menu : BaseEntity
{
    public long id { get; set; }

    [StringLength(20)]
    public string menu_unique { get; set; } = string.Empty;

    [StringLength(100)]
    public string? menu_name { get; set; }

    [StringLength(500)]
    public string? menu_url { get; set; }

    [StringLength(1)]
    public string? parent_level_id { get; set; }

    [StringLength(20)]
    public string? menu_unique_parent { get; set; }

    [StringLength(100)]
    public string? menu_icon { get; set; }

    public int? order_seq { get; set; }

    [StringLength(255)]
    public string? component_type { get; set; }
}
