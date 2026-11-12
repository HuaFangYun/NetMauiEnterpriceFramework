using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace __PROJECT__.Domain.Entities;

public partial class temp_user_app_pending
{
    public long id { get; set; }

    [StringLength(100)]
    public string user_id { get; set; } = string.Empty;

    [StringLength(100)]
    public string? full_name { get; set; }

    [StringLength(100)]
    public string? email { get; set; }

    [StringLength(20)]
    public string? phone_number { get; set; }

    [StringLength(20)]
    public string? kode_cabang { get; set; }

    [StringLength(1)]
    public string? gender_id { get; set; }

    [StringLength(100)]
    public string? group_id { get; set; }

    [StringLength(255)]
    public string? password { get; set; }

    [StringLength(1)]
    public string? approval_type_id { get; set; }

    [StringLength(100)]
    public string? created_by { get; set; }

    public DateTime created_date { get; set; }

    [StringLength(20)]
    public string? nomor_induk_karyawan { get; set; }

    public bool? is_active { get; set; }

    public bool? is_deleted { get; set; }
}
