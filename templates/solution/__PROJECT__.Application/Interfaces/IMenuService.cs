using __PROJECT__.Application.DTOs.Menu;

namespace __PROJECT__.Application.Interfaces;

public interface IMenuService
{
    Task<List<MenuItemDto>> GetMenuTreeByGroupIdAsync(string groupId);
}
