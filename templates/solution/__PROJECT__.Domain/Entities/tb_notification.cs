using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace __PROJECT__.Domain.Entities;

public partial class tb_notification
{
    public long id { get; set; }

    public Guid? notification_type_id { get; set; }

    [StringLength(100)]
    public string? receiver_user_id { get; set; }

    [StringLength(100)]
    public string? reff_id { get; set; }

    public bool? is_seen { get; set; }

    public DateTime? created_date { get; set; }

    [StringLength(100)]
    public string? created_by { get; set; }
}
