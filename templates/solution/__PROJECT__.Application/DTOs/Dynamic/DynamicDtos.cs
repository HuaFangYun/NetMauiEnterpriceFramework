namespace __PROJECT__.Application.DTOs.Dynamic;

public class DynamicFieldDto
{
    public string Id { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Type { get; set; } = "v";
    public string? Extra { get; set; }
}

public class DynamicScreenDto
{
    public string ScreenId { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<DynamicFieldDto> Fields { get; set; } = new();
    public bool HasButtonLoad { get; set; }
    public bool HasButtonProcess { get; set; }
    public bool HasButtonGenerateTxt { get; set; }
    public bool HasButtonExportExcel { get; set; }
    public bool HasButtonAdd { get; set; }
    public bool HasButtonDelete { get; set; }
    public bool HasActionRow { get; set; }
    public bool HasButtonImportExcel { get; set; }
    public string? ScreenMenuChildId { get; set; }
    public DynamicChildDto? Child { get; set; }
}

public class DynamicChildDto
{
    public string ScreenMenuChildId { get; set; } = string.Empty;
    public string? Type { get; set; }
    public string? UrlAction { get; set; }
    public string? UrlActionParam { get; set; }
    public List<DynamicFieldDto> ModalFields { get; set; } = new();
    public bool HasModalDetail { get; set; }
    public bool HasModalSave { get; set; }
}

public class DynamicMutationRequestDto
{
    public string? Key { get; set; }
    public Dictionary<string, string?> Values { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}

public class DynamicExportResultDto
{
    public string FileName { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}

public class DynamicReportDto
{
    public string ReportId { get; set; } = string.Empty;
    public string? Name { get; set; }
    public List<DynamicFieldDto> Fields { get; set; } = new();
}

public class DynamicLoadRequestDto
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SearchTerm { get; set; }
    public string? SortColumn { get; set; }
    public string? SortDirection { get; set; } = "asc";
    public Dictionary<string, string?> Filters { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}

public class DynamicLoadResultDto
{
    public List<DynamicFieldDto> Fields { get; set; } = new();
    public List<string> Columns { get; set; } = new();
    public List<Dictionary<string, object>> Rows { get; set; } = new();
    public int TotalCount { get; set; }
}

public class DynamicOptionDto
{
    public string Value { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
}
