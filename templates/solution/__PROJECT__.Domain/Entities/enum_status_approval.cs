using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace __PROJECT__.Domain.Entities;

public partial class enum_status_approval
{
    public int status_code { get; set; }

    [StringLength(100)]
    public string? status_name { get; set; }
}
