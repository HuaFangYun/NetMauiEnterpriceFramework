using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using __PROJECT__.Api.Helpers;
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
    {
        var result = await _approvalService.GetPendingUsersAsync(request);
        return Ok(ApiResponseFactory.Page(result.Data, request, result.TotalCount, "Users retrieved successfully"));
    }

    [HttpPost("users/{id:long}/approve")]
    public async Task<IActionResult> ApproveUser(long id)
        => ToActionResult(await _approvalService.ApproveUserAsync(id));

    [HttpPost("users/{id:long}/reject")]
    public async Task<IActionResult> RejectUser(long id)
        => ToActionResult(await _approvalService.RejectUserAsync(id));

    [HttpPost("groups/list")]
    public async Task<IActionResult> GetPendingGroups([FromBody] DataTableRequestDto request)
    {
        var result = await _approvalService.GetPendingGroupsAsync(request);
        return Ok(ApiResponseFactory.Page(result.Data, request, result.TotalCount, "Groups retrieved successfully"));
    }

    [HttpPost("groups/{id:long}/approve")]
    public async Task<IActionResult> ApproveGroup(long id)
        => ToActionResult(await _approvalService.ApproveGroupAsync(id));

    [HttpPost("groups/{id:long}/reject")]
    public async Task<IActionResult> RejectGroup(long id)
        => ToActionResult(await _approvalService.RejectGroupAsync(id));

    [HttpPost("roles/list")]
    public async Task<IActionResult> GetPendingRoles([FromBody] DataTableRequestDto request)
    {
        var result = await _approvalService.GetPendingRolesAsync(request);
        return Ok(ApiResponseFactory.Page(result.Data, request, result.TotalCount, "Roles retrieved successfully"));
    }

    [HttpPost("roles/{id:guid}/approve")]
    public async Task<IActionResult> ApproveRole(Guid id)
        => ToActionResult(await _approvalService.ApproveRoleAsync(id));

    [HttpPost("roles/{id:guid}/reject")]
    public async Task<IActionResult> RejectRole(Guid id)
        => ToActionResult(await _approvalService.RejectRoleAsync(id));

    [HttpPost("menus/list")]
    public async Task<IActionResult> GetPendingMenus([FromBody] DataTableRequestDto request)
    {
        var result = await _approvalService.GetPendingMenusAsync(request);
        return Ok(ApiResponseFactory.Page(result.Data, request, result.TotalCount, "Menus retrieved successfully"));
    }

    [HttpPost("menus/{id:guid}/approve")]
    public async Task<IActionResult> ApproveMenu(Guid id)
        => ToActionResult(await _approvalService.ApproveMenuAsync(id));

    [HttpPost("menus/{id:guid}/reject")]
    public async Task<IActionResult> RejectMenu(Guid id)
        => ToActionResult(await _approvalService.RejectMenuAsync(id));

    private IActionResult ToActionResult(BaseGeneralResponse result)
    {
        var response = ApiResponseFactory.FromResult(result);
        return result.Success ? Ok(response) : BadRequest(response);
    }
}
