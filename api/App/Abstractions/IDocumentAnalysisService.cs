using App.Documents;

namespace App.Abstractions;

public interface IDocumentAnalysisService
{
    Task<DocumentAnalysisItem> RunGdAndTAnalysisAsync(Guid id, CancellationToken ct = default);
}