namespace MailBridgeSupport.API.Contracts;

using System.ComponentModel.DataAnnotations;
using MailBridgeSupport.Domain.Models;

public class MailAdminRegistrationRequest
{
    [MaxLength(MailAdmin.MaxLengthNickname)]
    public string Nickname { get; set; }

    [EmailAddress]
    public string Email { get; set; }

    public string Password { get; set; }
}