using Npgsql;
using App.Abstractions;
using Infra.Repositories;

namespace Infra.RecordStore;

public class PostgresRecordStore : PostgresQueryExecutor, IRecordStore, IAsyncDisposable
{
    public PostgresRecordStore(RecordStoreConfig config) : base(new NpgsqlConnection(config.ConnectionString)) { }

    #region Transaction

    public async Task BeginTransactionAsync(CancellationToken ct = default)
    {
        if (transaction is { }) return;
        transaction = await connection.BeginTransactionAsync(ct);
    }

    public async Task CommitTransactionAsync(CancellationToken ct = default)
    {
        if (transaction is null) return;
        await transaction.CommitAsync(ct);
        await transaction.DisposeAsync();
        transaction = null;
    }

    public async Task RollbackTransactionAsync(CancellationToken ct = default)
    {
        if (transaction is null) return;
        await transaction.RollbackAsync(ct);
        await transaction.DisposeAsync();
        transaction = null;
    }

    public async ValueTask DisposeAsync()
    {
        if (transaction is not null)
        {
            await transaction.DisposeAsync();
        }

        await connection.DisposeAsync();
    }

    #endregion

    #region Repositories

    private IDocumentRepository? _documentRepository = null;
    public IDocumentRepository DocumentRepository { get => _documentRepository ??= new DocumentRepository(this); }

    private IDocumentAnalysisRepository? _documentAnalysisRepository = null;
    public IDocumentAnalysisRepository DocumentAnalysisRepository { get => _documentAnalysisRepository ??= new DocumentAnalysisRepository(this); }

    #endregion
}