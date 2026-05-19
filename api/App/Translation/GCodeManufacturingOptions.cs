namespace App.Translation;

public record GCodeManufacturingOptions
{
    public required string MachineType { get; set; }
    public string? Material { get; set; }

    public required StockSizeData StockSize { get; set; }
    public List<ToolDefinitionData> Tools { get; set; } = new();

    public required string WorkOffset { get; set; }
    public required string WorkOffsetLocation { get; set; }
    public required string OperationStrategy { get; set; }
    public required string Coolant { get; set; }
    public required double SafeZHeight { get; set; }
    public string? AdditionalNotes { get; set; }

    public class StockSizeData
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
        public required string Unit { get; set; }
    }

    public class ToolDefinitionData
    {
        public required string Name { get; set; }
        public double Diameter { get; set; }
        public required int Flutes { get; set; }
        public required string Material { get; set; }
    }
}