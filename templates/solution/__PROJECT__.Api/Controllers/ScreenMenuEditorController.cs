using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using __PROJECT__.Application.DTOs.General;
using __PROJECT__.Application.DTOs.Screen;
using __PROJECT__.Application.Interfaces;

namespace __PROJECT__.Api.Controllers;

[ApiController]
[Route("api/screen-menu-editor")]
[Authorize(Roles = "00")]
public class ScreenMenuEditorController : ControllerBase
{
    private readonly IScreenMenuEditorService _screenMenuService;

    public ScreenMenuEditorController(IScreenMenuEditorService screenMenuService)
        => _screenMenuService = screenMenuService;

    [HttpPost("list")]
    public async Task<IActionResult> GetList([FromBody] DataTableRequestDto request)
        => Ok(await _screenMenuService.GetListAsync(request));

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(string id)
    {
        var screen = await _screenMenuService.GetAsync(id);
        return screen == null ? NotFound() : Ok(screen);
    }

    [HttpPost]
    public async Task<IActionResult> Stage([FromBody] ScreenMenuEditDto dto)
    {
        var result = await _screenMenuService.StageAsync(dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
