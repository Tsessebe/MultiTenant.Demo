using FluentResults;
using Microsoft.EntityFrameworkCore;
using MultiTenant.Demo.Api.Contracts;
using MultiTenant.Demo.Api.MasterContext;
using MultiTenant.Demo.Api.Models.Config;
using MultiTenant.Demo.TenantCore.Context;

namespace MultiTenant.Demo.Api.Services;

public class HttpContextTenantResolverService : ITenantResolver
{
    private readonly IHttpContextAccessor httpContextAccessor;
    private readonly MultiTenantMasterContext masterContext;
    private readonly ConnectionsConfig connectionsConfig;

    public HttpContextTenantResolverService(IHttpContextAccessor httpContextAccessor,
        MultiTenantMasterContext masterContext, ConnectionsConfig connectionsConfig)
    {
        this.httpContextAccessor = httpContextAccessor;
        this.masterContext = masterContext;
        this.connectionsConfig = connectionsConfig;
    }

    public Result<TenantToken> GetCurrentTenant()
    {
        if (this.httpContextAccessor.HttpContext?.Items["currentTenantCode"] is not string tenantCode)
        {
            return Result.Fail("Cannot Find Tenant");
        }

        var tenant = masterContext.Tenants.AsNoTracking()
            .FirstOrDefault(_ => _.Code == tenantCode);

        if (tenant == null)
        {
            return Result.Fail("Tenant not registered.");
        }
        
        // Instead of the Tenant Code we could use the tenant.ConnectionString
        // values a key in the secret. 
        if (!this.connectionsConfig.TenantDb.TryGetValue(tenantCode, out var connStr))
        {
            return Result.Fail("Could not Find Connection String.");
        }
        
        var result = new TenantToken(tenant.Id, tenantCode, connStr);

        return Result.Ok(result);

    }
}