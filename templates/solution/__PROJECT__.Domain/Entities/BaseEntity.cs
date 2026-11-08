using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace __PROJECT__.Domain.Entities;

public abstract class BaseAuditEntity
{
    [Column("is_active")]
    public bool? isActive { get; set; }

    [Column("is_deleted")]
    public bool? isDeleted { get; set; }

    [Column("created_by")]
    [StringLength(100)]
    public string? createdBy { get; set; }

    [Column("created_date")]
    public DateTime? createdDate { get; set; }

    [Column("modified_by")]
    [StringLength(100)]
    public string? modifiedBy { get; set; }

    [Column("modified_date")]
    public DateTime? modifiedDate { get; set; }

    [Column("deleted_by")]
    [StringLength(100)]
    public string? deletedBy { get; set; }

    [Column("deleted_date")]
    public DateTime? deletedDate { get; set; }
}

public abstract class BaseEntity : BaseAuditEntity
{
    [Column("status_approval")]
    public int? statusApproval { get; set; }

    [Column("is_locked")]
    public bool? isLocked { get; set; }

    [Column("is_default")]
    public bool? isDefault { get; set; }

    [Column("approved_by")]
    [StringLength(100)]
    public string? approvedBy { get; set; }

    [Column("approved_date")]
    public DateTime? approvedDate { get; set; }

    [Column("approval_type_id")]
    [StringLength(2)]
    public string? approvalTypeId { get; set; }
}
