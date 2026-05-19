using Domain.Analysis;

namespace App.Analysis;

public record DocumentAnalysisItem(
    Guid AnalysisId,
    DateTime CreatedOn,
    DocumentAnalysisType AnalysisType,
    string AnalysisJson
);