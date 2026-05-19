using App.Abstractions;
using Domain.Documents;
using Infra.Abstractions;

namespace Infra.Repositories;

public class DocumentRepository(IQueryExecutor executor) : IDocumentRepository
{
    private Document MapRow(dynamic row) => Document.Reconstitute(
        (Guid)row.id,
        (DateTime)row.created_on,
        (string)row.file_name,
        (string)row.file_path,
        (string)row.file_hash,
        Convert.ToUInt32(row.size_bytes),
        (string)row.content_type
    );

    public async Task AddAsync(Document doc, CancellationToken ct = default)
    {
        await executor.ExecuteAsync(
            """
            INSERT INTO documents (id, created_on, file_name, file_path, file_hash, size_bytes, content_type)
            VALUES (@Id, @CreatedOn, @FileName, @FilePath, @FileHash, @SizeBytes, @ContentType);
            """,
            new
            {
                doc.Id,
                doc.CreatedOn,
                doc.FileName,
                doc.FilePath,
                doc.FileHash,
                SizeBytes = (int)doc.SizeBytes,
                doc.ContentType
            },
            ct
        );
    }

    public Task<IEnumerable<Document>> GetAllAsync(CancellationToken ct = default)
    {
        return executor.QueryAsync(
            "SELECT * FROM documents",
            null,
            MapRow,
            ct
        );
    }

    public Task<Document?> GetByFileHashAsync(string fileHash, CancellationToken ct = default)
    {
        return executor.QuerySingleAsync(
            "SELECT * FROM documents WHERE file_hash = @fileHash",
            new { fileHash },
            MapRow,
            ct
        );
    }

    public Task<Document?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return executor.QuerySingleAsync(
            "SELECT * FROM documents WHERE id = @id",
            new { id },
            MapRow,
            ct
        );
    }
}