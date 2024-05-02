using Microsoft.AspNetCore.Authorization;

namespace MailBridgeSupport.API.Controllers;

[Authorize(Roles = nameof(Roles.SystemAdmin))]
public class SystemAdminsController : BaseController
{
    
}