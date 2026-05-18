namespace Infra.RecordStore;

public record RecordStoreConfig(
    string ConnectionString
)
{
    public void EnsureValid()
    {
        if (string.IsNullOrWhiteSpace(ConnectionString))
            throw new InvalidOperationException("RecordStore ConnectionString is required.");
    }
}