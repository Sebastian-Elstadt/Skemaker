using App.Documents;

namespace App.Abstractions;

public interface IDocumentsService
{
    Task<Guid> StoreDocumentAsync(Stream fileStream, string fileName, CancellationToken ct = default);
    Task<IEnumerable<DocumentItem>> GetAllDocumentsAsync(CancellationToken ct = default);
    Task<DocumentFile?> GetDocumentFileAsync(Guid id, CancellationToken ct = default);
    Task<string> RunDocumentGdAndTAnalysisAsync(Guid id, CancellationToken ct = default);
}
