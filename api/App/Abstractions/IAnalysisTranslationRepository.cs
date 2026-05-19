using Domain.Translation;

namespace App.Abstractions;

public interface IAnalysisTranslationRepository
{
    Task AddAsync<TTranslation>(AnalysisTranslation<TTranslation> translation, CancellationToken ct = default);
    Task<AnalysisTranslation<TTranslation>?> GetByIdAsync<TTranslation>(Guid id, CancellationToken ct = default);
    Task<IEnumerable<AnalysisTranslation<TTranslation>>> GetByAnalysisIdAsync<TTranslation>(Guid analysisId, CancellationToken ct = default);
}