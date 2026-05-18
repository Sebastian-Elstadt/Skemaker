namespace App.Abstractions;

public interface IFileStore
{
    Task<(string filePath, string fileHash)> StoreAsync(string dirPath, Stream fileStream, CancellationToken ct = default);
    void Delete(string filePath);
    Stream OpenReadStream(string filePath);
}