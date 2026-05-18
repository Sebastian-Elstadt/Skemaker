using App.Abstractions;
using App.Utils;
using Domain.Documents;

namespace App.Documents;

public class DocumentsService(IFileStore fileStore, IRecordStore recordStore) : IDocumentsService
{
    public async Task<Guid> StoreDocumentAsync(Stream fileStream, string fileName, CancellationToken ct = default)
    {
        string storedFilePath = await fileStore.StoreAsync("documents", fileStream, ct);

        try
        {
            var doc = new Document(fileName, storedFilePath);
            await recordStore.DocumentRepository.AddAsync(doc, ct);
            return doc.Id;
        }
        catch
        {
            ExecutionUtils.GracefullyFail(() => fileStore.Delete(storedFilePath));
            throw;
        }
    }
}