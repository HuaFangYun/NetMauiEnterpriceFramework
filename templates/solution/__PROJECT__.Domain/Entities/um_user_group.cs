using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace __PROJECT__.Domain.Entities;

public partial class um_user_group : BaseEntity
{
    [StringLength(50)]
    public string user_id { get; set; } = string.Empty;

    [StringLength(100)]
    public string? group_id { get; set; }

    public bool can_add_edit { get; set; }

    public bool can_delete { get; set; }

    public bool can_read_only { get; set; }
}
