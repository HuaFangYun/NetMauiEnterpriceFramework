using System.Text.Json.Serialization;

namespace __PROJECT__.Application.DTOs.General;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public T? Data { get; set; }
    public List<ApiError>? Errors { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public class PaginatedResponse<T> : ApiResponse<List<T>>
{
    [JsonPropertyOrder(-1)]
    public PaginationInfo Pagination { get; set; } = new();

    [JsonIgnore]
    public int TotalCount => Pagination.TotalCount;
}

public class PaginationInfo
{
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => PageSize > 0
        ? (int)Math.Ceiling((double)TotalCount / PageSize)
        : 0;
}

public class ApiError
{
    public string? Field { get; set; }
    public string? Message { get; set; }
}

public class BaseGeneralResponse
{
    public bool Success { get; set; }
    public string? Code { get; set; }
    public string? Message { get; set; }
}
