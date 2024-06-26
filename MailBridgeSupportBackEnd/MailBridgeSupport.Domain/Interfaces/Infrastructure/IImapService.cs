﻿using CSharpFunctionalExtensions;
using MailBridgeSupport.API;
using MailBridgeSupport.Domain.Models;
using MailBridgeSupport.Domain.Options;
using MailKit;
using MimeKit;

namespace MailBridgeSupport.Domain.Interfaces;

public interface IImapService
{
    public Task<Result<List<MimeMessage>>> GetMessagesAsync(ImapOptions imapOptions);
    Task<Result<List<ImapMessage>>> GetLastMessage(ImapOptions imapOptions);

    Task<Result<List<ImapMessage>>> GetMessagesFromRequester(ImapOptions imapOptions, string email);

    Task<Result<List<string>>>
        GetMessagesBody(ImapOptions imapOptions, Dictionary<string, IMessageSummary> allMessages);

    Task<Result<List<ReceivedMessage>>> GetNewMessages(ImapOptions imapOptions, int numberOfMessages);
    Task<Result<List<SentMessage>>> GetAllSentMessages(ImapOptions imapOptions);
}