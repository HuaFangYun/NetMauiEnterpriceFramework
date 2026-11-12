using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace __PROJECT__.Domain.Entities;

public partial class tb_screen_menu : BaseEntity
{
    [StringLength(100)]
    public string id { get; set; } = string.Empty;

    [StringLength(500)]
    public string? description { get; set; }

    public bool? has_button_load { get; set; }

    public bool? has_button_process { get; set; }

    public bool? has_button_generate_txt { get; set; }

    public bool? has_button_export_excel { get; set; }

    public bool? has_button_add { get; set; }

    public bool? has_button_delete { get; set; }

    public bool? has_action_row { get; set; }

    public bool? has_button_import_excel { get; set; }

    public string? field_filter { get; set; }

    public string? query_load { get; set; }

    public string? query_load_param { get; set; }

    public string? query_process { get; set; }

    public string? query_process_param { get; set; }

    public string? query_export_txt { get; set; }

    public string? query_export_txt_param { get; set; }

    [StringLength(100)]
    public string? screen_menu_import_excel_process_id { get; set; }

    [StringLength(100)]
    public string? screen_menu_child_id { get; set; }

    public string? query_delete { get; set; }

    public string? query_delete_param { get; set; }
}
