using App.Abstractions;
using Domain.Analysis;
using Domain.Translation;

namespace App.Translation;

public class AnalysisTranslationService(
    IRecordStore recordStore,
    IGdAndTAnalysisToGCodeTranslator gdAndTAnalysisToGCodeTranslator
) : IAnalysisTranslationService
{
    public async Task<IEnumerable<AnalysisTranslationListItem>> GetByAnalysisIdAsync(Guid analysisId, CancellationToken ct = default)
    {
        var list = await recordStore.AnalysisTranslationRepository.GetByAnalysisIdAsync<object>(analysisId, ct);
        return list.Select(AnalysisTranslationListItem.FromAnalysisTranslation);
    }

    public async Task<AnalysisTranslationItem<T>?> GetByIdAsync<T>(Guid translationId, CancellationToken ct = default)
    {
        var translation = await recordStore.AnalysisTranslationRepository.GetByIdAsync<T>(translationId, ct);
        if (translation is null) return null;
        
        return AnalysisTranslationItem<T>.FromAnalysisTranslation(translation);
    }

    public async Task<AnalysisTranslationItem<GCodeTranslation>> TranslateToGCodeAsync(Guid analysisId, GCodeManufacturingOptions options, CancellationToken ct)
    {
        var analysis = await recordStore.DocumentAnalysisRepository.GetByIdAsync(analysisId, ct);
        if (analysis is null)
            throw new ArgumentException($"No analysis found with ID {analysisId}");

        GCodeTranslation translationResult = analysis.Type switch
        {
            DocumentAnalysisType.GdAndT => await gdAndTAnalysisToGCodeTranslator.TranslateAsync(analysis, options, ct),
            _ => throw new NotSupportedException($"Analysis type {analysis.Type} is not supported for GCode translation")
            // could be expanded to whatever else type of analyses may be made
        };

        var translation = new AnalysisTranslation<GCodeTranslation>(analysisId, AnalysisTranslationTarget.GCode, translationResult);
        await recordStore.AnalysisTranslationRepository.AddAsync(translation, ct);

        return AnalysisTranslationItem<GCodeTranslation>.FromAnalysisTranslation(translation);
    }
}