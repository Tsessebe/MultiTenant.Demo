using Microsoft.EntityFrameworkCore;

namespace MultiTenant.Demo.TenantCore.Context;

#pragma warning disable 1591
public partial class TenantContext : DbContext
{
    private readonly TenantToken token;

    public TenantContext(TenantToken token, DbContextOptions<TenantContext> options)
        : base(options)
    {
        this.token = token;
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
    {

    }

    public static TenantContext Factory(TenantToken token)
    {
        var connStr = token.ConnectionString;

        var options = new DbContextOptionsBuilder<TenantContext>()
            .UseSqlServer(connStr)
            .Options;

        return new TenantContext(token, options);
    }
}

public record TenantToken(Guid TenantId, string TenantCode, string ConnectionString);

