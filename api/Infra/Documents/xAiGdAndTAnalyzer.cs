using System.Text.Json;
using App.Abstractions;
using App.Documents;

namespace Infra.Documents;

public class xAiGdAndTAnalyzer(HttpClient client) : IGdAndTAnalyzer
{
    public Task<string> AnalyzeDocumentAsync(Stream fileStream, string fileName, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public DocumentGdAndTAnalysis ParseAnalysis(string analysisJson)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true
        };

        return JsonSerializer.Deserialize<DocumentGdAndTAnalysis>(analysisJson, options) ?? throw new InvalidOperationException("Failed to parse Gd&T analysis JSON.");
    }
}