using MailKit;

namespace MailBridgeSupport.Domain.Models;

public class ImapMessage
{
    public string UniqueId { get; set; }
    public string? From { get; set; }
    public string? To { get; set; }
    public string Subject { get; set; }
    public string Body { get; set; }
    public DateTimeOffset Date { get; set; }
}