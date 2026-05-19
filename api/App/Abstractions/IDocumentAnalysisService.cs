using App.Analysis;

namespace App.Abstractions;

public interface IDocumentAnalysisService
{
    Task<DocumentAnalysisItem> RunGdAndTAnalysisAsync(Guid documentId, CancellationToken ct = default);
    Task<DocumentAnalysisItem?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IEnumerable<DocumentAnalysisListItem>> GetByDocumentIdAsync(Guid documentId, CancellationToken ct = default);
}