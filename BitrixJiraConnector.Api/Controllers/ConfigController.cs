using BitrixJiraConnector.Api.Models.Dto;
using BitrixJiraConnector.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BitrixJiraConnector.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ConfigController : ControllerBase
{
    private readonly IDbService _dbService;

    public ConfigController(IDbService dbService)
    {
        _dbService = dbService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<object>>> GetConfig()
    {
        var configs = await _dbService.GetConfigDatasAsync();
        return Ok(ApiResponse<object>.Ok(configs));
    }

    [HttpPut]
    public async Task<ActionResult<ApiResponse<object>>> SaveConfig([FromBody] SaveConfigRequest request)
    {
        await _dbService.SaveConfigAsync(request.QuetLaiSau, request.GuiLaiEmailSau, request.SoNgayQuet);
        return Ok(ApiResponse<object>.Ok(null!, "Config saved"));
    }
}
