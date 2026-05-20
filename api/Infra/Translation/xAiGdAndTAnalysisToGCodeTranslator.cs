using System.Text.Json;
using App.Abstractions;
using App.Translation;
using Domain.Analysis;
using Infra.xAI;

namespace Infra.Translation;

public class xAiGdAndTAnalysisToGCodeTranslator(
    xAiClient xAiClient,
    xAiUploadStore uploadStore
) : IGdAndTAnalysisToGCodeTranslator
{
    private record GCodeResponse
    {
        public string StrategySummary { get; set; } = string.Empty;
        public string ToolList { get; set; } = string.Empty;
        public string GCode { get; set; } = string.Empty;
    }

    public async Task<GCodeTranslation> TranslateAsync(DocumentAnalysis analysis, GCodeManufacturingOptions options, CancellationToken ct = default)
    {
        if (analysis.Type != DocumentAnalysisType.GdAndT)
            throw new ArgumentException($"Expected DocumentAnalysisType.GdAndT but got {analysis.Type}");

        string xAiFileId = await uploadStore.GetFileIdByDocumentIdAsync(analysis.DocumentId, ct)
            ?? throw new InvalidOperationException($"No xAI file ID found for document ID {analysis.DocumentId}. Cannot proceed with G-code translation without reference to original drawing.");

        string responseString = await xAiClient.GenerateResponseAsync(
            inputs: [
                new {
                    role = "system",
                    content = "You are an expert CNC programmer and CAM engineer. Generate safe, efficient, and well-commented G-code."
                },
                new {
                    role = "user",
                    content = new object[] {
                        new {
                            type = "input_text",
                            text = $"""
                            Part specification from drawing analysis:
                            {PrettyPrintJson(analysis.AnalysisJson)}

                            Manufacturing configuration:
                            {SerializeToPrettyJson(options)}

                            Original drawing file_id (for visual reference): {xAiFileId}

                            Generate a complete G-code program that:
                            - Uses only the tools listed above
                            - Respects all GD&T tolerances where practically possible
                            - Includes roughing and finishing passes where appropriate
                            - Uses proper feeds & speeds for the material
                            - Has safe retracts at the specified SafeZHeight
                            - Uses the requested work offset

                            Output **strictly** following the JSON schema.
                            - strategySummary: Brief manufacturing strategy and notes
                            - toolList: Markdown list of tools used with parameters
                            - gcode: The complete, clean G-code block only (no markdown outside the string)
                            """
                            // Output format:
                            // 1. Brief Strategy Summary
                            // 2. Tool List Used
                            // 3. Full G-code with clear comments and section headers (G-code only in one block)
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
                name = "gcode_generation",
                strict = true,
                schema = new
                {
                    type = "object",
                    properties = new
                    {
                        strategySummary = new { type = "string" },
                        toolList = new { type = "string" },
                        gcode = new { type = "string" }
                    },
                    required = new[] { "strategySummary", "toolList", "gcode" },
                    additionalProperties = false
                }
            },
            maxOutputTokens: 16000
        );

        if (string.IsNullOrWhiteSpace(responseString))
            throw new InvalidOperationException("Received empty response from xAI. Cannot proceed with G-code translation.");

        var parsed = JsonSerializer.Deserialize<GCodeResponse>(responseString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        if (parsed is null)
            throw new InvalidOperationException("Failed to parse xAI response into expected GCodeResponse structure. Response content: " + responseString);

        return new(responseString, parsed.StrategySummary, parsed.GCode, parsed.ToolList);
    }

    private string PrettyPrintJson(string minifiedJson)
    {
        if (string.IsNullOrWhiteSpace(minifiedJson))
            return minifiedJson;

        using JsonDocument document = JsonDocument.Parse(minifiedJson);

        var options = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        return JsonSerializer.Serialize(document.RootElement, options);
    }

    private string SerializeToPrettyJson(object data)
        => JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
}