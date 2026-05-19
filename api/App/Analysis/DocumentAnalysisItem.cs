using Domain.Documents;

namespace App.Analysis;

public record DocumentAnalysisItem(
    Guid AnalysisId,
    DateTime CreatedOn,
    DocumentAnalysisType AnalysisType,
    string AnalysisJson
);