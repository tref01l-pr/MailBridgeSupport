﻿namespace MailBridgeSupport.Domain.Options;

public class JWTSecretOptions
{
    public const string JWTSecret = "JWTSecret";

    public string Secret { get; set; } = string.Empty;
}