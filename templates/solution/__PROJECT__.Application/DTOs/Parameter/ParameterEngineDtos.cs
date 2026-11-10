using System.Text;
using System.Text.Json.Serialization;

using __PROJECT__.Application.DTOs.General;

namespace __PROJECT__.Application.DTOs.Parameter;

public static class ParameterReference
{
    public static string Encode(string tableName)
    {
        var encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(tableName));
        return encoded.TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }

    public static string Decode(string? reference)
    {
        if (string.IsNullOrWhiteSpace(reference))
        {
            return string.Empty;
        }

        var encoded = reference.Replace('-', '+').Replace('_', '/');
        encoded = encoded.PadRight(encoded.Length + ((4 - encoded.Length % 4) % 4), '=');

        try
        {
            var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(encoded));
            return string.IsNullOrWhiteSpace(decoded) ? reference : decoded;
        }
        catch (FormatException)
        {
            return reference;
        }
    }
}

public class ParameterCardDto
{
    public string Id { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsParamMaker { get; set; }
    public bool IsParamApproval { get; set; }
    public bool IsParamUpload { get; set; }
    public bool HasWaitingApproval { get; set; }
    public string? Icon { get; set; }

    [JsonIgnore]
    public string TableName => ParameterReference.Decode(Id);

    [JsonIgnore]
    public bool IsMaker => IsParamMaker;

    [JsonIgnore]
    public bool IsApproval => IsParamApproval;

    [JsonIgnore]
    public bool IsUpload => IsParamUpload;

    [JsonIgnore]
    public int PendingCount => HasWaitingApproval ? 1 : 0;
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

public class ParameterListResponseDto : PaginatedResponse<Dictionary<string, string>>
{
    public List<ParameterFieldDto> Fields { get; set; } = new();
    public string? KeyField { get; set; }

    [JsonIgnore]
    public List<Dictionary<string, string>> Rows
    {
        get => Data ?? new List<Dictionary<string, string>>();
        set => Data = value;
    }
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
