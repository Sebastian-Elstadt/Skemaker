using App.Translation;

namespace App.Abstractions;

public interface IAnalysisTranslationService
{
    Task<AnalysisTranslationItem<GCodeTranslation>> TranslateToGCodeAsync(Guid analysisId, GCodeManufacturingOptions options, CancellationToken ct);
}