using Domain.Documents;

namespace App.Abstractions;

public interface IGdAndTAnalysisToGCodeTranslator
{
    Task TranslateAsync(DocumentAnalysis analysis, CancellationToken ct = default);
}