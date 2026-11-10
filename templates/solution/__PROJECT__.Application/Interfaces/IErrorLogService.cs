namespace __PROJECT__.Application.Interfaces;

public interface IErrorLogService
{
    Task LogAsync(string entity, string action, Exception exception, string? userId = null);
}
