using System.Security.Claims;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using __PROJECT__.Application.DTOs.General;
using __PROJECT__.Application.DTOs.Menu;
using __PROJECT__.Application.Interfaces;

namespace __PROJECT__.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MenuController : ControllerBase
{
    private readonly IMenuService _menuService;
    private readonly IMenuManagementService _menuManagementService;
    private readonly IUnitOfWork _unitOfWork;

    public MenuController(
        IMenuService menuService,
        IMenuManagementService menuManagementService,
        IUnitOfWork unitOfWork)
    {
        _menuService = menuService;
        _menuManagementService = menuManagementService;
        _unitOfWork = unitOfWork;
    }

    [HttpGet]
    public async Task<IActionResult> GetUserMenu()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var userGroup = await _unitOfWork.UmUserGroup.GetByIdAsync(userId);
        if (userGroup == null || string.IsNullOrEmpty(userGroup.group_id))
        {
            return Ok(new List<MenuItemDto>());
        }

        var menu = await _menuService.GetMenuTreeByGroupIdAsync(userGroup.group_id);
        return Ok(menu);
    }

    [HttpPost("list")]
    [Authorize(Roles = "00")]
    public async Task<IActionResult> GetList([FromBody] DataTableRequestDto request)
        => Ok(await _menuManagementService.GetListAsync(request));

    [HttpGet("detail/{id:long}")]
    [Authorize(Roles = "00")]
    public async Task<IActionResult> GetDetail(long id)
    {
        var menu = await _menuManagementService.GetByIdAsync(id);
        return menu == null ? NotFound() : Ok(menu);
    }

    [HttpPost]
    [Authorize(Roles = "00")]
    public async Task<IActionResult> Create([FromBody] MenuEditDto dto)
    {
        var result = await _menuManagementService.CreateAsync(dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("{id:long}")]
    [Authorize(Roles = "00")]
    public async Task<IActionResult> Update(long id, [FromBody] MenuEditDto dto)
    {
        var result = await _menuManagementService.UpdateAsync(id, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{id:long}")]
    [Authorize(Roles = "00")]
    public async Task<IActionResult> Delete(long id)
    {
        var result = await _menuManagementService.DeleteAsync(id);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("tree")]
    [Authorize(Roles = "00")]
    public async Task<IActionResult> GetTree() => Ok(await _menuManagementService.GetTreeAsync());

    [HttpGet("assigned/{groupId}")]
    [Authorize(Roles = "00")]
    public async Task<IActionResult> GetAssigned(string groupId)
        => Ok(await _menuManagementService.GetAssignedMenuIdsAsync(groupId));

    [HttpPost("save-map")]
    [Authorize(Roles = "00")]
    public async Task<IActionResult> SaveMap([FromBody] MenuMapDto dto)
    {
        var result = await _menuManagementService.SaveMenuMapAsync(dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
