using App.Abstractions;
using App.Utils;
using Domain.Documents;

namespace App.Documents;

public class DocumentsService(IFileStorage fileStorage, IDocumentRepository repository) : IDocumentsService
{
    public async Task<Guid> StoreDocumentAsync(Stream fileStream, string fileName, CancellationToken ct = default)
    {
        string storedFilePath = await fileStorage.StoreAsync(fileStream, ct);

        try
        {
            var doc = new Document(fileName, storedFilePath);
            await repository.AddAsync(doc, ct);
            return doc.Id;
        }
        catch
        {
            await ExecutionUtils.GracefullyFailAsync(() => fileStorage.DeleteAsync(storedFilePath, ct));
            throw;
        }
    }
}