using System.Security.Claims;
using System.Text.Json;

using __PROJECT__.Application.DTOs.General;
using __PROJECT__.Application.Interfaces;
using __PROJECT__.Domain.Enums;

namespace __PROJECT__.Api.Services;

public class ActiveSessionMiddleware
{
    private readonly RequestDelegate _next;

    public ActiveSessionMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context, IUnitOfWork unitOfWork)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var sessionId = context.User.FindFirst("session_id")?.Value;

            if (!string.IsNullOrWhiteSpace(userId) && !string.IsNullOrWhiteSpace(sessionId))
            {
                var flag = await unitOfWork.UmUserAppFlag.GetByUserIdAsync(userId);
                if (flag == null
                    || flag.is_login != true
                    || !string.Equals(flag.session_id, sessionId, StringComparison.Ordinal))
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    context.Response.ContentType = "application/json";
                    var response = new ApiResponse<string>
                    {
                        Success = false,
                        Message = "Sesi Anda berakhir karena akun ini login dari browser lain.",
                        Code = Const.ERROR_CODE,
                        Errors = new List<ApiError>
                        {
                            new() { Field = string.Empty, Message = "Session revoked." },
                        },
                    };

                    await context.Response.WriteAsync(JsonSerializer.Serialize(
                        response,
                        new JsonSerializerOptions(JsonSerializerDefaults.Web)));
                    return;
                }
            }
        }

        await _next(context);
    }
}
