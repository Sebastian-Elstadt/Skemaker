using Infra.Abstractions;

namespace Infra.Documents;

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
            "UPDATE xai_document_uploads SET file_id = @fileId WHERE document_id = @documentId",
            new { documentId, fileId },
            ct
        );
    }
}