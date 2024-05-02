using Microsoft.AspNetCore.Identity;

namespace MailBridgeSupport.DataAccess.SqlServer.Entities;

public class UserEntity : IdentityUser<Guid>
{
}