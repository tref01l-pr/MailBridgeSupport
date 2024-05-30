using CSharpFunctionalExtensions;
using MailBridgeSupport.API;
using MailBridgeSupport.Domain.Interfaces;
using MailBridgeSupport.Domain.Interfaces.Application;
using MailBridgeSupport.Domain.Interfaces.DataAccess;
using MailBridgeSupport.Domain.Interfaces.Infrastructure;
using MailBridgeSupport.Domain.Models;

namespace MailBridgeSupport.Application.Services;

public class MessagesService : IMessagesService
{
    private readonly IImapService _imapService;
    private readonly ISmtpService _smtpService;
    private readonly ITransactionsRepository _transactionsRepository;
    private readonly ISentMessagesRepository _sentMessagesRepository;
    private readonly IUsersRepository _usersRepository;
    private readonly IReceivedMessagesRepository _receivedMessagesRepository;

    public MessagesService(
        IImapService imapService,
        ISmtpService smtpService,
        IUsersRepository usersRepository,
        ITransactionsRepository transactionsRepository,
        ISentMessagesRepository sentMessagesRepository,
        IReceivedMessagesRepository receivedMessagesRepository)
    {
        _imapService = imapService;
        _smtpService = smtpService;
        _usersRepository = usersRepository;
        _transactionsRepository = transactionsRepository;
        _sentMessagesRepository = sentMessagesRepository;
        _receivedMessagesRepository = receivedMessagesRepository;
    }

    public async Task<Result<int>> SendMessageAsync(SmtpOptions smtpOptions, SentMessage sentMessage)
    {
        await _transactionsRepository.BeginTransactionAsync();
        try
        {
            var createResult = await _sentMessagesRepository.CreateAsync(sentMessage);
            if (createResult.IsFailure)
            {
                throw new Exception(createResult.Error);
            }

            var sendResult = await _smtpService.SendMessageAsync(smtpOptions, sentMessage);
            if (sendResult.IsFailure)
            {
                throw new Exception(sendResult.Error);
            }

            var transactionResult = await _transactionsRepository.CommitAsync();

            if (transactionResult.IsFailure)
            {
                return Result.Failure<int>(transactionResult.Error);
            }

            return createResult.Value;
        }
        catch (Exception e)
        {
            var transactionRollbackResult = await _transactionsRepository.RollbackAsync();

            if (transactionRollbackResult.IsFailure)
            {
                return Result.Failure<int>(transactionRollbackResult.Error);
            }

            return Result.Failure<int>(e.Message);
        }
    }

    //TODO make here a request to redis
    public async Task<Result<ImapMessage[]>> GetLastMessagesAsync(ImapOptions imapOptions)
    {
        var newMessages = await CheckUpdates(imapOptions);
        var receivedMessageFromQuestionEmails = await _receivedMessagesRepository.GetLastReceivedMessagesAsync();
        if (receivedMessageFromQuestionEmails.IsFailure)
        {
            return Result.Failure<ImapMessage[]>(receivedMessageFromQuestionEmails.Error);
        }

        var sentMessageFromEmployee = await _sentMessagesRepository.GetLastSentMessagesAsync();

        if (sentMessageFromEmployee.IsFailure)
        {
            return Result.Failure<ImapMessage[]>(sentMessageFromEmployee.Error);
        }

        var filteredSentMessages = new List<ImapMessage>();
        var userIds = sentMessageFromEmployee.Value.Select(x => x.UserId).Distinct();
        var users = await _usersRepository.GetByIdsAsync(userIds);

        foreach (var receivedMessage in receivedMessageFromQuestionEmails.Value)
        {
            var message = filteredSentMessages.FirstOrDefault(m => m.Requester == receivedMessage.From);
            if (message != null && message.Date < receivedMessage.Date)
            {
                filteredSentMessages.Remove(message);
            }
            else if (message != null && message.Date >= receivedMessage.Date)
            {
                continue;
            }

            var result = ImapMessage.Create(
                receivedMessage.From,
                receivedMessage.From,
                receivedMessage.Subject,
                receivedMessage.Body,
                receivedMessage.Date,
                SentMessageStatus.Question);

            if (result.IsFailure)
            {
                return Result.Failure<ImapMessage[]>(result.Error);
            }

            filteredSentMessages.Add(result.Value);
        }


        foreach (var sentMessage in sentMessageFromEmployee.Value)
        {
            var message = filteredSentMessages.FirstOrDefault(m => m.Requester == sentMessage.To);

            if (message != null && message.Date < sentMessage.Date)
            {
                filteredSentMessages.Remove(message);
            }
            else if (message != null && message.Date >= sentMessage.Date)
            {
                continue;
            }

            var user = users.FirstOrDefault(u => u.Id == sentMessage.UserId);

            var result = ImapMessage.Create(
                sentMessage.To,
                user?.Email,
                sentMessage.Subject,
                sentMessage.Body,
                sentMessage.Date,
                SentMessageStatus.Answer);

            if (result.IsFailure)
            {
                return Result.Failure<ImapMessage[]>(result.Error);
            }

            filteredSentMessages.Add(result.Value);
        }

        return filteredSentMessages.OrderByDescending(u => u.Date).ToArray();
    }

    public async Task<Result<ImapMessage[]>> GetByRequesterEmailAsync(ImapOptions imapOptions, string email)
    {
        await CheckUpdates(imapOptions);
        var receivedMessagesFromRequester = await _receivedMessagesRepository.GetByRequesterAsync(email);
        if (receivedMessagesFromRequester.IsFailure)
        {
            return Result.Failure<ImapMessage[]>(receivedMessagesFromRequester.Error);
        }

        var sentMessagesFromEmployee = await _sentMessagesRepository.GetByRequesterEmailAsync(email);

        if (sentMessagesFromEmployee.IsFailure)
        {
            return Result.Failure<ImapMessage[]>(sentMessagesFromEmployee.Error);
        }

        var userIds = sentMessagesFromEmployee.Value.Select(x => x.UserId).Distinct();
        var users = await _usersRepository.GetByIdsAsync(userIds);
        var imapMessages = new List<ImapMessage>();

        foreach (var sentMessage in sentMessagesFromEmployee.Value)
        {
            var user = users.FirstOrDefault(u => u.Id == sentMessage.UserId);

            var imapMessage = ImapMessage.Create(
                sentMessage.To,
                user?.Email,
                sentMessage.Subject,
                sentMessage.Body,
                sentMessage.Date,
                SentMessageStatus.Answer);

            if (imapMessage.IsFailure)
            {
                return Result.Failure<ImapMessage[]>(imapMessage.Error);
            }

            imapMessages.Add(imapMessage.Value);
        }

        foreach (var receivedMessage in receivedMessagesFromRequester.Value)
        {
            var imapMessage = ImapMessage.Create(
                receivedMessage.From,
                receivedMessage.From,
                receivedMessage.Subject,
                receivedMessage.Body,
                receivedMessage.Date,
                SentMessageStatus.Question);

            if (imapMessage.IsFailure)
            {
                return Result.Failure<ImapMessage[]>(imapMessage.Error);
            }

            imapMessages.Add(imapMessage.Value);
        }

        return imapMessages.OrderByDescending(u => u.Date).ToArray();
    }

    private async Task<Result<List<ReceivedMessage>>> CheckUpdates(ImapOptions imapOptions)
    {
        var numberOfReceivedMessages = await _receivedMessagesRepository.GetNumberOfMessagesAsync();
        if (numberOfReceivedMessages.IsFailure)
        {
            return Result.Failure<List<ReceivedMessage>>(numberOfReceivedMessages.Error);
        }

        var newMessages = await _imapService.GetNewMessages(imapOptions, numberOfReceivedMessages.Value);
        if (newMessages.IsFailure)
        {
            return Result.Failure<List<ReceivedMessage>>(newMessages.Error);
        }
        else if (newMessages.Value.Count == 0)
        {
            return Result.Success<List<ReceivedMessage>>(new List<ReceivedMessage>());
        }

        foreach (var message in newMessages.Value)
        {
            var result = await _receivedMessagesRepository.CreateAsync(message);
            if (result.IsFailure)
            {
                return Result.Failure<List<ReceivedMessage>>(result.Error);
            }
        }

        //TODO: set redis variable

        return newMessages;
    }
}