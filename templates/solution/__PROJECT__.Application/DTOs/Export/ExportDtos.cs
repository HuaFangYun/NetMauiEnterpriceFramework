namespace __PROJECT__.Application.DTOs.Export;

public class ExportQueueResultDto
{
    public string JobId { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
}

public class ExportStatusDto
{
    public bool Ready { get; set; }
    public string? FileName { get; set; }
    public string Message { get; set; } = string.Empty;
}
