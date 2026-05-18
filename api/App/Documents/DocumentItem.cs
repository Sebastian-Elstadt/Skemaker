namespace App.Documents;

public record DocumentItem(
    Guid Id,
    DateTime CreatedOn,
    string FileName,
    string FileHash,
    uint SizeBytes
);