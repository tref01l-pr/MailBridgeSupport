﻿namespace MailBridgeSupport.API;

public class Gmail
{
    public string From { get; set; }
    public string To { get; set; }
    public string Body { get; set; }
    public string Subject { get; set; }
    public string MailDateTime { get; set; }
    public List<string> Attachments { get; set; }
    public string MsgID { get; set; }
}