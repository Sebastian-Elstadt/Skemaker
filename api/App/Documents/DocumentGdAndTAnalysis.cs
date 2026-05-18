using System.Text.Json.Serialization;

namespace App.Documents;

public record DocumentGdAndTAnalysis
{
    [JsonPropertyName("part_name")]
    public string PartName { get; init; } = string.Empty;

    [JsonPropertyName("part_number")]
    public string? PartNumber { get; init; }

    [JsonPropertyName("material")]
    public string Material { get; init; } = string.Empty;

    [JsonPropertyName("overall_dimensions")]
    public OverallDimensions OverallDimensions { get; init; } = new();

    [JsonPropertyName("dimensions")]
    public List<Dimension> Dimensions { get; init; } = new();

    [JsonPropertyName("gdandt")]
    public List<GdAndT> GdAndT { get; init; } = new();

    [JsonPropertyName("datums")]
    public List<Datum> Datums { get; init; } = new();

    [JsonPropertyName("notes")]
    public List<string> Notes { get; init; } = new();

    [JsonPropertyName("surface_finish")]
    public string? SurfaceFinish { get; init; }

    [JsonPropertyName("general_tolerances")]
    public string? GeneralTolerances { get; init; }

    [JsonPropertyName("confidence")]
    public double Confidence { get; init; } = 0.0;
}

public record OverallDimensions
{
    [JsonPropertyName("length")]
    public double Length { get; init; }

    [JsonPropertyName("width")]
    public double Width { get; init; }

    [JsonPropertyName("height")]
    public double Height { get; init; }

    [JsonPropertyName("unit")]
    public string Unit { get; init; } = "mm"; // "mm" or "inch"
}

public record Dimension
{
    [JsonPropertyName("description")]
    public string Description { get; init; } = string.Empty;

    [JsonPropertyName("nominal")]
    public double Nominal { get; init; }

    [JsonPropertyName("tolerance")]
    public string? Tolerance { get; init; } // e.g. "±0.1" or "0.05/0.00"

    [JsonPropertyName("upper")]
    public double? Upper { get; init; }

    [JsonPropertyName("lower")]
    public double? Lower { get; init; }

    [JsonPropertyName("unit")]
    public string Unit { get; init; } = "mm";
}

public record GdAndT
{
    [JsonPropertyName("feature")]
    public string Feature { get; init; } = string.Empty; // e.g. "Hole", "Surface A", "Boss"

    [JsonPropertyName("symbol")]
    public string Symbol { get; init; } = string.Empty; // e.g. "⌀", "▱", "⛢", "//"

    [JsonPropertyName("tolerance_value")]
    public string ToleranceValue { get; init; } = string.Empty; // e.g. "0.02", "0.05 Ⓜ"

    [JsonPropertyName("datums")]
    public string? Datums { get; init; } // e.g. "A-B", "A"

    [JsonPropertyName("modifiers")]
    public string? Modifiers { get; init; } // e.g. "Ⓜ", "Ⓛ", "Ⓢ"

    [JsonPropertyName("description")]
    public string? Description { get; init; }
}

public record Datum
{
    [JsonPropertyName("letter")]
    public string Letter { get; init; } = string.Empty; // A, B, C...

    [JsonPropertyName("description")]
    public string? Description { get; init; }
}