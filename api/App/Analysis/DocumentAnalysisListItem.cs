using Domain.Analysis;

namespace App.Analysis;

public record DocumentAnalysisListItem(
    Guid AnalysisId,
    DocumentAnalysisType AnalysisType,
    DateTime CreatedOn
)
{
    public static DocumentAnalysisListItem FromAnalysis(DocumentAnalysis analysis) => new(
        analysis.Id,
        analysis.Type,
        analysis.CreatedOn
    );
}