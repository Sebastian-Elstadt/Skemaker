namespace Domain.Documents;

public class DocumentAnalysis
{
    public Guid Id { get; private init; } = Guid.NewGuid();
    public DateTime CreatedOn { get; private init; } = DateTime.UtcNow;

    public Guid DocumentId { get; init; }
    public DocumentAnalysisType Type { get; set; }

    private string _analysisJson = string.Empty;
    public string AnalysisJson
    {
        get => _analysisJson;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Analysis JSON cannot be null or whitespace.");
            _analysisJson = value.Trim();
        }
    }

    private DocumentAnalysis() { }
    public DocumentAnalysis(Guid docId, DocumentAnalysisType type, string analysisJson)
    {
        if (docId == Guid.Empty)
            throw new ArgumentException("Document ID cannot be empty.");

        DocumentId = docId;
        Type = type;
        AnalysisJson = analysisJson;
    }

    public static DocumentAnalysis Reconstitute(
        Guid id,
        DateTime createdOn,
        Guid documentId,
        DocumentAnalysisType type,
        string analysisJson
    ) => new DocumentAnalysis
    {
        Id = id,
        CreatedOn = createdOn,
        DocumentId = documentId,
        Type = type,
        _analysisJson = analysisJson
    };
}