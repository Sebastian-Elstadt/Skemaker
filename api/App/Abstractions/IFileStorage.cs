namespace App.Abstractions;

public interface IFileStorage
{
    Task<string> StoreAsync(Stream fileStream, CancellationToken ct = default);
    Task DeleteAsync(string filePath, CancellationToken ct = default);
}