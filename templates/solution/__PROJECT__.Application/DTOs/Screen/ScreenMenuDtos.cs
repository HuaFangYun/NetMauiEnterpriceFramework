namespace __PROJECT__.Application.DTOs.Screen;

public class ScreenMenuListItemDto
{
    public string Id { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public int? StatusApproval { get; set; }
    public string? ScreenMenuChildId { get; set; }
    public string? ScreenMenuImportExcelProcessId { get; set; }
    public DateTime? ModifiedDate { get; set; }
}

public class ScreenMenuEditDto
{
    public string Id { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool HasButtonLoad { get; set; }
    public bool HasButtonProcess { get; set; }
    public bool HasButtonGenerateTxt { get; set; }
    public bool HasButtonExportExcel { get; set; }
    public bool HasButtonAdd { get; set; }
    public bool HasButtonDelete { get; set; }
    public bool HasActionRow { get; set; }
    public bool HasButtonImportExcel { get; set; }
    public string? FieldFilter { get; set; }
    public string? QueryLoad { get; set; }
    public string? QueryLoadParam { get; set; }
    public string? QueryProcess { get; set; }
    public string? QueryProcessParam { get; set; }
    public string? QueryExportTxt { get; set; }
    public string? QueryExportTxtParam { get; set; }
    public string? QueryDelete { get; set; }
    public string? QueryDeleteParam { get; set; }
    public bool IsActive { get; set; }
    public string? ScreenMenuChildId { get; set; }
    public string? ScreenMenuImportExcelProcessId { get; set; }
    public ScreenMenuChildEditDto? Child { get; set; }
    public ScreenMenuImportEditDto? Import { get; set; }
}

public class ScreenMenuChildEditDto
{
    public string ScreenMenuChildId { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Type { get; set; }
    public string? UrlAction { get; set; }
    public string? UrlActionParam { get; set; }
    public string? ModalField { get; set; }
    public string? ModalDetailFieldQuery { get; set; }
    public string? ModalDetailFieldQueryParam { get; set; }
    public string? ModalSaveProcessQuery { get; set; }
    public string? ModalSaveProcessQueryParam { get; set; }
    public bool IsActive { get; set; }
}

public class ScreenMenuImportEditDto
{
    public string ScreenMenuImportExcelProcessId { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? TableName { get; set; }
    public string? FieldInput { get; set; }
    public bool WithApproval { get; set; }
    public bool IsActive { get; set; }
}
