using Api.Requests;
using App.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("translation")]
public class TranslationController(IAnalysisTranslationService translationService) : ControllerBase
{
    [HttpPost("gCode")]
    public async Task<IActionResult> TranslateToGCodeAsync([FromBody] TranslateToGCodeRequest req)
    {
        var result = await translationService.TranslateToGCodeAsync(req.AnalysisId, req.ToGCodeManufacturingOptions(), HttpContext.RequestAborted);
        return Ok(result);
    }

    [HttpGet("by-analysis/{analysisId:Guid}")]
    public async Task<IActionResult> GetByAnalysisIdAsync([FromRoute] Guid analysisId)
    {
        var list = await translationService.GetByAnalysisIdAsync(analysisId, HttpContext.RequestAborted);
        return Ok(list);
    }

    [HttpGet("{translationId:Guid}")]
    public async Task<IActionResult> GetByIdAsync([FromRoute] Guid translationId)
    {
        var translation = await translationService.GetByIdAsync<object>(translationId, HttpContext.RequestAborted);
        if (translation is null) return NotFound();
        return Ok(translation);
    }
}