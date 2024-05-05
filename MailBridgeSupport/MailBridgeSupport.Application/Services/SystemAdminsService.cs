using CSharpFunctionalExtensions;
using MailBridgeSupport.Domain.Interfaces.Application;
using MailBridgeSupport.Domain.Interfaces.DataAccess;
using MailBridgeSupport.Domain.Models;

namespace MailBridgeSupport.Application.Services;

public class SystemAdminsService : ISystemAdminsService
{
    private readonly IUsersRepository _usersRepository;

    public SystemAdminsService(IUsersRepository usersRepository)
    {
        _usersRepository = usersRepository;
    }

    public async Task<User[]> GetAsync()
    {
        return await _usersRepository.GetAsync();
    }

    public async Task<Result<User>> GetByIdAsync(Guid id)
    {
        var user = await _usersRepository.GetByIdAsync(id);

        if (user is null)
        {
            return Result.Failure<User>("No user with this id");
        }

        return user;
    }

    public async Task<Result> Delete(Guid id)
    {
        var result = await _usersRepository.Delete(id);

        if (result == false)
        {
            return Result.Failure("Не удалось удалить пользователя с таким идентификатором");
        }

        return Result.Success();
    }
}