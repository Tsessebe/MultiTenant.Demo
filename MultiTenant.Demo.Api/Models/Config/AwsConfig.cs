namespace MultiTenant.Demo.Api.Models.Config;

public class AwsConfig
{
    public string Region { get; set; } = string.Empty;
    
    public string Profile { get; set; } = string.Empty;
    
    public CognitoConfig Cognito { get; set; } = new();
}