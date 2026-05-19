using App.Abstractions;
using Domain.Analysis;
using Domain.Translation;

namespace App.Translation;

public class AnalysisTranslationService(IRecordStore recordStore, IGdAndTAnalysisToGCodeTranslator gdAndTAnalysisToGCodeTranslator) : IAnalysisTranslationService
{
    public async Task<AnalysisTranslation<ToGCodeTranslationResult>> TranslateToGCodeAsync(Guid analysisId, TranslateToGCodeOptions options, CancellationToken ct)
    {
        var analysis = await recordStore.DocumentAnalysisRepository.GetByIdAsync(analysisId, ct);
        if (analysis is null)
            throw new ArgumentException($"No analysis found with ID {analysisId}");

        ToGCodeTranslationResult translationResult = analysis.Type switch
        {
            DocumentAnalysisType.GdAndT => await gdAndTAnalysisToGCodeTranslator.TranslateAsync(analysis, options, ct),
            _ => throw new NotSupportedException($"Analysis type {analysis.Type} is not supported for GCode translation")
            // could be expanded to whatever else type of analyses may be made
        };

        var translation = new AnalysisTranslation<ToGCodeTranslationResult>(analysisId, translationResult);
        await recordStore.AnalysisTranslationRepository.AddAsync(translation, ct);
        return translation;
    }
}