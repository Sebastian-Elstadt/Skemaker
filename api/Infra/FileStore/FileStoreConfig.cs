namespace Infra.FileStorage;

public record FileStoreConfig(
    string BasePath
)
{
    public void EnsureValid()
    {
        if (string.IsNullOrWhiteSpace(BasePath))
            throw new InvalidOperationException("FileStore BasePath is required.");
    }
}