using System.ComponentModel.DataAnnotations;

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

public partial class tb_screen_menu_import_excel_process : BaseAuditEntity
{
    [StringLength(100)]
    public string screen_menu_import_excel_process_id { get; set; } = string.Empty;

    [StringLength(500)]
    public string? description { get; set; }

    public string? table_name { get; set; }

    public string? field_input { get; set; }

    public bool? with_approval { get; set; }
}

public partial class tb_report_set : BaseEntity
{
    [StringLength(100)]
    public string id { get; set; } = string.Empty;

    public string? name { get; set; }

    public string? query_list { get; set; }

    public string? filter_param { get; set; }

    public string? detail_url { get; set; }

    public string? detail_param { get; set; }

    public string? query_list_param { get; set; }
}
