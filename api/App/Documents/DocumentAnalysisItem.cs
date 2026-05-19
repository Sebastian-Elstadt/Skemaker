using Domain.Documents;

namespace App.Documents;

public record DocumentAnalysisItem(
    Guid AnalysisId,
    DocumentAnalysisType AnalysisType,
    string AnalysisJson
);