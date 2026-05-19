using Domain.Analysis;

namespace App.Abstractions;

public interface IDocumentAnalysisRepository
{
    Task AddAsync(DocumentAnalysis analysis, CancellationToken ct = default);
    Task<DocumentAnalysis?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IEnumerable<DocumentAnalysis>> GetByDocumentIdAsync(Guid documentId, CancellationToken ct = default);
}