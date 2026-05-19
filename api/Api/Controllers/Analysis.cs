using App.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("analysis")]
public class AnalysisController(IDocumentAnalysisService analysisService) : ControllerBase
{
    public record CreateGdAndTAnalysisRequest(Guid DocumentId);

    [HttpPost("gdAndT")]
    public async Task<IActionResult> CreateGdAndTAnalysisAsync([FromBody] CreateGdAndTAnalysisRequest req)
    {
        var analysis = await analysisService.RunGdAndTAnalysisAsync(req.DocumentId, HttpContext.RequestAborted);
        return Ok(analysis);
    }
}