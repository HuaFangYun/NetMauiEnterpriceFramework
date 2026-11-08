using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using __PROJECT__.Application.DTOs.General;
using __PROJECT__.Application.Interfaces;

namespace __PROJECT__.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "00")]
public class ApprovalController : ControllerBase
{
    private readonly IApprovalService _approvalService;

    public ApprovalController(IApprovalService approvalService) => _approvalService = approvalService;

    [HttpPost("users/list")]
    public async Task<IActionResult> GetPendingUsers([FromBody] DataTableRequestDto request)
        => Ok(await _approvalService.GetPendingUsersAsync(request));

    [HttpPost("users/{id:long}/approve")]
    public async Task<IActionResult> ApproveUser(long id)
    {
        var result = await _approvalService.ApproveUserAsync(id);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("users/{id:long}/reject")]
    public async Task<IActionResult> RejectUser(long id)
    {
        var result = await _approvalService.RejectUserAsync(id);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("groups/list")]
    public async Task<IActionResult> GetPendingGroups([FromBody] DataTableRequestDto request)
        => Ok(await _approvalService.GetPendingGroupsAsync(request));

    [HttpPost("groups/{id:long}/approve")]
    public async Task<IActionResult> ApproveGroup(long id)
    {
        var result = await _approvalService.ApproveGroupAsync(id);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("groups/{id:long}/reject")]
    public async Task<IActionResult> RejectGroup(long id)
    {
        var result = await _approvalService.RejectGroupAsync(id);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("roles/list")]
    public async Task<IActionResult> GetPendingRoles([FromBody] DataTableRequestDto request)
        => Ok(await _approvalService.GetPendingRolesAsync(request));

    [HttpPost("roles/{id:guid}/approve")]
    public async Task<IActionResult> ApproveRole(Guid id)
    {
        var result = await _approvalService.ApproveRoleAsync(id);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("roles/{id:guid}/reject")]
    public async Task<IActionResult> RejectRole(Guid id)
    {
        var result = await _approvalService.RejectRoleAsync(id);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("menus/list")]
    public async Task<IActionResult> GetPendingMenus([FromBody] DataTableRequestDto request)
        => Ok(await _approvalService.GetPendingMenusAsync(request));

    [HttpPost("menus/{id:guid}/approve")]
    public async Task<IActionResult> ApproveMenu(Guid id)
    {
        var result = await _approvalService.ApproveMenuAsync(id);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("menus/{id:guid}/reject")]
    public async Task<IActionResult> RejectMenu(Guid id)
    {
        var result = await _approvalService.RejectMenuAsync(id);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
