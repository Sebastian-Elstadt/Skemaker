using Domain.Documents;

namespace App.Abstractions;

public interface IDocumentAnalysisRepository
{
    Task AddAsync(DocumentAnalysis analysis, CancellationToken ct = default);
}