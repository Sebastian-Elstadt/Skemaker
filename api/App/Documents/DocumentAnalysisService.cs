using App.Abstractions;
using Domain.Documents;

namespace App.Documents;

public class DocumentAnalysisService(IRecordStore recordStore, IGdAndTAnalyzer gdAndTAnalyzer) : IDocumentAnalysisService
{
    public async Task<DocumentAnalysisItem> RunGdAndTAnalysisAsync(Guid id, CancellationToken ct = default)
    {
        var doc = await recordStore.DocumentRepository.GetByIdAsync(id, ct);
        if (doc is null) throw new ArgumentException($"Document with ID {id} not found.");

        var analysisJson = await gdAndTAnalyzer.AnalyzeDocumentAsync(doc, ct);
        var analysis = new DocumentAnalysis(doc.Id, DocumentAnalysisType.GdAndT, analysisJson);
        await recordStore.DocumentAnalysisRepository.AddAsync(analysis, ct);

        return new(analysis.Id, analysis.Type, analysisJson);
    }
}