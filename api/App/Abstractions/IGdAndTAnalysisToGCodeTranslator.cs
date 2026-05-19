using App.Translation;
using Domain.Analysis;

namespace App.Abstractions;

public interface IGdAndTAnalysisToGCodeTranslator
{
    Task<ToGCodeTranslationResult> TranslateAsync(DocumentAnalysis analysis, TranslateToGCodeOptions options, CancellationToken ct = default);
}