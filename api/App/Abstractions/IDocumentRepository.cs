using Domain.Documents;

namespace App.Abstractions;

public interface IDocumentRepository
{
    Task AddAsync(Document doc, CancellationToken ct = default);
    Task<Document?> GetByFileHashAsync(string fileHash, CancellationToken ct = default);
    Task<IEnumerable<Document>> GetAllAsync(CancellationToken ct = default);
}