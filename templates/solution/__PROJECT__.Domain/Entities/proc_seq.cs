using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace __PROJECT__.Domain.Entities;

public partial class proc_seq
{
    public int id { get; set; }

    [StringLength(100)]
    public string? proc_type { get; set; }

    [Column("proc_seq")]
    public long? proc_seq1 { get; set; }

    public DateTime? proc_date { get; set; }
}
