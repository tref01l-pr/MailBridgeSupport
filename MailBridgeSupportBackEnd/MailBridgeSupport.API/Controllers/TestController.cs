using System.Security.Claims;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;
using MailBridgeSupport.API.Contracts;
using MailBridgeSupport.Domain.Options;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MimeKit;

namespace MailBridgeSupport.API.Controllers;

[ApiController]
public class TestController : BaseController
{
    public TestController(
        IOptions<GoogleApiOptions> googleApiOptions,
        IOptions<SmtpOptions> smtpOptions)
    {
        _googleApiOptions = googleApiOptions.Value;
        _smtpOptions = smtpOptions.Value;
    }
    /*[HttpGet("GgetHeaders")]
    public async Task<IActionResult> GetHeaders()
    {
        var headers = HttpContext.Request.Headers;
        var result = headers.Authorization;
        var asdf = HttpContext.User.Claims;
        var asd = HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);
        return Ok("Success");
    }
    
    [Authorize]
    [HttpGet("GggetHeaders")]
    public async Task<IActionResult> GetHeaderss()
    {
        var headers = HttpContext.Request.Headers;
        var result = headers.Authorization;
        var asdf = HttpContext.User.Claims;
        var asd = HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);
        return Ok("Success");
    }*/

    /*[HttpPost("send-message")]
    public async Task<IActionResult> SendMessage([FromBody] SentMessageRequest request)
    {
        const string accessToken = await 
    }*/
    
    private static readonly string[] Scopes = { GmailService.Scope.GmailSend };
    private static readonly string ApplicationName = "MailBridgeSupportGmailApi";
    private readonly GoogleApiOptions _googleApiOptions;
    private readonly SmtpOptions _smtpOptions;
    private static readonly string _hostUserName = "romanlistsender@gmail.com";

    [HttpPost("get-messages")]
    public async Task<IActionResult> GetMessages()
    {
        try
        {
            GmailService gmailService = await GmailServiceHelper.GetAccessTokenAsync();
            List<Gmail> emailList = new List<Gmail>();

            // Настройка запроса на получение списка сообщений
            UsersResource.MessagesResource.ListRequest listRequest =
                gmailService.Users.Messages.List(_hostUserName);
            listRequest.LabelIds = "INBOX";
            listRequest.IncludeSpamTrash = false;
            listRequest.Q = "is:unread";

            ListMessagesResponse listResponse = await listRequest.ExecuteAsync();

            if (listResponse != null && listResponse.Messages != null)
            {
                foreach (var messageItem in listResponse.Messages)
                {
                    UsersResource.MessagesResource.GetRequest message = gmailService.Users.Messages.Get(_hostUserName, messageItem.Id);
                    Message msgContent = await message.ExecuteAsync();
                    if (msgContent != null)
                    {
                        string FromAdress = string.Empty;
                        string Date = string.Empty;
                        string Subject = string.Empty;
                        string MailBody = string.Empty;
                        string ReadableText = string.Empty;

                        foreach (var messagePart in msgContent.Payload.Headers)
                        {
                            if (messagePart.Name == "From")
                            {
                                FromAdress = messagePart.Value;
                            }
                            else if (messagePart.Name == "Date")
                            {
                                Date = messagePart.Value;
                            }
                            else if (messagePart.Name == "Subject")
                            {
                                Subject = messagePart.Value;
                            }
                        }
                        
                        emailList.Add(new Gmail()
                        {
                            From = FromAdress,
                            MailDateTime = Date,
                            Subject = Subject,
                        });
                    }
                }

                if (emailList.Count > 0)
                {
                    return Ok(emailList);
                }
                else
                {
                    return BadRequest("Something went wrong!!!!");
                }
            }

            return Ok(emailList);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }
    

    private string Base64UrlEncode(byte[] input)
    {
        var s = System.Convert.ToBase64String(input); // Standard base64 encoder
        s = s.Split('=')[0]; // Remove any trailing '='s
        s = s.Replace('+', '-'); // 62nd char of encoding
        s = s.Replace('/', '_'); // 63rd char of encoding
        return s;
    }
}