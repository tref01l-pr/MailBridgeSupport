﻿using AutoMapper;
using MailBridgeSupport.API.Contracts;
using MailBridgeSupport.Domain.Interfaces;
using MailBridgeSupport.Domain.Interfaces.Application;
using MailBridgeSupport.Domain.Interfaces.Infrastructure;
using MailBridgeSupport.Domain.Models;
using MailBridgeSupport.Domain.Options;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace MailBridgeSupport.API.Controllers;

[Authorize]
public class SentMessagesController : BaseController
{
    private readonly ILogger<SentMessagesController> _logger;
    private readonly IMapper _mapper;
    private readonly SmtpOptions _smtpOptions;
    private readonly ImapOptions _imapOptions;
    private readonly IImapService _imapService;
    private readonly ISmtpService _smtpService;
    private readonly ISentMessagesService _sentMessagesService;

    public SentMessagesController(
        ILogger<SentMessagesController> logger,
        IMapper mapper,
        IOptions<SmtpOptions> smtpOptions,
        IOptions<ImapOptions> imapOptions,
        IImapService imapService,
        ISmtpService smtpService,
        ISentMessagesService sentMessagesService)
    {
        _logger = logger;
        _mapper = mapper;
        _smtpOptions = smtpOptions.Value;
        _imapOptions = imapOptions.Value;
        _imapService = imapService;
        _smtpService = smtpService;
        _sentMessagesService = sentMessagesService;
    }

    [HttpPost("send-message")]
    public async Task<IActionResult> SendMessage([FromBody] SentMessageRequest request)
    {
        var result = SentMessage.Create(
            UserId.Value,
            request.To,
            request.Subject,
            request.Body);

        if (result.IsFailure)
        {
            _logger.LogError("{errors}", result.Error);
            return BadRequest(result.Error);
        }

        var sentMessage = await _sentMessagesService.SendMessageAsync(_smtpOptions, result.Value);
        if (sentMessage.IsFailure)
        {
            _logger.LogError("{errors}", sentMessage.Error);
            return BadRequest(sentMessage.Error);
        }

        return Ok("Message was sent))");
    }

    [HttpGet("get-last-messages")]
    public async Task<IActionResult> GetLastMessages()
    {
        var resultMessages = await _sentMessagesService.GetLastSentMessagesAsync(_imapOptions);
        if (resultMessages.IsFailure)
        {
            return BadRequest(resultMessages.Error);
        }

        return Ok(resultMessages.Value);
    }

    [HttpGet("get-messages-from-requester")]
    public async Task<IActionResult> GetMessagesFromRequester([FromQuery] MessagesFromRequesterRequest request)
    {
        var queryString = HttpContext.Request;
        var resultMessages = await _sentMessagesService.GetByRequesterEmailAsync(_imapOptions, request.Requester);
        if (resultMessages.IsFailure)
        {
            _logger.LogError("{error}", resultMessages.Error);
            return BadRequest(resultMessages.Error);
        }

        var response = _mapper.Map<ImapMessage[], GetMessageResponse[]>(resultMessages.Value);

        return Ok(response);
    }
}