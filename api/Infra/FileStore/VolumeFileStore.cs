using System.Security.Cryptography;
using App.Abstractions;

namespace Infra.FileStorage;

public class VolumeFileStore(FileStoreConfig config) : IFileStore
{
    public void Delete(string filePath)
    {
        string fullPath = Path.Combine(config.BasePath, filePath);

        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }
    }

    public async Task<string> StoreAsync(string dirPath, Stream fileStream, CancellationToken ct = default)
    {
        string fullDirPath = Path.Combine(config.BasePath, dirPath);
        Directory.CreateDirectory(fullDirPath);

        string tempPath = Path.Combine(fullDirPath, $".upload-{Guid.NewGuid():N}.tmp");

        try
        {
            using var hasher = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
            await using var writeStream = new FileStream(
                tempPath,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                bufferSize: 81920,
                options: FileOptions.Asynchronous | FileOptions.SequentialScan
            );

            byte[] buffer = new byte[81920];
            int read;
            while ((read = await fileStream.ReadAsync(buffer.AsMemory(0, buffer.Length), ct)) > 0)
            {
                hasher.AppendData(buffer, 0, read);
                await writeStream.WriteAsync(buffer.AsMemory(0, read), ct);
            }

            await writeStream.FlushAsync(ct);

            string fileHash = Convert.ToHexString(hasher.GetHashAndReset()).ToLowerInvariant();
            string finalPath = Path.Combine(fullDirPath, fileHash);

            if (File.Exists(finalPath))
            {
                File.Delete(tempPath);
                return fileHash;
            }

            File.Move(tempPath, finalPath);
            return Path.Combine(dirPath, fileHash);
        }
        catch
        {
            if (File.Exists(tempPath))
            {
                File.Delete(tempPath);
            }

            throw;
        }
    }
}