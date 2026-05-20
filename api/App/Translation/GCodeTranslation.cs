namespace App.Translation;

public record GCodeTranslation(
    string FullResult,
    string StrategySummary,
    string GCode,
    string ToolList
);