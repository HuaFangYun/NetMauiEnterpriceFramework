namespace __PROJECT__.Shared.Services.Interfaces;

public interface IExcelFilePicker
{
    bool IsNativePickerSupported { get; }
    Task<ExcelFilePickResult?> PickAsync(CancellationToken cancellationToken = default);
}

public sealed record ExcelFilePickResult(string FileName, byte[] Content);
