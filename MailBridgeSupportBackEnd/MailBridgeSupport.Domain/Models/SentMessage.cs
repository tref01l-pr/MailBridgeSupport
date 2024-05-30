using System.Net.Mail;
using CSharpFunctionalExtensions;

namespace MailBridgeSupport.Domain.Models;

public class SentMessage
{
    public const int MaxEmailLength = 320;
    public const int MaxSubjectLength = 500;
    public const int MaxBodyLength = 1500;

    private SentMessage(
        int id,
        Guid userId,
        string to,
        string subject,
        string body,
        DateTimeOffset date)
    {
        Id = id;
        UserId = userId;
        To = to;
        Subject = subject;
        Body = body;
        Date = date;
    }

    public int Id { get; set; }
    public Guid UserId { get; set; }
    public string To { get; set; }
    public string Subject { get; set; }
    public string Body { get; set; }
    public DateTimeOffset Date { get; set; }


    public static Result<SentMessage> Create(
        Guid userId,
        string to,
        string subject,
        string body,
        DateTimeOffset date)
    {
        Result failure = Result.Success();

        if (string.IsNullOrWhiteSpace(to))
        {
            failure = Result.Failure<SentMessage>($"SentMessage {nameof(to)} can't be null or white space");
        }
        else if (!IsValidEmail(to))
        {
            failure = Result.Combine(
                failure,
                Result.Failure<SentMessage>($"Email is incorrect"));
        }
        else if (to.Length > MaxEmailLength)
        {
            failure = Result.Combine(
                failure,
                Result.Failure<SentMessage>($"SentMessage {nameof(to)} can`t be more than {MaxEmailLength} chars"));
        }

        if (string.IsNullOrWhiteSpace(subject))
        {
            failure = Result.Combine(
                failure,
                Result.Failure<SentMessage>($"SentMessage {nameof(subject)} can't be null or white space"));
        }
        else if (subject.Length > MaxSubjectLength)
        {
            failure = Result.Combine(
                failure,
                Result.Failure<SentMessage>(
                    $"SentMessage {nameof(subject)} can`t be more than {MaxSubjectLength} chars"));
        }

        if (string.IsNullOrWhiteSpace(body))
        {
            failure = Result.Combine(
                failure,
                Result.Failure<SentMessage>($"SentMessage {nameof(body)} can't be null or white space"));
        }
        else if (body.Length > MaxBodyLength)
        {
            failure = Result.Combine(
                failure,
                Result.Failure<SentMessage>($"SentMessage {nameof(body)} can`t be more than {MaxBodyLength} chars"));
        }

        if (date > DateTimeOffset.Now)
        {
            failure = Result.Combine(
                failure,
                Result.Failure<SentMessage>($"SentMessage {nameof(date)} can`t be > than DateTimeOffset.Now"));
        }

        if (failure.IsFailure)
        {
            return Result.Failure<SentMessage>(failure.Error);
        }

        return new SentMessage(
            0,
            userId,
            to,
            subject,
            body,
            date);
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