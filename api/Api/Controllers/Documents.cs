using App.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace App.Controllers;

[ApiController]
[Route("documents")]
public class DocumentsController(IDocumentsService documents) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateAsync(IFormFile file)
    {
        using var stream = file.OpenReadStream();
        var docId = await documents.StoreDocumentAsync(stream, file.FileName, HttpContext.RequestAborted);
        return Ok(new { DocumentId = docId });
    }

    [HttpGet]
    public async Task<IActionResult> GetAllAsync()
    {
        var docs = await documents.GetAllDocumentsAsync(HttpContext.RequestAborted);
        return Ok(docs);
    }
}