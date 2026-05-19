using Domain.Translation;

namespace App.Translation;

public record AnalysisTranslationListItem(
    Guid Id,
    DateTime CreatedOn,
    Guid AnalysisId,
    AnalysisTranslationTarget Target
)
{
    public static AnalysisTranslationListItem FromAnalysisTranslation<T>(AnalysisTranslation<T> translation)
        => new(
            translation.Id,
            translation.CreatedOn,
            translation.AnalysisId,
            translation.Target
        );
}