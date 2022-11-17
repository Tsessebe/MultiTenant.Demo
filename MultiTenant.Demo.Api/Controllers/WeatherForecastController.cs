using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MultiTenant.Demo.Api.Contracts;
using MultiTenant.Demo.TenantCore.Context;

namespace MultiTenant.Demo.Api.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private readonly ILogger<WeatherForecastController> logger;
    private readonly TenantContext context;
    
    public WeatherForecastController(ILogger<WeatherForecastController> logger,
        TenantContext context)
    {
        this.logger = logger;
        this.context = context;
    }

    [HttpGet(Name = "GetWeatherForecast")]
    public IEnumerable<WeatherForecast> Get()
    {
        var list = context.Weathers.AsNoTracking()
            .Include(_ => _.Summary);

        return list.Select(_ => new WeatherForecast
        {
            Tenant = _.Tenant,
            Date = _.Date.LocalDateTime,
            Summary = _.Summary.Name,
            TemperatureC = _.TemperatureC
        });
    }
}