using Domain.Analysis;

namespace App.Analysis;

public record DocumentAnalysisItem(
    Guid AnalysisId,
    DateTime CreatedOn,
    DocumentAnalysisType AnalysisType,
    string AnalysisJson
)
{
    public static DocumentAnalysisItem FromAnalysis(DocumentAnalysis analysis) => new(
        analysis.Id,
        analysis.CreatedOn,
        analysis.Type,
        analysis.AnalysisJson
    );
}