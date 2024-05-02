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

    public async Task<Result<List<ImapMessage>>> GetLastMessageFromUsers(ImapOptions imapOptions)
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
                        latestMessagesByUser.Add(new ImapMessage()
                        {
                            UniqueId = message.UniqueId.ToString(),
                            To = message.Envelope.To.Mailboxes.Single().Address,
                            From = message.Envelope.From.Mailboxes.Single().Address,
                            Date = message.Date,
                            Body = text.Text
                        });
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
    
    
    
    
}