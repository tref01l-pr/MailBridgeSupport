namespace MailBridgeSupport.API.Contracts;

public class TokenResponse
{
    public string Role { get; set; }

    public string AccessToken { get; set; }
    
    public string Nickname { get; set; }
}