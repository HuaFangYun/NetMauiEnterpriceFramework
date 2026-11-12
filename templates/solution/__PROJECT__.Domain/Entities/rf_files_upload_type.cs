using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace __PROJECT__.Domain.Entities;

public partial class rf_files_upload_type : BaseEntity
{
    [StringLength(10)]
    public string files_upload_type_code { get; set; } = string.Empty;

    [StringLength(50)]
    public string? files_upload_type_T24_code { get; set; }

    [StringLength(100)]
    public string? files_upload_type_name { get; set; }
}
