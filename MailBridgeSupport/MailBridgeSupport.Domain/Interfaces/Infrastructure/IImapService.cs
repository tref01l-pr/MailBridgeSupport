using CSharpFunctionalExtensions;
using MailBridgeSupport.API;
using MailBridgeSupport.Domain.Models;
using MailBridgeSupport.Domain.Options;
using MailKit;
using MimeKit;

namespace MailBridgeSupport.Domain.Interfaces;

public interface IImapService
{
    public Task<Result<List<MimeMessage>>> GetMessagesAsync(ImapOptions imapOptions);
    Task<Result<List<ImapMessage>>> GetLastMessageFromUsers(ImapOptions imapOptions);

    Task<Result<List<string>>> GetMessagesBody(ImapOptions imapOptions, Dictionary<string, IMessageSummary> allMessages);
}