using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using __PROJECT__.Application.DTOs.General;
using __PROJECT__.Application.DTOs.Role;
using __PROJECT__.Application.Interfaces;

namespace __PROJECT__.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[Authorize(Roles = "00")]
public class UserGroupController : ControllerBase
{
    private readonly IRoleManagementService _roleService;

    public UserGroupController(IRoleManagementService roleService) => _roleService = roleService;

    [HttpPost("list")]
    public async Task<IActionResult> GetList([FromBody] DataTableRequestDto request)
        => Ok(await _roleService.GetListAsync(request));

    [HttpGet("{userId}")]
    public async Task<IActionResult> Get(string userId)
    {
        var role = await _roleService.GetByIdAsync(userId);
        return role == null ? NotFound() : Ok(role);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] RoleEditDto dto)
    {
        var result = await _roleService.CreateAsync(dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("{userId}")]
    public async Task<IActionResult> Update(string userId, [FromBody] RoleEditDto dto)
    {
        var result = await _roleService.UpdateAsync(userId, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{userId}")]
    public async Task<IActionResult> Delete(string userId)
    {
        var result = await _roleService.DeleteAsync(userId);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
