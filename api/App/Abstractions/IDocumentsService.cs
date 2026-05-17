namespace App.Abstractions;

public interface IDocumentsService
{
    Task<Guid> StoreDocumentAsync(Stream fileStream, string fileName, CancellationToken ct = default);
}