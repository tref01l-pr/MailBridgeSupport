using Microsoft.AspNetCore.Mvc;

namespace MailBridgeSupport.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    [HttpGet("/GgetHeaders")]
    public async Task<IActionResult> GetHeaders()
    {
        var headers = HttpContext.Request.Headers;
        var result = headers.Authorization;
        return Ok("Success");
    }
}