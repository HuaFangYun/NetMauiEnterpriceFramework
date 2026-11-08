namespace __PROJECT__.Application.DTOs.General;

public class DataTableRequestDto
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SearchTerm { get; set; }
    public string? SortColumn { get; set; }
    public string? SortDirection { get; set; } = "asc";
}

public class GeneralListResponseDto<T>
{
    public List<T> Data { get; set; } = new();
    public int TotalCount { get; set; }
}

public class DropdownItemDto
{
    public string Value { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
}
