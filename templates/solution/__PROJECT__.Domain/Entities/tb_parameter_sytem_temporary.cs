using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace __PROJECT__.Domain.Entities;

public partial class tb_parameter_sytem_temporary
{
    public Guid id { get; set; }

    [StringLength(100)]
    public string? table_name { get; set; }

    [StringLength(50)]
    public string? type { get; set; }

    public DateTime? created_date { get; set; }

    [StringLength(100)]
    public string? created_by { get; set; }
}
