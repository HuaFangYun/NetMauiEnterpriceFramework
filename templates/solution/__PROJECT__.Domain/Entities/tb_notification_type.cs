using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace __PROJECT__.Domain.Entities;

public partial class tb_notification_type
{
    public Guid id { get; set; }

    [StringLength(1000)]
    public string? short_text { get; set; }

    public string? long_text { get; set; }

    public bool? is_send_mail { get; set; }

    public bool? is_send_web { get; set; }

    public string? href { get; set; }

    public DateTime? created_date { get; set; }

    [StringLength(100)]
    public string? created_by { get; set; }
}
