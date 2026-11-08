using __PROJECT__.Application.DTOs.Menu;

namespace __PROJECT__.Shared.Services;

public class MenuService
{
    private readonly ApiService _api;

    public MenuService(ApiService api) => _api = api;

    public async Task<List<MenuItemDto>> GetUserMenuAsync()
    {
        try
        {
            return await _api.GetFromJsonAsync<List<MenuItemDto>>("api/menu") ?? new List<MenuItemDto>();
        }
        catch
        {
            return new List<MenuItemDto>();
        }
    }
}
