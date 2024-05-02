using System.Net.Mail;
using CSharpFunctionalExtensions;

namespace MailBridgeSupport.Domain.Models;

public class SentMessage
{
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
        string body)
    {
        Result failure = Result.Success();

        if (userId == Guid.Empty)
        {
            failure = Result.Failure<Session>($"{nameof(userId)} is not be empty!");
        }
        
        if (string.IsNullOrWhiteSpace(to))
        {
            failure = Result.Combine(
                failure,
                Result.Failure<SentMessage>($"SmtpMessage {nameof(to)} can't be null or white space"));
        }
        else if (!IsValidEmail(to))
        {
            failure = Result.Combine(
                failure,
                Result.Failure<SentMessage>($"Email is incorrect"));
        }

        if (string.IsNullOrWhiteSpace(subject))
        {
            failure = Result.Combine(
                failure,
                Result.Failure<SentMessage>($"SmtpMessage {nameof(subject)} can't be null or white space"));
        }
        else if (subject.Length > MaxSubjectLength)
        {
            failure = Result.Combine(
                failure,
                Result.Failure<SentMessage>($"Course {nameof(subject)} can`t be more than {MaxSubjectLength} chars"));
        }
        
        if (string.IsNullOrWhiteSpace(body))
        {
            failure = Result.Combine(
                failure,
                Result.Failure<SentMessage>($"SmtpMessage {nameof(body)} can't be null or white space"));
        }
        else if (body.Length > MaxBodyLength)
        {
            failure = Result.Combine(
                failure,
                Result.Failure<SentMessage>($"Course {nameof(body)} can`t be more than {MaxBodyLength} chars"));
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
            DateTimeOffset.Now);
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