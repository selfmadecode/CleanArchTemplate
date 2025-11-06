using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.Controllers;

public class BaseController : ControllerBase
{
    protected IActionResult ReturnResponse(dynamic model)
    {
        if (model.StatusCode == StatusCodes.Status200OK)
        {
            return Ok(model);
        }

        return BadRequest(model);
    }

    protected Guid UserId
    {
        get { return Guid.Parse(User.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value); }
    }
}
