namespace __PROJECT__.Shared.Services;

using __PROJECT__.Shared.Services.Interfaces;

public class NoopExcelFilePicker : IExcelFilePicker
{
    public bool IsNativePickerSupported => false;

    public Task<ExcelFilePickResult?> PickAsync(CancellationToken cancellationToken = default)
        => Task.FromResult<ExcelFilePickResult?>(null);
}
