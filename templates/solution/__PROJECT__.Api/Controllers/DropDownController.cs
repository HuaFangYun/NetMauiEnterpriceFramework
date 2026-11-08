using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using __PROJECT__.Application.Interfaces;

namespace __PROJECT__.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DropDownController : ControllerBase
{
    private readonly IDropDownService _dropDownService;

    public DropDownController(IDropDownService dropDownService) => _dropDownService = dropDownService;

    [HttpGet("groups")]
    public async Task<IActionResult> GetGroups() => Ok(await _dropDownService.GetGroupsAsync());

    [HttpGet("genders")]
    public async Task<IActionResult> GetGenders() => Ok(await _dropDownService.GetGendersAsync());

    [HttpGet("users")]
    public async Task<IActionResult> GetUsers() => Ok(await _dropDownService.GetUsersAsync());

    [HttpGet("menu-icons")]
    public async Task<IActionResult> GetMenuIcons() => Ok(await _dropDownService.GetMenuIconsAsync());

    [HttpGet("menu-parent-levels")]
    public async Task<IActionResult> GetMenuParentLevels() => Ok(await _dropDownService.GetMenuParentLevelsAsync());
}
