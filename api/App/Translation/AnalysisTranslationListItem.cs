using Domain.Translation;

namespace App.Translation;

public record AnalysisTranslationListItem(
    Guid Id,
    DateTime CreatedOn,
    Guid AnalysisId
)
{
    public static AnalysisTranslationListItem FromAnalysisTranslation<T>(AnalysisTranslation<T> translation)
        => new(
            translation.Id,
            translation.CreatedOn,
            translation.AnalysisId
        );
}