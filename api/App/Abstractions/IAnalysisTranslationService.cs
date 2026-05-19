using App.Translation;
using Domain.Translation;

namespace App.Abstractions;

public interface IAnalysisTranslationService
{
    Task<AnalysisTranslation<ToGCodeTranslationResult>> TranslateToGCodeAsync(Guid analysisId, TranslateToGCodeOptions options, CancellationToken ct);
}