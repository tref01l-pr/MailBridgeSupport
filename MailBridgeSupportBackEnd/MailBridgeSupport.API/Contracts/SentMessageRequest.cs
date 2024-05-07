using System.ComponentModel.DataAnnotations;
using MailBridgeSupport.Domain.Models;

namespace MailBridgeSupport.API.Contracts;

public class SentMessageRequest
{
    [Required]
    [EmailAddress]
    [StringLength(SentMessage.MaxEmailLength)]
    public string To { get; set; }

    [Required]
    [StringLength(SentMessage.MaxSubjectLength)]
    public string Subject { get; set; }

    [Required]
    [StringLength(SentMessage.MaxBodyLength)]
    public string Body { get; set; }
}