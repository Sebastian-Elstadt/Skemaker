using App.Abstractions;
using App.Utils;
using Domain.Documents;

namespace App.Documents;

public class DocumentsService(
    IFileStore fileStore, 
    IRecordStore recordStore
) : IDocumentsService
{
    private const string DocumentsDirName = "documents";
    private const string DefaultContentType = "application/octet-stream";

    public async Task<Guid> StoreDocumentAsync(Stream fileStream, string fileName, string? contentType, CancellationToken ct = default)
    {
        var (filePath, fileHash) = await fileStore.StoreAsync(DocumentsDirName, fileStream, ct);
        contentType = NormalizeContentType(contentType);

        try
        {
            var doc = await recordStore.DocumentRepository.GetByFileHashAsync(fileHash, ct);
            if (doc is { }) return doc.Id;

            doc = new Document(fileName, filePath, contentType, fileHash, (uint)fileStream.Length);
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
        return new(stream, doc.FileName, doc.ContentType);
    }

    private static string NormalizeContentType(string? contentType)
    {
        if (string.IsNullOrWhiteSpace(contentType))
            return DefaultContentType;

        contentType = contentType.Trim().ToLowerInvariant();
        
        var slashIdx = contentType.IndexOf('/');
        if (slashIdx <= 0 || slashIdx >= contentType.Length - 1)
            return DefaultContentType;

        if (contentType.Contains(' ') || contentType.Contains(';') || contentType.Contains(',') || contentType.Contains('\r') || contentType.Contains('\n'))
            return DefaultContentType;

        return contentType;
    }
}