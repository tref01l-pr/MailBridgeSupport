using CSharpFunctionalExtensions;
using MailBridgeSupport.Domain.Models;

namespace MailBridgeSupport.Domain.Interfaces.Application;

public interface ISystemAdminsService
{
    Task<Result> Delete(Guid id);

    Task<User[]> GetAsync();

    Task<Result<User>> GetByIdAsync(Guid id);
}