namespace MailBridgeSupport.API.Controllers;

using MailBridgeSupport.Domain.Interfaces;
using MailBridgeSupport.Domain.Interfaces.Infrastructure;
using MailBridgeSupport.Domain.Models;
using MailBridgeSupport.Domain.Options;
using MailKit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MimeKit;

[Route("api/smtpAndImapController")]
public class SmtpAndImapController : BaseController
{
    private readonly SmtpOptions _smtpOptions;
    private readonly ImapOptions _imapOptions;
    private readonly IImapService _imapService;
    private readonly ISmtpService _smtpService;
    private readonly JWTSecretOptions _jwtSecretOptions;

    public SmtpAndImapController(
        IOptions<SmtpOptions> smtpOptions,
        IOptions<ImapOptions> imapOptions,
        IOptions<JWTSecretOptions> jwtSecretOptions,
        IImapService imapService,
        ISmtpService smtpService)
    {
        _smtpOptions = smtpOptions.Value;
        _imapOptions = imapOptions.Value;
        _jwtSecretOptions = jwtSecretOptions.Value;
        _imapService = imapService;
        _smtpService = smtpService;
    }

    [HttpGet("/getHeaders")]
    public async Task<IActionResult> GetHeaders()
    {
        var headers = HttpContext.Request.Headers;
        var result = headers.Authorization;
        return Ok("Success");
    }
    
    [Authorize]
    [HttpGet("/getMessages")]
    public async Task<IActionResult> GetMessages()
    {
        
        var result = await _imapService.GetMessagesAsync(_imapOptions);
        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }



        List<valueToResult> valueToResults = new List<valueToResult>();

        foreach (var mimeMessage in result.Value)
        {
            valueToResults.Add(new valueToResult(
                mimeMessage.From.Mailboxes,
                mimeMessage.To.Mailboxes,
                mimeMessage.Subject,
                mimeMessage.TextBody,
                mimeMessage.Date
            ));
        }
        return Ok(valueToResults);
    }

    [HttpPost("/sendMessage")]
    public async Task<IActionResult> SendMessage([FromBody] SentMessage sentMessage)
    {
        var result = await _smtpService.SendMessageAsync(_smtpOptions, sentMessage);
        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        return Ok("everything was sent");
    }
    
    [HttpGet("/getMessageByUsers")]
    public async Task<IActionResult> GetMessageByUsers()
    {
        var resultMessages = await _imapService.GetLastMessageFromUsers(_imapOptions);
        if (resultMessages.IsFailure)
        {
            return BadRequest(resultMessages.Error);
        }
        
        
        return Ok(resultMessages.Value);
    }
}

class valueToResult
{
    public List<string> From { get; set; } = new List<string>();
    public List<string> To { get; set; } = new List<string>();
    public string Subject { get; set; } = string.Empty;
    public string BodyParts { get; set; } = string.Empty;
    public DateTimeOffset Date { get; set; } = DateTimeOffset.Now;
    

    public valueToResult(IEnumerable<MailboxAddress> fromMailboxes, IEnumerable<MailboxAddress> toMailboxes, string mimeMessageSubject, string mimeMessageTextBody, DateTimeOffset mimeMessageDate)
    {
        foreach (var mailboxAddress in fromMailboxes)
        {
            From.Add(mailboxAddress.Address);
        }

        foreach (var mailboxAddress in toMailboxes)
        {
            To.Add(mailboxAddress.Address);
        }
        
        Subject = mimeMessageSubject;

        BodyParts = mimeMessageTextBody;
        
        Date = mimeMessageDate;
    }
}

class ValueToResult
{
    public string From { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public BodyPartText BodyParts { get; set; }
    public DateTimeOffset Date { get; set; } = DateTimeOffset.Now;

    public ValueToResult(string mimeMessageKey, string valueNormalizedSubject, BodyPartText mimeMessageSubject, DateTimeOffset mimeMessageTextBody)
    {
        From = mimeMessageKey;
        Subject = valueNormalizedSubject;
        BodyParts = mimeMessageSubject;
        Date = mimeMessageTextBody;
    }
}