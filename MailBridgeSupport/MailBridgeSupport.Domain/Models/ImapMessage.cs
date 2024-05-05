using System.Net.Mail;
using CSharpFunctionalExtensions;
using MailKit;

namespace MailBridgeSupport.Domain.Models;

public class ImapMessage
{
    public ImapMessage(
        string requester,
        string from,
        string subject,
        string body,
        DateTimeOffset date,
        SentMessageStatus sentMessageStatus)
    {
        Requester = requester;
        From = from;
        Subject = subject;
        Body = body;
        Date = date;
        SentMessageStatus = sentMessageStatus;
    }

    public string Requester { get; set; }
    public string From { get; set; }

    public string Subject { get; set; }

    public string Body { get; set; }

    public DateTimeOffset Date { get; set; }

    public SentMessageStatus SentMessageStatus { get; set; }

    public static Result<ImapMessage> Create(
        string requester,
        string? from,
        string subject,
        string body,
        DateTimeOffset date,
        SentMessageStatus sentMessageStatus)
    {
        Result failure = Result.Success();

        if (string.IsNullOrWhiteSpace(requester))
        {
            failure = Result.Combine(
                failure,
                Result.Failure<ImapMessage>($"ImapMessage {nameof(requester)} can't be null or white space"));
        }
        else if (!IsValidEmail(requester))
        {
            failure = Result.Combine(
                failure,
                Result.Failure<ImapMessage>($"ImapMessage {nameof(requester)} is not an email"));
        }

        if (string.IsNullOrWhiteSpace(from))
        {
            if (sentMessageStatus == SentMessageStatus.Answer)
            {
                from = "deleted@account.com";
            }
            else
            {
                failure = Result.Combine(
                    failure,
                    Result.Failure<ImapMessage>($"ImapMessage {nameof(from)} can't be null or white space"));
            }
        }
        else if (!IsValidEmail(from))
        {
            failure = Result.Combine(
                failure,
                Result.Failure<ImapMessage>($"ImapMessage {nameof(from)} is not an email"));
        }

        if (DateTimeOffset.Now < date)
        {
            failure = Result.Combine(
                failure,
                Result.Failure<ImapMessage>($"ImapMessage {nameof(date)} can't be null or white space"));
        }

        if (failure.IsFailure)
        {
            return Result.Failure<ImapMessage>(failure.Error);
        }

        return new ImapMessage(
            requester,
            from,
            subject,
            body,
            date,
            sentMessageStatus);
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var address = new MailAddress(email);
            return address.Address == email;
        }
        catch
        {
            return false;
        }
    }
}