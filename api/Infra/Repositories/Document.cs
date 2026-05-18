using App.Abstractions;
using Domain.Documents;
using Infra.Abstractions;

namespace Infra.Repositories;

public class DocumentRepository(IQueryExecutor executor) : IDocumentRepository
{
    public async Task AddAsync(Document doc, CancellationToken ct = default)
    {
        await executor.ExecuteAsync(
            """
            INSERT INTO documents (id, created_on, file_name, file_path, file_hash)
            VALUES (@Id, @CreatedOn, @FileName, @FilePath, @FileHash);
            """,
            new
            {
                doc.Id,
                doc.CreatedOn,
                doc.FileName,
                doc.FilePath,
                doc.FileHash
            },
            ct
        );
    }

    public Task<Document?> GetByFileHashAsync(string fileHash, CancellationToken ct = default)
    {
        return executor.QuerySingleAsync<Document>(
            """
            SELECT
                id,
                created_on,
                file_name,
                file_path,
                file_hash
            FROM documents
            WHERE file_hash = @fileHash
            """,
            new { fileHash },
            row => Document.Reconstitute(
                row.id,
                row.created_on,
                row.file_name,
                row.file_path,
                row.file_hash
            ),
            ct
        );
    }
}