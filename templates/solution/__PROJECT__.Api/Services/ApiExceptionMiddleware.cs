using System.Text.Json;

using __PROJECT__.Application.DTOs.General;
using __PROJECT__.Application.Interfaces;
using __PROJECT__.Domain.Enums;

namespace __PROJECT__.Api.Services;

public class ApiExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ApiExceptionMiddleware> _logger;

    public ApiExceptionMiddleware(RequestDelegate next, ILogger<ApiExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IErrorLogService errorLog)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await errorLog.LogAsync("Api", "ERROR", ex);
            _logger.LogError(ex, "Unhandled API error on {Method} {Path}", context.Request.Method, context.Request.Path);

            if (context.Response.HasStarted)
            {
                throw;
            }

            context.Response.Clear();
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json";

            var response = new ApiResponse<string>
            {
                Success = false,
                Message = "Terjadi kesalahan pada server. Silakan hubungi administrator.",
                Code = Const.ERROR_CODE,
                Errors = new List<ApiError>
                {
                    new() { Field = string.Empty, Message = "Internal server error." },
                },
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(
                response,
                new JsonSerializerOptions(JsonSerializerDefaults.Web)));
        }
    }
}
