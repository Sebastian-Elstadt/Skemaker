using App.Abstractions;

namespace App.Translation;

public class AnalysisTranslationService(IRecordStore recordStore, IGdAndTAnalysisToGCodeTranslator gdAndTAnalysisToGCodeTranslator) : IAnalysisTranslationService
{
    public async Task TranslateToGCodeAsync(Guid analysisId, TranslateToGCodeOptions options, CancellationToken ct)
    {
        var analysis = await recordStore.DocumentAnalysisRepository.GetByIdAsync(analysisId, ct);
        if (analysis is null)
            throw new ArgumentException($"No analysis found with ID {analysisId}");

        switch (analysis.Type)
        {
            case Domain.Documents.DocumentAnalysisType.GdAndT:
                await gdAndTAnalysisToGCodeTranslator.TranslateAsync(analysis, ct);
                break;
            // could be expanded to whatever else type of analyses may be made
        }
    }
}