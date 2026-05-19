using App.Translation;

namespace App.Abstractions;

public interface IAnalysisTranslationService
{
    Task<AnalysisTranslationItem<GCodeTranslation>> TranslateToGCodeAsync(Guid analysisId, GCodeManufacturingOptions options, CancellationToken ct);
    Task<IEnumerable<AnalysisTranslationListItem>> GetByAnalysisIdAsync(Guid analysisId, CancellationToken ct = default);
    Task<AnalysisTranslationItem<GCodeTranslation>?> GetByIdAsync(Guid translationId, CancellationToken ct = default);
}