using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace __PROJECT__.Domain.Entities;

public partial class tb_email_outbox
{
    public Guid id { get; set; }

    [StringLength(20)]
    public string? provider_type { get; set; }

    [StringLength(255)]
    public string recipient { get; set; } = string.Empty;

    [StringLength(255)]
    public string subject { get; set; } = string.Empty;

    public string? body_html { get; set; }

    public string? body_text { get; set; }

    [StringLength(50)]
    public string? reference_type { get; set; }

    [StringLength(100)]
    public string? reference_id { get; set; }

    [StringLength(30)]
    public string status { get; set; } = "Pending";

    public int attempt_count { get; set; }

    public DateTime? next_attempt_at { get; set; }

    public DateTime? sent_at { get; set; }

    [StringLength(2000)]
    public string? last_error { get; set; }

    public DateTime? created_date { get; set; }
}
