using CSharpFunctionalExtensions;
using MailBridgeSupport.API;
using MailBridgeSupport.Domain.Models;

namespace MailBridgeSupport.Domain.Interfaces.DataAccess;

public interface IReceivedMessagesRepository
{
    Task<Result<int>> GetNumberOfMessagesAsync();
    Task<Result<ReceivedMessage[]>> GetByRequesterAsync(string email);
    Task<Result<ReceivedMessage[]>> GetLastReceivedMessagesAsync();
    Task<Result<int>> CreateAsync(ReceivedMessage receivedMessage);
    Task<Result<bool>> SaveAsync();
}