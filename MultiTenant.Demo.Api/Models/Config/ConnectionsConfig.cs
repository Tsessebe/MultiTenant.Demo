namespace MultiTenant.Demo.Api.Models.Config;

public class ConnectionsConfig
{
    public string MasterDb { get; set; } = string.Empty;

    public Dictionary<string, string> TenantDb { get; set; } = new();
}