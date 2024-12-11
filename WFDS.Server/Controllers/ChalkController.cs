using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WFDS.Common.Types;

namespace WFDS.Server.Controllers;

[ApiController]
[Route("api/v1/chalk")]
[Tags("chalk")]
public sealed class ChalkController(ICanvasManager canvas) : ControllerBase
{
    [HttpDelete]
    [SwaggerOperation("clear all chalk")]
    public IActionResult Delete()
    {
        canvas.ClearAll();
        return Ok();
    }
}