using Npgsql;
using Dapper;
using Infra.Abstractions;

namespace Infra.RecordStore;

public abstract class PostgresQueryExecutor : IQueryExecutor
{
    protected readonly NpgsqlConnection connection;
    protected NpgsqlTransaction? transaction = null;

    public PostgresQueryExecutor(NpgsqlConnection conn)
    {
        connection = conn;
    }

    public Task<IEnumerable<T>> QueryAsync<T>(string query, object? param = null, CancellationToken ct = default, int timeoutSeconds = 180)
    {
        var cmd = new CommandDefinition(
            query,
            parameters: param,
            transaction: transaction,
            commandTimeout: timeoutSeconds,
            commandType: System.Data.CommandType.Text,
            cancellationToken: ct
        );

        return connection.QueryAsync<T>(cmd);
    }

    public Task<T?> QuerySingleAsync<T>(string query, object? param = null, CancellationToken ct = default, int timeoutSeconds = 180)
    {
        var cmd = new CommandDefinition(
            query,
            parameters: param,
            transaction: transaction,
            commandTimeout: timeoutSeconds,
            commandType: System.Data.CommandType.Text,
            cancellationToken: ct
        );

        return connection.QuerySingleOrDefaultAsync<T>(cmd);
    }

    public Task ExecuteAsync(string query, object? param = null, CancellationToken ct = default, int timeoutSeconds = 180)
    {
        var cmd = new CommandDefinition(
            query,
            parameters: param,
            transaction: transaction,
            commandTimeout: timeoutSeconds,
            commandType: System.Data.CommandType.Text,
            cancellationToken: ct
        );

        return connection.ExecuteAsync(cmd);
    }
}