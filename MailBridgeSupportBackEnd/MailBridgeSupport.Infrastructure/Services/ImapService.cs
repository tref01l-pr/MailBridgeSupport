using CSharpFunctionalExtensions;
using MailBridgeSupport.API;
using MailBridgeSupport.Domain.Interfaces;
using MailBridgeSupport.Domain.Models;
using MailBridgeSupport.Domain.Options;
using MailKit;
using MailKit.Net.Imap;
using MimeKit;

namespace MailBridgeSupport.Infrastructure.Services;

public class ImapService : IImapService
{
    public async Task<Result<List<MimeMessage>>> GetMessagesAsync(ImapOptions imapOptions)
    {
        try
        {
            var client = new ImapClient();

            client.ServerCertificateValidationCallback = (s, c, h, e) => true;

            await client.ConnectAsync("imap.gmail.com", imapOptions.Port, true);

            await client.AuthenticateAsync(imapOptions.User, imapOptions.Password);

            var inbox = client.Inbox;
            await inbox.OpenAsync(FolderAccess.ReadOnly);

            List<MimeMessage> mimeMessages = new List<MimeMessage>();
            
            for (int i = 0; i < inbox.Count; i++)
            {
                var message = await inbox.GetMessageAsync(i);
                mimeMessages.Add(message);
            }

            await client.DisconnectAsync(true);

            return mimeMessages;
        }
        catch (Exception e)
        {
            return Result.Failure<List<MimeMessage>>(e.Message);
        }
    }

    public async Task<Result<List<ImapMessage>>> GetLastMessage(ImapOptions imapOptions)
    {
        try
        {
            using (var client = new ImapClient ()) {

                await client.ConnectAsync("imap.gmail.com", imapOptions.Port, true);
                await client.AuthenticateAsync(imapOptions.User, imapOptions.Password);

                var inbox = client.Inbox;
                await inbox.OpenAsync(FolderAccess.ReadOnly);

                // Get all messages sorted by date (newest first)
                var allMessages = 
                    (await inbox.FetchAsync(0, -1, MessageSummaryItems.UniqueId | MessageSummaryItems.Envelope | MessageSummaryItems.BodyStructure))
                    .OrderByDescending(summary => summary.Date);

                var latestMessagesByUser = new List<ImapMessage>();

                foreach (var message in allMessages)
                {
                    var sender = message.Envelope.From.Mailboxes.Single().Address;
                    if (!string.IsNullOrEmpty(sender) && !latestMessagesByUser.Any(m => m.From == sender))
                    {
                        var text = (TextPart)(await inbox.GetBodyPartAsync(message.UniqueId, message.TextBody));
                        var imapMessage = ImapMessage.Create(
                            message.Envelope.From.Mailboxes.Single().Address,
                            message.Envelope.From.Mailboxes.Single().Address,
                            message.Envelope.Subject,
                            text.Text,
                            message.Date,
                            SentMessageStatus.Question);

                        if (imapMessage.IsFailure)
                        {
                            return Result.Failure<List<ImapMessage>>(imapMessage.Error);
                        }

                        latestMessagesByUser.Add(imapMessage.Value);
                    }
                }
                
                

                await client.DisconnectAsync(true);
                return latestMessagesByUser;
            }
        }
        catch (Exception e)
        {
            return Result.Failure<List<ImapMessage>>(e.Message);
        }
        
    }

    public async Task<Result<List<ImapMessage>>> GetMessagesFromRequester(ImapOptions imapOptions, string email)
    {
        try
        {
            using (var client = new ImapClient ()) {

                await client.ConnectAsync("imap.gmail.com", imapOptions.Port, true);
                await client.AuthenticateAsync(imapOptions.User, imapOptions.Password);

                var inbox = client.Inbox;
                await inbox.OpenAsync(FolderAccess.ReadOnly);

                var allMessages = 
                    (await inbox.FetchAsync(0, -1, MessageSummaryItems.UniqueId | MessageSummaryItems.Envelope | MessageSummaryItems.BodyStructure))
                    .Where(m => m.Envelope.From.Mailboxes.Single().Address == email)
                    .OrderByDescending(summary => summary.Date);

                if (!allMessages.Any())
                {
                    return Result.Failure<List<ImapMessage>>("There is no lists from that email");
                }

                var userMessages = new List<ImapMessage>();

                foreach (var message in allMessages)
                {
                    var text = (TextPart)(await inbox.GetBodyPartAsync(message.UniqueId, message.TextBody));
                    var imapMessage = ImapMessage.Create(
                        message.Envelope.From.Mailboxes.Single().Address,
                        message.Envelope.From.Mailboxes.Single().Address,
                        message.Envelope.Subject,
                        text.Text,
                        message.Date,
                        SentMessageStatus.Question);

                    if (imapMessage.IsFailure)
                    {
                        return Result.Failure<List<ImapMessage>>(imapMessage.Error);
                    }

                    userMessages.Add(imapMessage.Value);
                }

                await client.DisconnectAsync(true);
                return userMessages;
            }
        }
        catch (Exception e)
        {
            return Result.Failure<List<ImapMessage>>(e.Message);
        }
    }

    public async Task<Result<List<string>>> GetMessagesBody(ImapOptions imapOptions, Dictionary<string, IMessageSummary> allMessages)
    {
        try
        {
            using (var client = new ImapClient ()) {

                await client.ConnectAsync("imap.gmail.com", imapOptions.Port, true);
                await client.AuthenticateAsync(imapOptions.User, imapOptions.Password);

                var inbox = client.Inbox;
                await inbox.OpenAsync(FolderAccess.ReadOnly);

                List<string> resultBodies = new List<string>();
                
                foreach (var message in allMessages)
                {
                    var text = (TextPart)(await inbox.GetBodyPartAsync(message.Value.UniqueId, message.Value.TextBody));
                    resultBodies.Add(text.Text);
                }
                
                await client.DisconnectAsync(true);
                return resultBodies;
            }
        }
        catch (Exception e)
        {
            return Result.Failure<List<string>>(e.Message);
        }
    }

    //TODO: переделать
    public async Task<Result<List<ReceivedMessage>>> GetNewMessages(ImapOptions imapOptions, int numberOfMessages)
    {
        try
        {
            using (var client = new ImapClient ()) {

                await client.ConnectAsync("imap.gmail.com", imapOptions.Port, true);
                await client.AuthenticateAsync(imapOptions.User, imapOptions.Password);

                var inbox = client.Inbox;
                await inbox.OpenAsync(FolderAccess.ReadOnly);

                // Get all messages sorted by date (newest first)

                if (inbox.Count == numberOfMessages)
                {
                    return Result.Success<List<ReceivedMessage>>(new List<ReceivedMessage>());
                }
                else if (inbox.Count < numberOfMessages)
                {
                    return Result.Failure<List<ReceivedMessage>>("Fatal Error inbox messages < numberOfMessages");
                }
                
                var allMessages = 
                    (await inbox.FetchAsync(0, -1, MessageSummaryItems.UniqueId | MessageSummaryItems.Envelope | MessageSummaryItems.BodyStructure))
                    .OrderByDescending(summary => summary.Date).ToList();

                var newMessages = new List<ReceivedMessage>();

                for (int i = 0; i < allMessages.Count - numberOfMessages; i++)
                {
                    var sender = allMessages[i].Envelope.From.Mailboxes.Single().Address;
                    if (string.IsNullOrEmpty(sender))
                    {
                        return Result.Failure<List<ReceivedMessage>>("Sender name is empty!!!");
                    }
                    
                    var text = (TextPart)(await inbox.GetBodyPartAsync(allMessages[i].UniqueId, allMessages[i].TextBody));
                    var receivedMessage = ReceivedMessage.Create(
                        allMessages[i].UniqueId.ToString(),
                        allMessages[i].Envelope.From.Mailboxes.Single().Address,
                        allMessages[i].Envelope.To.Mailboxes.Single().Address,
                        allMessages[i].Envelope.Subject,
                        text.Text,
                        allMessages[i].Date);

                    if (receivedMessage.IsFailure)
                    {
                        return Result.Failure<List<ReceivedMessage>>(receivedMessage.Error);
                    }
                    
                    newMessages.Add(receivedMessage.Value);
                }

                await client.DisconnectAsync(true);
                return newMessages;
            }
        }
        catch (Exception e)
        {
            return Result.Failure<List<ReceivedMessage>>(e.Message);
        }
    }

    public async Task<Result<List<SentMessage>>> GetAllSentMessages(ImapOptions imapOptions)
    {
        try
        {
            using (var client = new ImapClient ()) {

                await client.ConnectAsync("imap.gmail.com", imapOptions.Port, true);
                await client.AuthenticateAsync(imapOptions.User, imapOptions.Password);

                var sentFolder = client.GetFolder(SpecialFolder.Sent);
                await sentFolder.OpenAsync(FolderAccess.ReadOnly);
                
                var allMessages = 
                    (await sentFolder.FetchAsync(0, -1, MessageSummaryItems.UniqueId | MessageSummaryItems.Envelope | MessageSummaryItems.BodyStructure))
                    .OrderByDescending(summary => summary.Date).ToList();

                var newMessages = new List<SentMessage>();

                foreach (var t in allMessages)
                {
                    var sender = t.Envelope.From.Mailboxes.Single().Address;
                    if (string.IsNullOrEmpty(sender))
                    {
                        return Result.Failure<List<SentMessage>>("Sender name is empty!!!");
                    }

                    string text = "null";
                    
                    if (t.TextBody is not null)
                    {
                        text = ((TextPart)await sentFolder.GetBodyPartAsync(t.UniqueId, t.TextBody)).ToString();
                    }
                    
                    var sentMessage = SentMessage.Create(
                        Guid.Empty, 
                        t.Envelope.To.Mailboxes.Single().Address,
                        t.Envelope.Subject,
                        text,
                        t.Date);

                    if (sentMessage.IsFailure)
                    {
                        return Result.Failure<List<SentMessage>>(sentMessage.Error);
                    }
                    
                    newMessages.Add(sentMessage.Value);
                }

                await client.DisconnectAsync(true);
                return newMessages;
            }
        }
        catch (Exception e)
        {
            return Result.Failure<List<SentMessage>>(e.Message);
        }
    }
}