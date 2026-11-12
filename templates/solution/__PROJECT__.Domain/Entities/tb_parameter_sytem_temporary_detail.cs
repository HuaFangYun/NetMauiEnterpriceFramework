using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace __PROJECT__.Domain.Entities;

public partial class tb_parameter_sytem_temporary_detail
{
    public Guid tb_parameter_sytem_temporary_id { get; set; }

    [StringLength(1000)]
    public string column_name { get; set; } = string.Empty;

    public string? value { get; set; }

    public string? value_before { get; set; }
}
