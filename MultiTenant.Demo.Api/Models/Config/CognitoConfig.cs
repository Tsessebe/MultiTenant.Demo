namespace MultiTenant.Demo.Api.Models.Config;

public class CognitoConfig
{
    public string UserPoolId { get; set; } = string.Empty;
    
    public string AppClientId { get; set; } = string.Empty;
    
    public string ClientSecret { get; set; } = string.Empty;
}