using Domain.Documents;

namespace App.Documents;

public record DocumentItem(
    Guid Id,
    DateTime CreatedOn,
    string FileName,
    string FileHash,
    uint SizeBytes
)
{
    public static DocumentItem FromDocument(Document doc) => new(
        doc.Id,
        doc.CreatedOn,
        doc.FileName,
        doc.FileHash,
        doc.SizeBytes
    );
}