using App.Documents;

namespace App.Abstractions;

public interface IGdAndTAnalyzer
{
    DocumentGdAndTAnalysis ParseAnalysis(string analysisJson);
    Task<string> AnalyzeDocumentAsync(Stream fileStream, string fileName, CancellationToken ct = default);
}