using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using App.Abstractions;
using App.Documents;
using Domain.Documents;

namespace Infra.Documents;

public class xAiGdAndTAnalyzer(
    HttpClient client,
    xAiUploadStore uploadStore,
    IFileStore fileStore
) : IGdAndTAnalyzer
{
    private async Task<string> UploadDocumentAsync(Document doc, CancellationToken ct = default)
    {
        using var fileStream = fileStore.OpenReadStream(doc.FilePath);

        using var form = new MultipartFormDataContent();
        using var fileContent = new StreamContent(fileStream);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(doc.ContentType);

        form.Add(new StringContent("86400"), "expires_after");
        form.Add(new StringContent("assistants"), "purpose");
        form.Add(fileContent, "file", doc.FileName);

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

        await uploadStore.SetFileIdForDocumentIdAsync(doc.Id, fileId, ct);

        return fileId;
    }

    public async Task<string> AnalyzeDocumentAsync(Document doc, CancellationToken ct = default)
    {
        var xAiFileId = await uploadStore.GetFileIdByDocumentIdAsync(doc.Id, ct)
            ?? await UploadDocumentAsync(doc, ct);

        using var response = await client.PostAsJsonAsync(
            "v1/chat/completions",
            new
            {
                model = "grok-4.3",
                messages = new object[] {
                    new {
                        role = "system",
                        content = "You are an expert mechanical engineer specialized in reading technical drawings and GD&T per ASME Y14.5."
                    },
                    new {
                        role = "user",
                        content = new object[] {
                            new {
                                type = "text",
                                text = "Extract all information from the attached technical drawing according to the provided schema."
                            },
                            new {
                                type = "input_file",
                                file_id = xAiFileId
                            }
                        }
                    }
                },
                response_format = new
                {
                    type = "json_schema",
                    json_schema = new
                    {
                        name = "drawing_extraction",
                        strict = true,
                        schema = new
                        {
                            type = "object",
                            properties = new
                            {
                                part_name = new
                                {
                                    type = "string"
                                },
                                part_number = new
                                {
                                    type = "string"
                                },
                                material = new
                                {
                                    type = "string"
                                },
                                overall_dimensions = new
                                {
                                    type = "object",
                                    properties = new
                                    {
                                        length = new
                                        {
                                            type = "number"
                                        },
                                        width = new
                                        {
                                            type = "number"
                                        },
                                        height = new
                                        {
                                            type = "number"
                                        },
                                        unit = new
                                        {
                                            type = "string",
                                            @enum = new string[]{ "mm", "inch"
                                        }
                                        }
                                    },
                                    required = new string[] {
                                        "length",
                                        "width",
                                        "height",
                                        "unit"
                                    }
                                },
                                dimensions = new
                                {
                                    type = "array",
                                    items = new
                                    {
                                        type = "object",
                                        properties = new
                                        {
                                            description = new
                                            {
                                                type = "string"
                                            },
                                            nominal = new
                                            {
                                                type = "number"
                                            },
                                            tolerance = new
                                            {
                                                type = "string"
                                            },
                                            upper = new
                                            {
                                                type = "number"
                                            },
                                            lower = new
                                            {
                                                type = "number"
                                            },
                                            unit = new
                                            {
                                                type = "string"
                                            }
                                        },
                                        required = new string[]
                                        {
                                            "description",
                                            "nominal",
                                            "unit"
                                        }
                                    }
                                },
                                gdandt = new
                                {
                                    type = "array",
                                    items = new
                                    {
                                        type = "object",
                                        properties = new
                                        {
                                            feature = new
                                            {
                                                type = "string"
                                            },
                                            symbol = new
                                            {
                                                type = "string"
                                            },
                                            tolerance_value = new
                                            {
                                                type = "string"
                                            },
                                            datums = new
                                            {
                                                type = "string"
                                            },
                                            modifiers = new
                                            {
                                                type = "string"
                                            },
                                            description = new
                                            {
                                                type = "string"
                                            }
                                        },
                                        required = new string[]
                                        {
                                            "feature",
                                            "symbol",
                                            "tolerance_value"
                                        }
                                    }
                                },
                                datums = new
                                {
                                    type = "array",
                                    items = new
                                    {
                                        type = "object",
                                        properties = new
                                        {
                                            letter = new
                                            {
                                                type = "string"
                                            },
                                            description = new
                                            {
                                                type = "string"
                                            }
                                        },
                                        required = new string[] { "letter" }
                                    }
                                },
                                notes = new
                                {
                                    type = "array",
                                    items = new
                                    {
                                        type = "string"
                                    }
                                },
                                surface_finish = new
                                {
                                    type = "string"
                                },
                                general_tolerances = new
                                {
                                    type = "string"
                                },
                                confidence = new
                                {
                                    type = "number",
                                    minimum = 0,
                                    maximum = 1
                                }
                            },
                            required = new string[]
                            {
                                "part_name",
                                "material",
                                "overall_dimensions",
                                "dimensions",
                                "gdandt",
                                "datums",
                                "notes",
                                "confidence"
                            },
                            additionalProperties = false
                        }
                    }
                },
                temperature = 0.0,
                max_tokens = 8000
            }
        );

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(ct);
            throw new InvalidOperationException($"xAI analysis request failed with status code {response.StatusCode}: {errorContent}");
        }

        return await response.Content.ReadAsStringAsync(ct);
    }

    public DocumentGdAndTAnalysis ParseAnalysis(string analysisJson)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true
        };

        return JsonSerializer.Deserialize<DocumentGdAndTAnalysis>(analysisJson, options)
            ?? throw new InvalidOperationException("Failed to parse Gd&T analysis JSON.");
    }
}