using Api.Requests;
using App.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("translation")]
public class TranslationController(IAnalysisTranslationService analysisTranslationService) : ControllerBase
{
    [HttpPost("gCode")]
    public async Task<IActionResult> TranslateToGCodeAsync(
        [FromRoute] Guid analysisId,
        [FromBody] TranslateToGCodeRequest req
    )
    {
        var result = await analysisTranslationService.TranslateToGCodeAsync(analysisId, req.ToGCodeManufacturingOptions(), HttpContext.RequestAborted);
        return Ok(result);
    }
}