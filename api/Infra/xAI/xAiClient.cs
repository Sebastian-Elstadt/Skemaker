using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace Infra.xAI;

public class xAiClient(HttpClient client)
{
    public async Task<string> UploadFileAsync(string fileName, Stream fileStream, string contentType, CancellationToken ct = default)
    {
        using var form = new MultipartFormDataContent();
        using var fileContent = new StreamContent(fileStream);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);

        form.Add(new StringContent("86400"), "expires_after");
        form.Add(new StringContent("assistants"), "purpose");
        form.Add(fileContent, "file", fileName);

        using var request = new HttpRequestMessage(HttpMethod.Post, "/v1/files")
        {
            Content = form
        };

        using var response = await client.SendAsync(request, ct);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(ct);
            throw new InvalidOperationException($"xAI file upload failed with status code {response.StatusCode}: {errorContent}");
        }

        var json = await response.Content.ReadAsStringAsync(ct);

        using var docJson = JsonDocument.Parse(json);
        string fileId = docJson.RootElement.GetProperty("id").GetString()
            ?? throw new InvalidOperationException("xAI file upload response missing id.");

        return fileId;
    }

    public async Task<string> GenerateResponseAsync(object[] inputs, object? formatOptions = null, uint maxOutputTokens = 2000, string model = "grok-4.20-0309-reasoning", CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "v1/responses")
        {
            Content = JsonContent.Create(new
            {
                model,
                max_output_tokens = maxOutputTokens, // 12000
                stream = true,
                input = inputs,
                text = new
                {
                    format = formatOptions
                }
            })
        };

        using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException($"xAI analysis request failed with status code {response.StatusCode}: {errorContent}");
        }

        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(stream);
        var sb = new StringBuilder();

        string? line;
        while ((line = await reader.ReadLineAsync(cancellationToken)) is not null)
        {
            if (line is null || !line.StartsWith("data: ")) continue;

            var data = line["data: ".Length..];
            if (data == "[DONE]") break;

            using var json = JsonDocument.Parse(data);
            var root = json.RootElement;

            if (
                root.TryGetProperty("type", out var typeEl) &&
                typeEl.GetString() == "response.output_text.delta" &&
                root.TryGetProperty("delta", out var deltaEl)
            ) sb.Append(deltaEl.GetString());
        }

        return sb.ToString();
    }
}