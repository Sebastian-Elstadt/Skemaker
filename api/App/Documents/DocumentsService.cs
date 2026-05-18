using App.Abstractions;
using App.Utils;
using Domain.Documents;

namespace App.Documents;

public class DocumentsService(IFileStore fileStore, IRecordStore recordStore) : IDocumentsService
{
    private const string DocumentsDirName = "documents";

    public async Task<Guid> StoreDocumentAsync(Stream fileStream, string fileName, CancellationToken ct = default)
    {
        var (filePath, fileHash) = await fileStore.StoreAsync(DocumentsDirName, fileStream, ct);

        try
        {
            var doc = await recordStore.DocumentRepository.GetByFileHashAsync(fileHash, ct);
            if (doc is { }) return doc.Id;

            doc = new Document(fileName, filePath, fileHash, (uint)fileStream.Length);
            await recordStore.DocumentRepository.AddAsync(doc, ct);
            return doc.Id;
        }
        catch
        {
            ExecutionUtils.GracefullyFail(() => fileStore.Delete(filePath));
            throw;
        }
    }

    public async Task<IEnumerable<DocumentItem>> GetAllDocumentsAsync(CancellationToken ct = default)
    {
        var docs = await recordStore.DocumentRepository.GetAllAsync(ct);
        return docs.Select(d => new DocumentItem(
            d.Id,
            d.CreatedOn,
            d.FileName,
            d.FileHash,
            d.SizeBytes
        ));
    }

    public async Task<DocumentFile?> GetDocumentFileAsync(Guid id, CancellationToken ct = default)
    {
        var doc = await recordStore.DocumentRepository.GetByIdAsync(id, ct);
        if (doc is null) return null;

        var stream = fileStore.OpenReadStream(doc.FilePath);
        return new(stream, doc.FileName, "application/octet-stream");
    }
}