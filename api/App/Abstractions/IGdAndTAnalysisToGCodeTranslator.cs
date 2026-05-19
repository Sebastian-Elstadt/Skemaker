using App.Translation;
using Domain.Analysis;

namespace App.Abstractions;

public interface IGdAndTAnalysisToGCodeTranslator
{
    Task<GCodeTranslation> TranslateAsync(DocumentAnalysis analysis, GCodeManufacturingOptions options, CancellationToken ct = default);
}