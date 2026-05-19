using System.Text.Json;
using App.Abstractions;
using Domain.Translation;
using Infra.Abstractions;

namespace Infra.Repositories;

public class AnalysisTranslationRepository(IQueryExecutor executor) : IAnalysisTranslationRepository
{
    private AnalysisTranslation<TTranslation> MapRow<TTranslation>(dynamic row) => AnalysisTranslation<TTranslation>.Reconstitute<TTranslation>(
        id: (Guid)row.id,
        createdOn: (DateTime)row.created_on,
        analysisId: (Guid)row.analysis_id,
        translation: JsonSerializer.Deserialize<TTranslation>((string)row.translation_json)
            ?? throw new InvalidOperationException($"Failed to deserialize translation JSON for analysis translation with ID {row.id}")
    );

    public Task AddAsync<TTranslation>(AnalysisTranslation<TTranslation> translation, CancellationToken ct = default)
    {
        return executor.ExecuteAsync(
            """
            INSERT INTO analysis_translations (id, created_on, analysis_id, translation_json)
            VALUES (@Id, @CreatedOn, @AnalysisId, @TranslationJson::jsonb);
            """,
            new
            {
                translation.Id,
                translation.CreatedOn,
                translation.AnalysisId,
                TranslationJson = JsonSerializer.Serialize(translation.Translation)
            },
            ct
        );
    }

    public Task<IEnumerable<AnalysisTranslation<TTranslation>>> GetByAnalysisIdAsync<TTranslation>(Guid analysisId, CancellationToken ct = default)
    {
        return executor.QueryAsync(
            "SELECT * FROM analysis_translations WHERE analysis_id = @analysisId",
            new { analysisId },
            MapRow<TTranslation>,
            ct
        );
    }

    public Task<AnalysisTranslation<TTranslation>?> GetByIdAsync<TTranslation>(Guid id, CancellationToken ct = default)
    {
        return executor.QuerySingleAsync(
            "SELECT * FROM analysis_translations WHERE id = @id",
            new { id },
            MapRow<TTranslation>,
            ct
        );
    }
}