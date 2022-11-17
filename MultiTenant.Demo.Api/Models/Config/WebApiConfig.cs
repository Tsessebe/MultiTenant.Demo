namespace MultiTenant.Demo.Api.Models.Config;

public class WebApiConfig
{
    public AwsConfig Aws { get; set; } = new();

    public ConnectionsConfig ConnectionStrings { get; set; } = new();
}