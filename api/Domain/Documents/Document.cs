namespace Domain.Documents;

public class Document
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public DateTime CreatedOn { get; private set; } = DateTime.UtcNow;

    private string _fileName = string.Empty;
    public string FileName
    {
        get => _fileName;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("File name cannot be null or whitespace.", nameof(value));

            if (value.Length > 255)
                throw new ArgumentException("File name cannot exceed 255 characters.", nameof(value));

            if (value.Intersect(Path.GetInvalidFileNameChars()).Any())
                throw new ArgumentException("File name contains invalid characters.", nameof(value));

            if (value.Contains("..") || Path.IsPathRooted(value))
                throw new ArgumentException("File name must not contain path components.", nameof(value));

            if (string.IsNullOrEmpty(Path.GetExtension(value)))
                throw new ArgumentException("File name must have an extension.", nameof(value));

            _fileName = value.Trim();
        }
    }

    private string _filePath = string.Empty;
    public string FilePath
    {
        get => _filePath;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("File path cannot be null or whitespace.", nameof(value));

            if (value.Length > 4096)
                throw new ArgumentException("File path is too long.", nameof(value));

            if (!Path.IsPathFullyQualified(value))
                throw new ArgumentException("File path must be an absolute path.", nameof(value));

            if (value.Contains(".."))
                throw new ArgumentException("File path must not contain path traversal sequences.", nameof(value));

            if (value.Intersect(Path.GetInvalidPathChars()).Any())
                throw new ArgumentException("File path contains invalid characters.", nameof(value));

            _filePath = value.Trim();
        }
    }

    public Document(string name, string path)
    {
        FileName = name;
        FilePath = path;
    }
}