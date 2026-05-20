using System.Text.Json.Serialization;

namespace App.Analysis;

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

    [JsonPropertyName("features")]
    public required List<FeatureEntry> Features { get; init; }

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

    [JsonPropertyName("recommended_manufacturing_method")]
    public string? RecommendedManufacturingMethod { get; init; }

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

public record FeatureEntry
{
    [JsonPropertyName("feature_id")]
    public required string FeatureId { get; init; }

    [JsonPropertyName("type")]
    public required string Type { get; init; }

    [JsonPropertyName("description")]
    public required string Description { get; init; }

    [JsonPropertyName("nominal_diameter")]
    public double? NominalDiameter { get; init; }

    [JsonPropertyName("nominal_width")]
    public double? NominalWidth { get; init; }

    [JsonPropertyName("nominal_length")]
    public double? NominalLength { get; init; }

    [JsonPropertyName("position")]
    public required FeaturePosition Position { get; init; }

    [JsonPropertyName("tolerance")]
    public string? Tolerance { get; init; }

    [JsonPropertyName("gdandt_references")]
    public List<string>? GdAndTReferences { get; init; }

    [JsonPropertyName("basic_dimensions")]
    public List<BasicDimension>? BasicDimensions { get; init; }
}

public record BasicDimension
{
    [JsonPropertyName("description")]
    public required string Description { get; init; }
    [JsonPropertyName("value")]
    public required double Value { get; init; }
    [JsonPropertyName("from_datum")]
    public required string FromDatum { get; init; }
}

public record FeaturePosition
{
    [JsonPropertyName("x")]
    public required double X { get; init; }

    [JsonPropertyName("y")]
    public required double Y { get; init; }

    [JsonPropertyName("z")]
    public double? Z { get; init; }

    [JsonPropertyName("coordinate_system")]
    public string? CoordinateSystem { get; init; }
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
    public required string Datums { get; init; }

    [JsonPropertyName("modifiers")]
    public string? Modifiers { get; init; }

    [JsonPropertyName("description")]
    public string? Description { get; init; }

    [JsonPropertyName("affected_features")]
    public List<string>? AffectedFeatures { get; init; }
}

public record DatumEntry
{
    [JsonPropertyName("letter")]
    public required string Letter { get; init; }

    [JsonPropertyName("description")]
    public required string Description { get; init; }

    [JsonPropertyName("type")]
    public string? Type { get; init; }
}