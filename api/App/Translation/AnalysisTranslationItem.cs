using Domain.Translation;

namespace App.Translation;

public record AnalysisTranslationItem<TTranslation>(
    Guid Id,
    DateTime CreatedOn,
    Guid AnalysisId,
    AnalysisTranslationTarget Target,
    TTranslation Translation
)
{
    public static AnalysisTranslationItem<T> FromAnalysisTranslation<T>(AnalysisTranslation<T> translation)
        => new(
            translation.Id,
            translation.CreatedOn,
            translation.AnalysisId,
            translation.Target,
            translation.Translation
        );
}