using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using MultiTenant.Demo.Api.Contracts;
using MultiTenant.Demo.Api.Models.Config;
using MultiTenant.Demo.Api.Services;
using MultiTenant.Demo.Api.MasterContext;
using Microsoft.EntityFrameworkCore;
using MultiTenant.Demo.TenantCore.Context;

namespace MultiTenant.Demo.Api;

public class Startup
{
    private readonly IConfiguration configuration;
    private readonly IHostEnvironment hostingEnvironment;
    private readonly WebApiConfig config;
    
    public Startup(
        IConfiguration configuration,
        IHostEnvironment hostingEnvironment)
    {
        this.configuration = configuration;
        this.hostingEnvironment = hostingEnvironment;

        // Bind Config to object
        this.config = new WebApiConfig();
        configuration.Bind(this.config);
    }

// This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton(this.config);
        services.AddSingleton(this.config.ConnectionStrings);
        
        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        services.AddTransient<ITenantResolver, HttpContextTenantResolverService>();

        services.AddDbContext<MultiTenantMasterContext>(options =>
        {
            options.UseSqlServer(this.config.ConnectionStrings.MasterDb);
        });

        services.AddScoped<TenantContext>(ctx =>
        {
            var tenantResolver = ctx.GetRequiredService<ITenantResolver>();
            var resolveTenantResult = tenantResolver.GetCurrentTenant();

            var tenantToken = resolveTenantResult.Value;

            return TenantContext.Factory(tenantToken);
        });
        
        this.ConfigureIdentity(services, this.config.Aws);
        services.AddControllers();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "Multi Tenant Demo Api", Version = "v1" });
        });

        services.AddCors(options =>
        {
            options.AddPolicy("CorsPolicy", builder =>
            {
                builder
                    .AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            });
        });
    }

// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Multi Tenant Demo Api v1"));
        }

        app.UseHttpsRedirection();

        app.UseRouting();

        app.UseCors("CorsPolicy");

        app.UseAuthentication();

        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers()
                .RequireCors("CorsPolicy");
        });
    }

    private void ConfigureIdentity(IServiceCollection services, AwsConfig awsConfig)
    {
        services.AddCognitoIdentity();

        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(cfg =>
            {
                var authority = $"https://cognito-idp.{awsConfig.Region}.amazonaws.com/{awsConfig.Cognito.UserPoolId}";
                
                cfg.SaveToken = true;

                cfg.Authority = authority;
                cfg.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = authority,
                    ValidateLifetime = true,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                };
                
                cfg.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = (ctx) =>
                    {
                        if (ctx.Exception.GetType() == typeof(SecurityTokenExpiredException))
                        {
                            ctx.Response.Headers.Add("IS-TOKEN-EXPIRED", "true");
                        }

                        return Task.CompletedTask;
                    },
                    OnTokenValidated = (ctx) =>
                    {
                        // We have a validated Token, let's extract it from the context
                        if (ctx.SecurityToken is not JwtSecurityToken jwtToken)
                        {
                            ctx.Fail(new Exception("Token not a JWT Security Token."));
                            return Task.CompletedTask;
                        }

                        var tenants = jwtToken.Claims.Where(_ => _.Type == "custom:tenantCode")
                            .Select(_ => _.Value)
                            .ToList();
                        
                        if (!tenants.Any())
                        {
                            ctx.Response.Headers.Add("X-NO-TENANT", "true");
                            ctx.Fail(new Exception("JWT Security Token contains no tenants."));
                            return Task.CompletedTask;
                        }

                        var tenantCode = tenants.First();
                        
                        var userId = jwtToken.Claims.Where(_ => _.Type == "sub")
                            .Select(_ => _.Value)
                            .FirstOrDefault();

                        // Add the token claims to the principle
                        (ctx.Principal?.Identity as ClaimsIdentity)?.AddClaims(jwtToken.Claims);
                        
                        // Inject the tenant, user Id & Token into the http context.
                        ctx.HttpContext.Items["currentTenantCode"] = tenantCode;
                        ctx.HttpContext.Items["userId"] = userId;
                        ctx.HttpContext.Items["accessToken"] = jwtToken;
                        
                        return Task.CompletedTask; 
                    },
                };
            });
    }
}