using Api.Requests;
using App.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("analysis")]
public class AnalysisController(
    IDocumentAnalysisService analysisService,
    IAnalysisTranslationService analysisTranslationService
) : ControllerBase
{
    [HttpPost("gdAndT")]
    public async Task<IActionResult> CreateGdAndTAnalysisAsync([FromBody] CreateGdAndTAnalysisRequest req)
    {
        var analysis = await analysisService.RunGdAndTAnalysisAsync(req.DocumentId, HttpContext.RequestAborted);
        return Ok(analysis);
    }

    [HttpGet("by-document/{docId:Guid}")]
    public async Task<IActionResult> GetByDocumentIdAsync([FromRoute] Guid docId)
    {
        var list = await analysisService.GetByDocumentIdAsync(docId, HttpContext.RequestAborted);
        return Ok(list);
    }

    [HttpPost("{analysisId:Guid}/translate/gCode")]
    public async Task<IActionResult> TranslateAnalysisToGCodeAsync(
        [FromRoute] Guid analysisId,
        [FromBody] TranslateAnalysisToGCodeRequest req
    )
    {
        await analysisTranslationService.TranslateToGCodeAsync(analysisId, req.ToTranslateToGCodeOptions(), HttpContext.RequestAborted);
        return Ok();
    }

    [HttpGet("{analysisId:Guid}")]
    public async Task<IActionResult> GetByIdAsync([FromRoute] Guid analysisId)
    {
        var analysis = await analysisService.GetByIdAsync(analysisId, HttpContext.RequestAborted);
        if (analysis is null) return NotFound();
        return Ok(analysis);
    }
}