using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace __PROJECT__.Domain.Entities;

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
