using Domain.Documents;

namespace App.Abstractions;

public interface IDocumentRepository
{
    Task AddAsync(Document doc, CancellationToken ct = default);
}