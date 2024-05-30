using CSharpFunctionalExtensions;
using MailBridgeSupport.API;
using MailBridgeSupport.Domain.Models;

namespace MailBridgeSupport.Domain.Interfaces.Application;

public interface IMessagesService
{
    Task<Result<int>> SendMessageAsync(SmtpOptions smtpOptions, SentMessage sentMessage);

    Task<Result<ImapMessage[]>> GetLastMessagesAsync(ImapOptions imapOptions);

    Task<Result<ImapMessage[]>> GetByRequesterEmailAsync(ImapOptions imapOptions, string email);
}