using System.ComponentModel.DataAnnotations;
using MailBridgeSupport.Domain.Models;

namespace MailBridgeSupport.API.Contracts;

public class MessagesFromRequesterRequest
{
    [Required]
    [EmailAddress]
    [StringLength(SentMessage.MaxEmailLength)]
    public string Requester { get; set; }
}