namespace __PROJECT__.Application.Interfaces;

public interface IParameterStagingService
{
    Task<Guid> StageAsync(string tableName, string type, IReadOnlyDictionary<string, string?> values);
    Task<bool> HasPendingAsync(string tableName, string keyColumn, string keyValue);
}
