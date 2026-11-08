namespace __PROJECT__.Application.DTOs.General;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }
    public string? Code { get; set; }
    public List<ApiError>? Errors { get; set; }
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
