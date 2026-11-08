using __PROJECT__.Application.DTOs.General;
using __PROJECT__.Application.DTOs.Screen;

namespace __PROJECT__.Application.Interfaces;

public interface IScreenMenuEditorService
{
    Task<GeneralListResponseDto<ScreenMenuListItemDto>> GetListAsync(DataTableRequestDto request);
    Task<ScreenMenuEditDto?> GetAsync(string id);
    Task<BaseGeneralResponse> StageAsync(ScreenMenuEditDto dto);
}
