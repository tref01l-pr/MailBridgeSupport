using CSharpFunctionalExtensions;
using MailBridgeSupport.Domain.Models;

namespace MailBridgeSupport.Domain.Interfaces.Application;

public interface IClientMessagesService
{
    Task<Result<User>> GetUserInfoAsync(Guid userId);
}