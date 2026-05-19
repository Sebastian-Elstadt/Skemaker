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
        var list = await recordStore.AnalysisTranslationRepository.GetByAnalysisIdAsync<GCodeTranslation>(analysisId, ct);
        return list.Select(x => AnalysisTranslationListItem.FromAnalysisTranslation(x));
    }

    public async Task<AnalysisTranslationItem<GCodeTranslation>?> GetByIdAsync(Guid translationId, CancellationToken ct = default)
    {
        var translation = await recordStore.AnalysisTranslationRepository.GetByIdAsync<GCodeTranslation>(translationId, ct);
        if (translation is null) return null;
        
        return AnalysisTranslationItem<GCodeTranslation>.FromAnalysisTranslation(translation);
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

        var translation = new AnalysisTranslation<GCodeTranslation>(analysisId, translationResult);
        await recordStore.AnalysisTranslationRepository.AddAsync(translation, ct);

        return AnalysisTranslationItem<GCodeTranslation>.FromAnalysisTranslation(translation);
    }
}