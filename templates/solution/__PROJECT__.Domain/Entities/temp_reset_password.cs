using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace __PROJECT__.Domain.Entities;

public partial class temp_reset_password
{
    [StringLength(100)]
    public string user_id { get; set; } = string.Empty;

    [StringLength(100)]
    public string? new_password { get; set; }

    [StringLength(100)]
    public string? new_password_non_encrypt { get; set; }

    [StringLength(100)]
    public string? created_by { get; set; }

    public DateTime? created_date { get; set; }
}
