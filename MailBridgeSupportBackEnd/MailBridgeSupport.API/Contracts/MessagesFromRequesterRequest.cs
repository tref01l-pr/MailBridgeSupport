using System.ComponentModel.DataAnnotations;
using MailBridgeSupport.Domain.Models;

namespace MailBridgeSupport.API.Contracts;

public class MessagesFromRequesterRequest
{
    [Required]
    [EmailAddress]
    [StringLength(SentMessage.MaxEmailLength)]
    public string UserEmail { get; set; }
    
    [Required]
    [Range(0, int.MaxValue, ErrorMessage = "The {0} field must be greater than or equal to {1}.")]
    public int PageSize { get; set; }
    
    [Required]
    [Range(0, int.MaxValue, ErrorMessage = "The {0} field must be greater than or equal to {1}.")]
    public int PageNumber { get; set; }
}