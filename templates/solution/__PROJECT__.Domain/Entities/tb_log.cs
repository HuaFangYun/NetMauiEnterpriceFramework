using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace __PROJECT__.Domain.Entities;

public partial class tb_log
{
    public long id { get; set; }

    [StringLength(255)]
    public string? entity { get; set; }

    [StringLength(50)]
    public string? log_type { get; set; }

    [StringLength(4000)]
    public string? log_msg { get; set; }

    public DateTime? created_date { get; set; }

    [StringLength(100)]
    public string? created_by { get; set; }
}
