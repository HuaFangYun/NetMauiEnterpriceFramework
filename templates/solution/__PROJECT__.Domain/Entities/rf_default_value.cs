using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
