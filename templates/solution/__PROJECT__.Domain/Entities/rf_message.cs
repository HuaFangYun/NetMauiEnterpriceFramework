using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace __PROJECT__.Domain.Entities;

public partial class rf_message : BaseEntity
{
    [StringLength(10)]
    public string message_code { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? message_name { get; set; }
}
