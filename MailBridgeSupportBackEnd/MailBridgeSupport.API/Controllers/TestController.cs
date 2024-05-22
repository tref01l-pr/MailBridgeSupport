using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MailBridgeSupport.API.Controllers;

[ApiController]
public class TestController : BaseController
{
    /*[HttpGet("GgetHeaders")]
    public async Task<IActionResult> GetHeaders()
    {
        var headers = HttpContext.Request.Headers;
        var result = headers.Authorization;
        var asdf = HttpContext.User.Claims;
        var asd = HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);
        return Ok("Success");
    }
    
    [Authorize]
    [HttpGet("GggetHeaders")]
    public async Task<IActionResult> GetHeaderss()
    {
        var headers = HttpContext.Request.Headers;
        var result = headers.Authorization;
        var asdf = HttpContext.User.Claims;
        var asd = HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);
        return Ok("Success");
    }*/
}