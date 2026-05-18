using App.Abstractions;
using Domain.Documents;
using Infra.Abstractions;

namespace Infra.Repositories;

public class DocumentAnalysisRepository(IQueryExecutor executor) : IDocumentAnalysisRepository
{
    private DocumentAnalysis MapRow(dynamic row) => DocumentAnalysis.Reconstitute(
        (Guid)row.id,
        (DateTime)row.created_on,
        (Guid)row.document_id,
        (DocumentAnalysisType)row.type,
        (string)row.analysis_json
    );

    public async Task AddAsync(DocumentAnalysis analysis, CancellationToken ct = default)
    {
        await executor.ExecuteAsync(
            """
            INSERT INTO document_analyses (id, created_on, document_id, type, analysis_json)
            VALUES (@Id, @CreatedOn, @DocumentId, @Type, @AnalysisJson);
            """,
            new
            {
                analysis.Id,
                analysis.CreatedOn,
                analysis.DocumentId,
                Type = (short)analysis.Type,
                analysis.AnalysisJson
            },
            ct
        );
    }
}