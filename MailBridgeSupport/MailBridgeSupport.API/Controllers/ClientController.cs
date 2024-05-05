using MailBridgeSupport.Domain.Interfaces.Application;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MailBridgeSupport.API.Controllers;

[Authorize]
public class ClientController : BaseController
{
    private readonly IClientMessagesService _clientMessagesService;

    public ClientController(IClientMessagesService clientMessagesService)
    {
        _clientMessagesService = clientMessagesService;
    }

    [HttpGet("get-user-info")]
    public async Task<IActionResult> GetUserInfo()
    {
        var user = await _clientMessagesService.GetUserInfoAsync(UserId.Value);

        if (user.IsFailure)
        {
            return BadRequest(user.Error);
        }

        return Ok(user.Value);
    }
}