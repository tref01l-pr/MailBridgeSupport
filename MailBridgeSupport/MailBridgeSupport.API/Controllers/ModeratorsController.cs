using Microsoft.AspNetCore.Authorization;

namespace MailBridgeSupport.API.Controllers;

[Authorize(Roles = nameof(Roles.Moderator) + "," + nameof(Roles.SystemAdmin))]
public class ModeratorsController : BaseController
{
    
}