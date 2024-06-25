using CSharpFunctionalExtensions;
using MailBridgeSupport.Domain.Models;

namespace MailBridgeSupport.Domain.Interfaces.DataAccess;

public interface ICacheRepository
{
    Task<Result<List<ImapMessage>>> GetLastMessagesAsync();
    Task<Result<ImapMessage>> GetLastMessageByRequester(string requester);
    Task<Result<bool>> SetLastMessagesAsync(ImapMessage[] lastMessagesValue);
    Task<Result<bool>> UpdateLastMessagesAsync(ImapMessage[] lastMessages);
    Task<Result<bool>> RemoveKey();
}