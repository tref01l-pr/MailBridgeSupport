using CSharpFunctionalExtensions;
using MailBridgeSupport.API;
using MailBridgeSupport.Domain.Models;

namespace MailBridgeSupport.Domain.Interfaces.Application;

public interface ISentMessagesService
{
    Task<Result<int>> SendMessageAsync(SmtpOptions smtpOptions, SentMessage sentMessage);

    Task<Result<ImapMessage[]>> GetLastSentMessagesAsync(ImapOptions imapOptions);

    Task<Result<ImapMessage[]>> GetByRequesterEmailAsync(ImapOptions imapOptions, string email);
}