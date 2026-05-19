namespace Domain.Translation;

public class AnalysisTranslation<T>
{
    public Guid Id { get; private init; } = Guid.NewGuid();
    public DateTime CreatedOn { get; private init; } = DateTime.UtcNow;

    public Guid AnalysisId { get; init; }
    public AnalysisTranslationTarget Target { get; set; }
    public T Translation { get; set; }

    private AnalysisTranslation() { Translation = default!; }
    public AnalysisTranslation(Guid analysisId, AnalysisTranslationTarget target, T translation)
    {
        if (analysisId == Guid.Empty)
            throw new ArgumentException("Analysis ID cannot be empty.");

        AnalysisId = analysisId;
        Target = target;
        Translation = translation;
    }

    public static AnalysisTranslation<TTranslation> Reconstitute<TTranslation>(
        Guid id,
        DateTime createdOn,
        Guid analysisId,
        AnalysisTranslationTarget target,
        TTranslation translation
    ) => new AnalysisTranslation<TTranslation>
    {
        Id = id,
        CreatedOn = createdOn,
        AnalysisId = analysisId,
        Target = target,
        Translation = translation
    };
}