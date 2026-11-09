using System.Text.Json.Serialization;

namespace __PROJECT__.Application.DTOs.Parameter;

public class ParameterCardDto
{
    public string TableName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? MakerUrl { get; set; }
    public string? ApprovalUrl { get; set; }
    public bool IsMaker { get; set; }
    public bool IsApproval { get; set; }
    public bool IsUpload { get; set; }
    public string? Icon { get; set; }
    public int PendingCount { get; set; }
}

public class ParameterFieldDto
{
    public string FieldName { get; set; } = string.Empty;
    public string? FieldDesc { get; set; }
    public string? FieldType { get; set; }
    public string? FieldReff { get; set; }
    public bool IsMandatory { get; set; }
    public bool IsKey { get; set; }
    public bool IsOrderable { get; set; }
    public int OrderSeq { get; set; }
    public string? AddClass { get; set; }
}

public class ParameterListResponseDto
{
    public List<ParameterFieldDto> Fields { get; set; } = new();
    public List<Dictionary<string, string>> Rows { get; set; } = new();
    public int TotalCount { get; set; }
    public string? KeyField { get; set; }
}

public class ParameterSaveRequestDto
{
    public string TableName { get; set; } = string.Empty;
    public string? Key { get; set; }
    public Dictionary<string, string> Values { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}

public class ParameterPendingRequestDto
{
    public string? TableName { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SearchTerm { get; set; }
    public string? SortColumn { get; set; }
    public string? SortDirection { get; set; } = "asc";
}

public class ParameterPendingListItemDto
{
    public Guid Id { get; set; }
    public string TableName { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string? KeyValue { get; set; }
    public string? Summary { get; set; }
    public string? RequesterFullName { get; set; }
    public DateTime? CreatedDate { get; set; }
}

public class ParameterApprovalRowDto
{
    public Guid IdPending { get; set; }
    public string TypeParameter { get; set; } = string.Empty;
    public Dictionary<string, string?> Fields { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public int Action { get; set; } = 2;
}

public class ParameterKeyRequestDto
{
    public string TableName { get; set; } = string.Empty;
    public string? Key { get; set; }
}
