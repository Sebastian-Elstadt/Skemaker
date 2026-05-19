using Domain.Documents;

namespace App.Analysis;

public record DocumentAnalysisListItem(
    Guid AnalysisId,
    DocumentAnalysisType AnalysisType,
    DateTime CreatedOn
);