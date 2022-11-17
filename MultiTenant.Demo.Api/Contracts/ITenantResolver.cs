using FluentResults;
using MultiTenant.Demo.TenantCore.Context;

namespace MultiTenant.Demo.Api.Contracts;

public interface ITenantResolver
{
    Result<TenantToken> GetCurrentTenant();
}
