using BitrixJiraConnector.Api.BackgroundServices;
using BitrixJiraConnector.Api.Models.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;

namespace BitrixJiraConnector.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly IHostApplicationLifetime _lifetime;

    public HealthController(IHostApplicationLifetime lifetime)
    {
        _lifetime = lifetime;
    }

    [HttpGet]
    public ActionResult<ApiResponse<object>> GetHealth()
    {
        return Ok(ApiResponse<object>.Ok(new
        {
            status = "running",
            startedAt = DateTime.Now,
            applicationStopping = _lifetime.ApplicationStopping.IsCancellationRequested,
        }));
    }
}
