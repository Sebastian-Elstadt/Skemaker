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
            INSERT INTO documents (id, created_on, file_name, file_path)
            VALUES (@Id, @CreatedOn, @FileName, @FilePath);
            """,
            new
            {
                doc.Id,
                doc.CreatedOn,
                doc.FileName,
                doc.FilePath
            },
            ct
        );
    }
}