using CSharpFunctionalExtensions;
using MailBridgeSupport.Domain.Models;

namespace MailBridgeSupport.Domain.Interfaces.DataAccess;

public interface ISentMessagesRepository
{
    Task<Result<int>> CreateAsync(SentMessage sentMessage);

    Task<Result<SentMessage[]>> GetByEemployeeIdAsync(Guid userId);

    Task<Result<SentMessage[]>> GetByRequesterEmailAsync(string email);

    Task<Result<SentMessage[]>> GetLastSentMessagesAsync();

    Task<Result<bool>> SaveAsync();
}