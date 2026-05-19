using App.Abstractions;
using Domain.Analysis;

namespace App.Analysis;

public class DocumentAnalysisService(IRecordStore recordStore, IGdAndTAnalyzer gdAndTAnalyzer) : IDocumentAnalysisService
{
    public async Task<IEnumerable<DocumentAnalysisListItem>> GetByDocumentIdAsync(Guid documentId, CancellationToken ct = default)
    {
        var list = await recordStore.DocumentAnalysisRepository.GetByDocumentIdAsync(documentId, ct);
        return list.Select(DocumentAnalysisListItem.FromAnalysis);
    }

    public async Task<DocumentAnalysisItem?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var analysis = await recordStore.DocumentAnalysisRepository.GetByIdAsync(id, ct);
        if (analysis is null) return null;

        return DocumentAnalysisItem.FromAnalysis(analysis);
    }

    public async Task<DocumentAnalysisItem> RunGdAndTAnalysisAsync(Guid documentId, CancellationToken ct = default)
    {
        var doc = await recordStore.DocumentRepository.GetByIdAsync(documentId, ct);
        if (doc is null) throw new ArgumentException($"Document with ID {documentId} not found.");

        var analysisJson = await gdAndTAnalyzer.AnalyzeDocumentAsync(doc, ct);
        var analysis = new DocumentAnalysis(doc.Id, DocumentAnalysisType.GdAndT, analysisJson);
        await recordStore.DocumentAnalysisRepository.AddAsync(analysis, ct);

        return DocumentAnalysisItem.FromAnalysis(analysis);
    }
}