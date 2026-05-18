namespace Infra.Documents;

public record xAiConfig(
    string BaseUrl,
    string ApiKey
)
{
    public void EnsureValid()
    {
        if (string.IsNullOrWhiteSpace(BaseUrl))
            throw new InvalidOperationException("xAI BaseUrl is required.");

        if (string.IsNullOrWhiteSpace(ApiKey))
            throw new InvalidOperationException("xAI ApiKey is required.");
    }
}