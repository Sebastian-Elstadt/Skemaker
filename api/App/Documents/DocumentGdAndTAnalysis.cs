using System.Text.Json.Serialization;

namespace App.Documents;

public record DocumentGdAndTAnalysis
{
    [JsonPropertyName("part_name")]
    public required string PartName { get; init; }

    [JsonPropertyName("part_number")]
    public string? PartNumber { get; init; }

    [JsonPropertyName("material")]
    public required string Material { get; init; }

    [JsonPropertyName("overall_dimensions")]
    public required OverallDimensions OverallDimensions { get; init; }

    [JsonPropertyName("dimensions")]
    public required List<DimensionEntry> Dimensions { get; init; }

    [JsonPropertyName("gdandt")]
    public required List<GdAndTEntry> GdAndT { get; init; }

    [JsonPropertyName("datums")]
    public required List<DatumEntry> Datums { get; init; }

    [JsonPropertyName("notes")]
    public required List<string> Notes { get; init; }

    [JsonPropertyName("surface_finish")]
    public string? SurfaceFinish { get; init; }

    [JsonPropertyName("general_tolerances")]
    public string? GeneralTolerances { get; init; }

    [JsonPropertyName("confidence")]
    public required double Confidence { get; init; }
}

public record OverallDimensions
{
    [JsonPropertyName("length")]
    public required double Length { get; init; }

    [JsonPropertyName("width")]
    public required double Width { get; init; }

    [JsonPropertyName("height")]
    public required double Height { get; init; }

    [JsonPropertyName("unit")]
    public required string Unit { get; init; }
}

public record DimensionEntry
{
    [JsonPropertyName("description")]
    public required string Description { get; init; }

    [JsonPropertyName("nominal")]
    public required double Nominal { get; init; }

    [JsonPropertyName("tolerance")]
    public string? Tolerance { get; init; }

    [JsonPropertyName("upper")]
    public double? Upper { get; init; }

    [JsonPropertyName("lower")]
    public double? Lower { get; init; }

    [JsonPropertyName("unit")]
    public required string Unit { get; init; }
}

public record GdAndTEntry
{
    [JsonPropertyName("feature")]
    public required string Feature { get; init; }

    [JsonPropertyName("symbol")]
    public required string Symbol { get; init; }

    [JsonPropertyName("tolerance_value")]
    public required string ToleranceValue { get; init; }

    [JsonPropertyName("datums")]
    public string? Datums { get; init; }

    [JsonPropertyName("modifiers")]
    public string? Modifiers { get; init; }

    [JsonPropertyName("description")]
    public string? Description { get; init; }
}

public record DatumEntry
{
    [JsonPropertyName("letter")]
    public required string Letter { get; init; }

    [JsonPropertyName("description")]
    public string? Description { get; init; }
}