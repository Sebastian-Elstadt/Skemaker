namespace App.Documents;

public record DocumentFile(
    Stream Stream,
    string FileName,
    string ContentType
);