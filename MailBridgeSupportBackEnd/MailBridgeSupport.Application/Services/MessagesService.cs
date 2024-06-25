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
    private readonly ICacheRepository _cacheRepository;

    public MessagesService(
        IImapService imapService,
        ISmtpService smtpService,
        IUsersRepository usersRepository,
        ITransactionsRepository transactionsRepository,
        ISentMessagesRepository sentMessagesRepository,
        IReceivedMessagesRepository receivedMessagesRepository,
        ICacheRepository cacheRepository)
    {
        _imapService = imapService;
        _smtpService = smtpService;
        _usersRepository = usersRepository;
        _transactionsRepository = transactionsRepository;
        _sentMessagesRepository = sentMessagesRepository;
        _receivedMessagesRepository = receivedMessagesRepository;
        _cacheRepository = cacheRepository;
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
        var cacheLastMessages = await _cacheRepository.GetLastMessagesAsync();
        if (cacheLastMessages.IsFailure)
        {
            return Result.Failure<ImapMessage[]>(cacheLastMessages.Error);
        }

        if (cacheLastMessages.Value.Any())
        {
            return cacheLastMessages.Value.ToArray();
        }

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
                MessageStatus.Question);

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
                MessageStatus.Answer);

            if (result.IsFailure)
            {
                return Result.Failure<ImapMessage[]>(result.Error);
            }

            filteredSentMessages.Add(result.Value);
        }

        filteredSentMessages = filteredSentMessages.OrderByDescending(u => u.Date).ToList();

        var cacheSavingResult = await _cacheRepository.SetLastMessagesAsync(filteredSentMessages.ToArray());

        if (cacheSavingResult.IsFailure)
        {
            return Result.Failure<ImapMessage[]>(cacheSavingResult.Error);
        }

        return filteredSentMessages.ToArray();
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
                MessageStatus.Answer);

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
                MessageStatus.Question);

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

        var imapMessageList = new List<ImapMessage>();

        foreach (var message in newMessages.Value)
        {
            var createMessageresult = await _receivedMessagesRepository.CreateAsync(message);
            if (createMessageresult.IsFailure)
            {
                return Result.Failure<List<ReceivedMessage>>(createMessageresult.Error);
            }

            var imapMessage = ImapMessage.Create(
                message.From,
                message.From,
                message.Subject,
                message.Body,
                message.Date,
                MessageStatus.Question);

            if (imapMessage.IsFailure)
            {
                return Result.Failure<List<ReceivedMessage>>(imapMessage.Error);
            }

            var localLastMessage = imapMessageList.FirstOrDefault(m => m.Requester == imapMessage.Value.Requester);

            if (localLastMessage == null)
            {
                imapMessageList.Add(imapMessage.Value);
                continue;
            }

            if (localLastMessage.Date > imapMessage.Value.Date)
            {
                continue;
            }

            imapMessageList.RemoveAll(m => m.Requester == imapMessage.Value.Requester);
            imapMessageList.Add(imapMessage.Value);
        }

        var cacheResult = await _cacheRepository.UpdateLastMessagesAsync(imapMessageList.ToArray());

        return newMessages;
    }

    public async Task<Result<bool>> RemoveKeyAsync()
    {
        var result = await _cacheRepository.RemoveKey();
        if (result.IsFailure)
        {
            return Result.Failure<bool>(result.Error);
        }

        return result.Value;
    }
}