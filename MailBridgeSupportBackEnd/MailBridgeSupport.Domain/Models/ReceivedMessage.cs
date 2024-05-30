using System.Net.Mail;
using CSharpFunctionalExtensions;

namespace MailBridgeSupport.Domain.Models;

public class ReceivedMessage
{
    public const int MaxEmailLength = 320;
    public const int MaxSubjectLength = 5000;
    public const int MaxBodyLength = 15000;

    private ReceivedMessage(
        int id,
        int msgId,
        string from,
        string to,
        string subject,
        string body,
        DateTimeOffset date)
    {
        Id = id;
        MsgId = msgId;
        From = from;
        To = to;
        Subject = subject;
        Body = body;
        Date = date;
    }

    public int Id { get; set; }
    public int MsgId { get; set; }
    public string From { get; set; }
    public string To { get; set; }
    public string Subject { get; set; }
    public string Body { get; set; }
    public DateTimeOffset Date { get; set; }


    public static Result<ReceivedMessage> Create(
        string msgIdStr,
        string from,
        string to,
        string? subject,
        string? body,
        DateTimeOffset date)
    {
        Result failure = Result.Success();

        if (!int.TryParse(msgIdStr, out int msgId))
        {
            failure = Result.Failure<ReceivedMessage>($"ReceivedMessage {nameof(msgIdStr)} is not a number");
        }
        else if (msgId < 0)
        {
            failure = Result.Combine(
                failure,
                Result.Failure<ReceivedMessage>($"ReceivedMessage {nameof(msgId)} can't be < 0"));
        }

        if (string.IsNullOrWhiteSpace(from))
        {
            failure = Result.Combine(
                failure,
                Result.Failure<ReceivedMessage>($"ReceivedMessage {nameof(from)} can't be null or white space"));
        }
        else if (!IsValidEmail(from))
        {
            failure = Result.Combine(
                failure,
                Result.Failure<ReceivedMessage>($"Email is incorrect"));
        }
        else if (from.Length > MaxEmailLength)
        {
            failure = Result.Combine(
                failure,
                Result.Failure<ReceivedMessage>(
                    $"ReceivedMessage {nameof(from)} can`t be more than {MaxEmailLength} chars"));
        }

        if (string.IsNullOrWhiteSpace(to))
        {
            failure = Result.Combine(
                failure,
                Result.Failure<ReceivedMessage>($"ReceivedMessage {nameof(to)} can't be null or white space"));
        }
        else if (!IsValidEmail(to))
        {
            failure = Result.Combine(
                failure,
                Result.Failure<ReceivedMessage>($"Email is incorrect"));
        }
        else if (to.Length > MaxEmailLength)
        {
            failure = Result.Combine(
                failure,
                Result.Failure<ReceivedMessage>(
                    $"ReceivedMessage {nameof(to)} can`t be more than {MaxEmailLength} chars"));
        }

        if (subject != null)
        {
            if (subject.Length > MaxSubjectLength)
            {
                failure = Result.Combine(
                    failure,
                    Result.Failure<ReceivedMessage>(
                        $"ReceivedMessage {nameof(subject)} can`t be more than {MaxSubjectLength} chars"));
            }
        }
        else
        {
            subject = "null";
        }


        if (body != null)
        {
            if (body.Length > MaxBodyLength)
            {
                failure = Result.Combine(
                    failure,
                    Result.Failure<ReceivedMessage>(
                        $"ReceivedMessage {nameof(body)} can`t be more than {MaxBodyLength} chars"));
            }
        }
        else
        {
            body = "null";
        }

        if (date > DateTimeOffset.Now)
        {
            failure = Result.Combine(
                failure,
                Result.Failure<ReceivedMessage>(
                    $"ReceivedMessage {nameof(date)} can`t be more than current date {DateTimeOffset.Now} chars"));
        }

        if (failure.IsFailure)
        {
            return Result.Failure<ReceivedMessage>(failure.Error);
        }

        return new ReceivedMessage(
            0,
            msgId,
            from,
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