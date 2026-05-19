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

    public Task AddAsync(DocumentAnalysis analysis, CancellationToken ct = default)
    {
        return executor.ExecuteAsync(
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

    public Task<DocumentAnalysis?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return executor.QuerySingleAsync(
            "SELECT * FROM document_analyses WHERE id = @id",
            new { id },
            MapRow,
            ct
        );
    }

    public Task<IEnumerable<DocumentAnalysis>> GetByDocumentIdAsync(Guid documentId, CancellationToken ct = default)
    {
        return executor.QueryAsync(
            "SELECT * FROM document_analyses WHERE document_id = @documentId",
            new { documentId },
            MapRow,
            ct
        );
    }
}