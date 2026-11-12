using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace __PROJECT__.Domain.Entities;

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
