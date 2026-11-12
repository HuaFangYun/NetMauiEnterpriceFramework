using __PROJECT__.Application.DTOs.Email;
using __PROJECT__.Application.DTOs.General;

namespace __PROJECT__.Application.Interfaces;

public interface IEmailProviderService
{
    Task<(List<EmailProviderDto> Data, int TotalCount)> GetListAsync(DataTableRequestDto request);
    Task<EmailProviderDto?> GetAsync(int id);
    Task<BaseGeneralResponse> SaveAsync(EmailProviderDto request);
    Task<BaseGeneralResponse> DeleteAsync(int id);
    Task<BaseGeneralResponse> TestAsync(int id, string? recipient);
}
