using System.Text.Json;
using App.Abstractions;
using App.Analysis;
using Domain.Documents;
using Infra.xAI;

namespace Infra.Analysis;

public class xAiGdAndTAnalyzer(
    xAiClient xAiClient,
    xAiUploadStore uploadStore,
    IFileStore fileStore
) : IGdAndTAnalyzer
{
    private async Task<string> UploadDocumentAsync(Document doc, CancellationToken ct = default)
    {
        using var fileStream = fileStore.OpenReadStream(doc.FilePath);
        string fileId = await xAiClient.UploadFileAsync(doc.FileName, fileStream, doc.ContentType, ct);
        await uploadStore.SetFileIdForDocumentIdAsync(doc.Id, fileId, ct);
        return fileId;
    }

    public async Task<string> AnalyzeDocumentAsync(Document doc, CancellationToken ct = default)
    {
        var xAiFileId = await uploadStore.GetFileIdByDocumentIdAsync(doc.Id, ct)
            ?? await UploadDocumentAsync(doc, ct);

        return await xAiClient.GenerateResponseAsync(
            inputs: [
                new {
                    role = "system",
                    content = "You are an expert mechanical engineer and GD&T specialist per ASME Y14.5. Extract all information from the attached technical drawing."
                },
                new {
                    role = "user",
                    content = new object[] {
                        new {
                            type = "input_text",
                            text = """
                            Extract ALL technical information from the attached technical drawing into the exact JSON schema.

                            CRITICAL REQUIREMENTS:
                            - For every feature (hole, slot, circle, rectangle, etc.), provide accurate X/Y coordinates relative to the primary datums (usually A|B|C).
                            - If the drawing shows basic dimensions or coordinate callouts, calculate and include the real position values.
                            - Pay special attention to datum references and feature relationships.
                            - Set recommended_manufacturing_method correctly (e.g., 'laser-cut contour', 'mill', 'waterjet', etc.).
                            - Be precise — the downstream G-code generator will ONLY use the coordinates you provide.

                            Output ONLY valid JSON matching the schema. No extra text.
                            """
                        },
                        new {
                            type = "input_file",
                            file_id = xAiFileId
                        }
                    }
                }
            ],
            formatOptions: new
            {
                type = "json_schema",
                name = "drawing_extraction",
                strict = true,
                schema = new
                {
                    type = "object",
                    properties = new
                    {
                        part_name = new { type = "string" },
                        part_number = new { type = "string" },
                        material = new { type = "string" },

                        overall_dimensions = new
                        {
                            type = "object",
                            properties = new
                            {
                                length = new { type = "number" },
                                width = new { type = "number" },
                                height = new { type = "number" },
                                unit = new { type = "string", @enum = new[] { "mm", "inch" } }
                            },
                            required = new[] { "length", "width", "height", "unit" }
                        },

                        features = new
                        {
                            type = "array",
                            items = new
                            {
                                type = "object",
                                properties = new
                                {
                                    feature_id = new { type = "string" },
                                    type = new { type = "string", @enum = new[] { "hole", "slot", "circle", "rectangle", "profile", "boss", "pocket", "other" } },
                                    description = new { type = "string" },
                                    nominal_diameter = new { type = "number" },
                                    nominal_width = new { type = "number" },
                                    nominal_length = new { type = "number" },
                                    position = new
                                    {
                                        type = "object",
                                        properties = new
                                        {
                                            x = new { type = "number" },
                                            y = new { type = "number" },
                                            z = new { type = "number" },
                                            coordinate_system = new { type = "string" }
                                        },
                                        required = new[] { "x", "y" }
                                    },
                                    tolerance = new { type = "string" },
                                    gdandt_references = new
                                    {
                                        type = "array",
                                        items = new { type = "string" }
                                    },
                                    basic_dimensions = new
                                    {
                                        type = "array",
                                        items = new
                                        {
                                            type = "object",
                                            properties = new
                                            {
                                                description = new { type = "string" },
                                                value = new { type = "number" },
                                                from_datum = new { type = "string" }
                                            }
                                        }
                                    }
                                },
                                required = new[] { "feature_id", "type", "description", "position", "basic_dimensions" }
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
                                    description = new { type = "string" },
                                    nominal = new { type = "number" },
                                    tolerance = new { type = "string" },
                                    upper = new { type = "number" },
                                    lower = new { type = "number" },
                                    unit = new { type = "string" }
                                },
                                required = new[] { "description", "nominal", "unit" }
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
                                    feature = new { type = "string" },
                                    symbol = new { type = "string" },
                                    tolerance_value = new { type = "string" },
                                    datums = new { type = "string" },
                                    modifiers = new { type = "string" },
                                    description = new { type = "string" },
                                    affected_features = new { type = "array", items = new { type = "string" } }
                                },
                                required = new[] { "feature", "symbol", "tolerance_value", "datums" }
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
                                    letter = new { type = "string" },
                                    description = new { type = "string" },
                                    type = new { type = "string" }
                                },
                                required = new[] { "letter", "description" }
                            }
                        },

                        notes = new
                        {
                            type = "array",
                            items = new { type = "string" }
                        },

                        surface_finish = new { type = "string" },
                        general_tolerances = new { type = "string" },
                        recommended_manufacturing_method = new { type = "string" },

                        confidence = new
                        {
                            type = "number",
                            minimum = 0,
                            maximum = 1
                        }
                    },
                    required = new[]
                    {
                        "part_name", "material", "overall_dimensions", "features",
                        "dimensions", "gdandt", "datums", "notes", "confidence"
                    },
                    additionalProperties = false
                }
            },
            maxOutputTokens: 12000,
            cancellationToken: ct
        );
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