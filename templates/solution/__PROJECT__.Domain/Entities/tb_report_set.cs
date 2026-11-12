using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace __PROJECT__.Domain.Entities;

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
