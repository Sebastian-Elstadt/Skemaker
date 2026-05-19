using Infra.Abstractions;

namespace Infra.xAI;

public class xAiUploadStore(IQueryExecutor executor)
{
    public Task<string?> GetFileIdByDocumentIdAsync(Guid documentId, CancellationToken ct = default)
    {
        return executor.QuerySingleAsync(
            "SELECT file_id FROM xai_document_uploads WHERE document_id = @documentId",
            new { documentId },
            row => (string)row.file_id,
            ct
        );
    }

    public Task SetFileIdForDocumentIdAsync(Guid documentId, string fileId, CancellationToken ct = default)
    {
        return executor.ExecuteAsync(
            "INSERT INTO xai_document_uploads (document_id, file_id) VALUES (@documentId, @fileId) ON CONFLICT (document_id) DO UPDATE SET file_id = @fileId",
            new { documentId, fileId },
            ct
        );
    }
}