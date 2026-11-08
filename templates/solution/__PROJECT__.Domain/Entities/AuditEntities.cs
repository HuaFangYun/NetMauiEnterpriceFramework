using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace __PROJECT__.Domain.Entities;

public partial class tb_audit_trail
{
    public Guid id { get; set; }

    [StringLength(255)]
    public string? entity { get; set; }

    [StringLength(100)]
    public string? table_name { get; set; }

    [StringLength(50)]
    public string? type { get; set; }

    public DateTime? created_date { get; set; }

    [StringLength(100)]
    public string? created_by { get; set; }
}

public partial class tb_audit_trail_detail
{
    public Guid tb_audit_trail_id { get; set; }

    public string column_name { get; set; } = string.Empty;

    public string? value { get; set; }

    public string? value_before { get; set; }
}

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

public partial class proc_seq
{
    public int id { get; set; }

    [StringLength(100)]
    public string? proc_type { get; set; }

    [Column("proc_seq")]
    public long? proc_seq1 { get; set; }

    public DateTime? proc_date { get; set; }
}
