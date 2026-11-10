using __PROJECT__.Application.DTOs.General;
using __PROJECT__.Domain.Enums;

namespace __PROJECT__.Api.Helpers;

public static class ApiResponseFactory
{
    public static ApiResponse<T> Success<T>(T data, string message = "", string code = Const.SUCCESS_CODE)
        => new()
        {
            Success = true,
            Message = message,
            Code = code,
            Data = data,
        };

    public static ApiResponse<string> Success(string message = "", string? data = null, string code = Const.SUCCESS_CODE)
        => new()
        {
            Success = true,
            Message = message,
            Code = code,
            Data = data,
        };

    public static PaginatedResponse<T> Page<T>(
        IEnumerable<T> data,
        DataTableRequestDto request,
        int totalCount,
        string message = "")
        => new()
        {
            Success = true,
            Message = message,
            Code = Const.SUCCESS_CODE,
            Data = data.ToList(),
            Pagination = new PaginationInfo
            {
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalCount = totalCount,
            },
        };

    public static ApiResponse<T> NotFound<T>(string message = "Data not found.")
        => Error<T>(message, Const.DATA_NOT_FOUND_CODE);

    public static ApiResponse<T> Error<T>(string message, string code = Const.ERROR_CODE)
        => new()
        {
            Success = false,
            Message = message,
            Code = code,
            Errors = new List<ApiError>
            {
                new() { Field = string.Empty, Message = message },
            },
        };

    public static ApiResponse<string> FromResult(BaseGeneralResponse result, string? data = null)
        => new()
        {
            Success = result.Success,
            Message = result.Message ?? string.Empty,
            Code = result.Code ?? (result.Success ? Const.SUCCESS_CODE : Const.ERROR_CODE),
            Data = data,
            Errors = result.Success
                ? null
                : new List<ApiError> { new() { Field = string.Empty, Message = result.Message } },
        };
}
