using CSharpFunctionalExtensions;
using MailBridgeSupport.Domain.Interfaces.Application;
using MailBridgeSupport.Domain.Interfaces.DataAccess;
using MailBridgeSupport.Domain.Models;

namespace MailBridgeSupport.Application.Services;

public class ClientsService : IClientMessagesService
{
    private readonly IUsersRepository _usersRepository;

    public ClientsService(IUsersRepository usersRepository)
    {
        _usersRepository = usersRepository;
    }
    public async Task<Result<User>> GetUserInfoAsync(Guid userId)
    {
        var user = await _usersRepository.GetByIdAsync(userId);

        if (user is null)
        {
            return Result.Failure<User>($"User with {nameof(userId)} not found");
        }

        return user;
    }
}