using __PROJECT__.Application.DTOs.Menu;
using __PROJECT__.Application.Interfaces;
using __PROJECT__.Application.Interfaces.Queries;

namespace __PROJECT__.Infrastructure.Services;

public class MenuService : IMenuService
{
    private readonly IMenuQueryService _queryService;

    public MenuService(IMenuQueryService queryService) => _queryService = queryService;

    public Task<List<MenuItemDto>> GetMenuTreeByGroupIdAsync(string groupId)
        => _queryService.GetMenuTreeByGroupIdAsync(groupId);
}
