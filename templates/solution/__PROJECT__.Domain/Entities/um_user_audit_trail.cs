using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace __PROJECT__.Domain.Entities;

public partial class um_user_audit_trail
{
    public Guid id { get; set; }

    public bool? type { get; set; }

    [StringLength(100)]
    public string? created_by { get; set; }

    public DateTime? created_date { get; set; }
}
