using Domain.Documents;

namespace App.Documents;

public record DocumentAnalysisListItem(
    Guid AnalysisId,
    DocumentAnalysisType AnalysisType,
    DateTime CreatedOn
);