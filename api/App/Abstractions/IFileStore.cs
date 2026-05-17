namespace App.Abstractions;

public interface IFileStore
{
    Task<string> StoreAsync(string dirPath, Stream fileStream, CancellationToken ct = default);
    void Delete(string filePath);
}