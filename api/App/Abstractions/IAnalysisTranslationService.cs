using App.Translation;

namespace App.Abstractions;

public interface IAnalysisTranslationService
{
    Task TranslateToGCodeAsync(Guid analysisId, TranslateToGCodeOptions options, CancellationToken ct);
}