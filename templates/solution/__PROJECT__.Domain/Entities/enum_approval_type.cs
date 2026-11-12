using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace __PROJECT__.Domain.Entities;

public partial class enum_approval_type
{
    [StringLength(2)]
    public string approval_type_code { get; set; } = string.Empty;

    [StringLength(100)]
    public string? approval_type_name { get; set; }

    public bool? is_active { get; set; }
}
