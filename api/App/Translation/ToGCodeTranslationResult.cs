namespace App.Translation;

public record ToGCodeTranslationResult(
    string FullResult,
    string StrategySummary,
    string GCode
);