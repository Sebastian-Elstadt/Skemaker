using App.Analysis;
using Domain.Documents;

namespace App.Abstractions;

public interface IGdAndTAnalyzer
{
    DocumentGdAndTAnalysis ParseAnalysis(string analysisJson);
    Task<string> AnalyzeDocumentAsync(Document doc, CancellationToken ct = default);
}