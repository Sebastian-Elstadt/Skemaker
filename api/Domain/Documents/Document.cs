namespace Domain.Documents;

public class Document
{
    public Guid Id { get; private init; } = Guid.NewGuid();
    public DateTime CreatedOn { get; private init; } = DateTime.UtcNow;

    public uint SizeBytes { get; set; }

    private string _contentType = string.Empty;
    public string ContentType
    {
        get => _contentType;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Content type cannot be null or whitespace.");
            _contentType = value.Trim();
        }
    }

    private string _fileName = string.Empty;
    public string FileName
    {
        get => _fileName;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("File name cannot be null or whitespace.");

            if (value.Length > 255)
                throw new ArgumentException("File name cannot exceed 255 characters.");

            if (value.Intersect(Path.GetInvalidFileNameChars()).Any())
                throw new ArgumentException("File name contains invalid characters.");

            if (value.Contains("..") || Path.IsPathRooted(value))
                throw new ArgumentException("File name must not contain path components.");

            if (string.IsNullOrEmpty(Path.GetExtension(value)))
                throw new ArgumentException("File name must have an extension.");

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
                throw new ArgumentException("File path cannot be null or whitespace.");

            if (value.Length > 4096)
                throw new ArgumentException("File path is too long.");

            if (value.Contains(".."))
                throw new ArgumentException("File path must not contain path traversal sequences.");

            if (value.Intersect(Path.GetInvalidPathChars()).Any())
                throw new ArgumentException("File path contains invalid characters.");

            _filePath = value.Trim();
        }
    }

    private string _fileHash = string.Empty;
    public string FileHash
    {
        get => _fileHash;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("File hash cannot be null or whitespace.");

            if (value.Length > 256)
                throw new ArgumentException("File hash is too long.");

            _fileHash = value.Trim();
        }
    }

    private Document() { }
    public Document(string name, string path, string contentType, string hash, uint sizeBytes)
    {
        FileName = name;
        FilePath = path;
        FileHash = hash;
        SizeBytes = sizeBytes;
        ContentType = contentType;
    }

    public static Document Reconstitute(
        Guid id,
        DateTime createdOn,
        string fileName,
        string filePath,
        string fileHash,
        uint sizeBytes,
        string contentType
    ) => new Document
    {
        Id = id,
        CreatedOn = createdOn,
        _fileName = fileName,
        _filePath = filePath,
        _fileHash = fileHash,
        SizeBytes = sizeBytes,
        _contentType = contentType
    };
}