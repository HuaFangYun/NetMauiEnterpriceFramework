using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace __PROJECT__.Domain.Entities;

public partial class tb_screen_menu_child : BaseAuditEntity
{
    [StringLength(100)]
    public string screen_menu_child_id { get; set; } = string.Empty;

    [StringLength(500)]
    public string? description { get; set; }

    public string? type { get; set; }

    public string? url_action { get; set; }

    public string? url_action_param { get; set; }

    public string? modal_field { get; set; }

    public string? modal_detail_field_query { get; set; }

    public string? modal_detail_field_query_param { get; set; }

    public string? modal_save_process_query { get; set; }

    public string? modal_save_process_query_param { get; set; }
}
