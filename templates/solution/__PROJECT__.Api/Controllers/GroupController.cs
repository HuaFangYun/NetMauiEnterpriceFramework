using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using __PROJECT__.Application.DTOs.General;
using __PROJECT__.Application.DTOs.Group;
using __PROJECT__.Application.Interfaces;

namespace __PROJECT__.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[Authorize(Roles = "00")]
public class GroupController : ControllerBase
{
    private readonly IGroupManagementService _groupService;

    public GroupController(IGroupManagementService groupService) => _groupService = groupService;

    [HttpPost("list")]
    public async Task<IActionResult> GetList([FromBody] DataTableRequestDto request)
        => Ok(await _groupService.GetListAsync(request));

    [HttpGet("{groupId}")]
    public async Task<IActionResult> Get(string groupId)
    {
        var group = await _groupService.GetByIdAsync(groupId);
        return group == null ? NotFound() : Ok(group);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] GroupEditDto dto)
    {
        var result = await _groupService.CreateAsync(dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("{groupId}")]
    public async Task<IActionResult> Update(string groupId, [FromBody] GroupEditDto dto)
    {
        var result = await _groupService.UpdateAsync(groupId, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{groupId}")]
    public async Task<IActionResult> Delete(string groupId)
    {
        var result = await _groupService.DeleteAsync(groupId);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
