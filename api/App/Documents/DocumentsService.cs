using App.Abstractions;
using App.Utils;
using Domain.Documents;

namespace App.Documents;

public class DocumentsService(IFileStore fileStore, IRecordStore recordStore) : IDocumentsService
{
    public async Task<Guid> StoreDocumentAsync(Stream fileStream, string fileName, CancellationToken ct = default)
    {
        var (filePath, fileHash) = await fileStore.StoreAsync("documents", fileStream, ct);

        try
        {
            var doc = await recordStore.DocumentRepository.GetByFileHashAsync(fileHash, ct);
            if (doc is { }) return doc.Id;

            doc = new Document(fileName, filePath, fileHash);
            await recordStore.DocumentRepository.AddAsync(doc, ct);
            return doc.Id;
        }
        catch
        {
            ExecutionUtils.GracefullyFail(() => fileStore.Delete(filePath));
            throw;
        }
    }
}