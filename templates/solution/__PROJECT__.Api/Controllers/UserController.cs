using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using __PROJECT__.Application.DTOs.General;
using __PROJECT__.Application.DTOs.User;
using __PROJECT__.Application.Interfaces;

namespace __PROJECT__.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[Authorize(Roles = "00")]
public class UserController : ControllerBase
{
    private readonly IUserManagementService _userService;

    public UserController(IUserManagementService userService) => _userService = userService;

    [HttpPost("list")]
    public async Task<IActionResult> GetList([FromBody] DataTableRequestDto request)
        => Ok(await _userService.GetListAsync(request));

    [HttpGet("{userId}")]
    public async Task<IActionResult> Get(string userId)
    {
        var user = await _userService.GetByIdAsync(userId);
        return user == null ? NotFound() : Ok(user);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] UserEditDto dto)
    {
        var result = await _userService.CreateAsync(dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("{userId}")]
    public async Task<IActionResult> Update(string userId, [FromBody] UserEditDto dto)
    {
        var result = await _userService.UpdateAsync(userId, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{userId}")]
    public async Task<IActionResult> Delete(string userId)
    {
        var result = await _userService.DeleteAsync(userId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("unlock")]
    public async Task<IActionResult> Unlock([FromBody] UserActionDto dto)
    {
        var result = await _userService.UnlockAsync(dto.UserId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] UserActionDto dto)
    {
        var result = await _userService.ResetPasswordAsync(dto.UserId, dto.NewPassword ?? string.Empty);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
