using CSharpFunctionalExtensions;
using MailBridgeSupport.API;
using MailBridgeSupport.Domain.Interfaces;
using MailBridgeSupport.Domain.Interfaces.Application;
using MailBridgeSupport.Domain.Interfaces.DataAccess;
using MailBridgeSupport.Domain.Interfaces.Infrastructure;
using MailBridgeSupport.Domain.Models;

namespace MailBridgeSupport.Application.Services;

public class SentMessagesService : ISentMessagesService
{
    private readonly IImapService _imapService;
    private readonly ISmtpService _smtpService;
    private readonly ITransactionsRepository _transactionsRepository;
    private readonly ISentMessagesRepository _sentMessagesRepository;
    private readonly IUsersRepository _usersRepository;

    public SentMessagesService(
        IImapService imapService,
        ISmtpService smtpService,
        IUsersRepository usersRepository,
        ITransactionsRepository transactionsRepository,
        ISentMessagesRepository sentMessagesRepository)
    {
        _imapService = imapService;
        _smtpService = smtpService;
        _usersRepository = usersRepository;
        _transactionsRepository = transactionsRepository;
        _sentMessagesRepository = sentMessagesRepository;
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

    public async Task<Result<ImapMessage[]>> GetLastSentMessagesAsync(ImapOptions imapOptions)
    {
        var sentMessageFromQuestionEmails = await _imapService.GetLastMessage(imapOptions);
        if (sentMessageFromQuestionEmails.IsFailure)
        {
            return Result.Failure<ImapMessage[]>(sentMessageFromQuestionEmails.Error);
        }

        var sentMessageFromEmployee = await _sentMessagesRepository.GetLastSentMessagesAsync();

        if (sentMessageFromEmployee.IsFailure)
        {
            return Result.Failure<ImapMessage[]>(sentMessageFromEmployee.Error);
        }

        var filteredSentMessages = new List<ImapMessage>();
        var userIds = sentMessageFromEmployee.Value.Select(x => x.UserId).Distinct();
        var users = await _usersRepository.GetByIdsAsync(userIds);

        foreach (var message in sentMessageFromQuestionEmails.Value)
        {
            var employeeMessage = sentMessageFromEmployee.Value.FirstOrDefault(x => x.To == message.From);
            if (employeeMessage != null)
            {
                var user = users.FirstOrDefault(u => u.Id == employeeMessage.UserId);

                var imapMessage = ImapMessage.Create(
                    employeeMessage.To,
                    user?.Email,
                    employeeMessage.Subject,
                    employeeMessage.Body,
                    employeeMessage.Date,
                    SentMessageStatus.Answer);

                if (imapMessage.IsFailure)
                {
                    filteredSentMessages.Add(message);
                    continue;
                }

                filteredSentMessages.Add(imapMessage.Value);
            }
            else
            {
                filteredSentMessages.Add(message);
            }
        }

        return filteredSentMessages.OrderByDescending(u => u.Date).ToArray();
    }

    public async Task<Result<ImapMessage[]>> GetByRequesterEmailAsync(ImapOptions imapOptions, string email)
    {
        var sentMessageFromQuestionEmails = await _imapService.GetMessagesFromRequester(imapOptions, email);
        if (sentMessageFromQuestionEmails.IsFailure)
        {
            return Result.Failure<ImapMessage[]>(sentMessageFromQuestionEmails.Error);
        }

        var sentMessageFromEmployee = await _sentMessagesRepository.GetByRequesterEmailAsync(email);

        if (sentMessageFromEmployee.IsFailure)
        {
            return Result.Failure<ImapMessage[]>(sentMessageFromEmployee.Error);
        }
        
        var userIds = sentMessageFromEmployee.Value.Select(x => x.UserId).Distinct();
        var users = await _usersRepository.GetByIdsAsync(userIds);
        
        foreach (var sentMessage in sentMessageFromEmployee.Value)
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
                continue;
            }

            sentMessageFromQuestionEmails.Value.Add(imapMessage.Value);
        }

        return sentMessageFromQuestionEmails.Value.OrderByDescending(u => u.Date).ToArray();
    }
}