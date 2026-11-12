using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using __PROJECT__.Api.Helpers;
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
    [Authorize(Roles = "00")]
    public async Task<IActionResult> GetGroups()
        => Ok(ApiResponseFactory.Success(await _dropDownService.GetGroupsAsync(), "Groups retrieved successfully"));

    [HttpGet("genders")]
    public async Task<IActionResult> GetGenders()
        => Ok(ApiResponseFactory.Success(await _dropDownService.GetGendersAsync(), "Genders retrieved successfully"));

    [HttpGet("users")]
    [Authorize(Roles = "00")]
    public async Task<IActionResult> GetUsers()
        => Ok(ApiResponseFactory.Success(await _dropDownService.GetUsersAsync(), "Users retrieved successfully"));

    [HttpGet("menu-icons")]
    [Authorize(Roles = "00")]
    public async Task<IActionResult> GetMenuIcons()
        => Ok(ApiResponseFactory.Success(await _dropDownService.GetMenuIconsAsync(), "Menu icons retrieved successfully"));

    [HttpGet("menu-parent-levels")]
    [Authorize(Roles = "00")]
    public async Task<IActionResult> GetMenuParentLevels()
        => Ok(ApiResponseFactory.Success(await _dropDownService.GetMenuParentLevelsAsync(), "Menu parent levels retrieved successfully"));
}
