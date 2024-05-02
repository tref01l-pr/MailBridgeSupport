using CSharpFunctionalExtensions;
using MailBridgeSupport.API;
using MailBridgeSupport.Domain.Models;
using MailBridgeSupport.Domain.Options;

namespace MailBridgeSupport.Domain.Interfaces.Infrastructure;

public interface ISmtpService
{
    public Task<Result> SendMessageAsync(SmtpOptions smtpOptions, SentMessage sentMessage);
}