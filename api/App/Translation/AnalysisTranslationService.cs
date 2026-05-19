using App.Abstractions;

namespace App.Translation;

public class AnalysisTranslationService(IRecordStore recordStore) : IAnalysisTranslationService
{
    public async Task TranslateToGCodeAsync(Guid analysisId, TranslateToGCodeOptions options, CancellationToken ct)
    {
        var analysis = await recordStore.DocumentAnalysisRepository.GetByIdAsync(analysisId, ct);
        if (analysis is null)
            throw new ArgumentException($"No analysis found with ID {analysisId}");
            
        
    }
}