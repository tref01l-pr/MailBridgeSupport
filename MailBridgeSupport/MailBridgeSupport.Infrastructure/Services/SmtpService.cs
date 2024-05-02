using System.Net;
using CSharpFunctionalExtensions;
using MailBridgeSupport.API;
using MailBridgeSupport.Domain.Interfaces;
using MailBridgeSupport.Domain.Interfaces.Infrastructure;
using MailBridgeSupport.Domain.Models;
using MailBridgeSupport.Domain.Options;
using MailKit.Net.Smtp;
using MimeKit;
using MimeKit.Text;

namespace MailBridgeSupport.Infrastructure.Services;

public class SmtpService : ISmtpService
{
    public async Task<Result> SendMessageAsync(SmtpOptions smtpOptions, SentMessage sentMessage)
    {
        try
        {
            var email = new MimeMessage();

            email.From.Add(new MailboxAddress("", smtpOptions.User));
            email.To.Add(new MailboxAddress("", sentMessage.To));

            email.Subject = sentMessage.Subject;
            email.Body = new TextPart(MimeKit.Text.TextFormat.Html) { 
                Text = sentMessage.Body
            };

            using (var smtp = new SmtpClient())
            {
                await smtp.ConnectAsync("smtp.gmail.com", smtpOptions.Port, false);
                await smtp.AuthenticateAsync(smtpOptions.User, smtpOptions.Password);
                await smtp.SendAsync(email);
                await smtp.DisconnectAsync(true);
            }

            return Result.Success();
        }
        catch (Exception e)
        {
            return Result.Failure(e.Message);
        }
    }
}