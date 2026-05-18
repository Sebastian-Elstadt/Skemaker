using Npgsql;
using Dapper;
using Infra.Abstractions;
using System.Data;

namespace Infra.RecordStore;

public abstract class PostgresQueryExecutor : IQueryExecutor
{
    protected readonly NpgsqlConnection connection;
    protected NpgsqlTransaction? transaction = null;

    public PostgresQueryExecutor(NpgsqlConnection conn)
    {
        connection = conn;
    }

    public async Task<IEnumerable<T>> QueryAsync<T>(string query, object? param, Func<dynamic, T> rowMapFunc, CancellationToken ct = default, int timeoutSeconds = 180)
    {
        var cmd = new CommandDefinition(
            query,
            parameters: param,
            transaction: transaction,
            commandTimeout: timeoutSeconds,
            commandType: CommandType.Text,
            cancellationToken: ct
        );

        var rows = await connection.QueryAsync<T>(cmd);
        return rows.Select(row => rowMapFunc(row!));
    }

    public async Task<T?> QuerySingleAsync<T>(string query, object? param, Func<dynamic, T> rowMapFunc, CancellationToken ct = default, int timeoutSeconds = 180)
    {
        var cmd = new CommandDefinition(
            query,
            parameters: param,
            transaction: transaction,
            commandTimeout: timeoutSeconds,
            commandType: CommandType.Text,
            cancellationToken: ct
        );

        var row = await connection.QuerySingleOrDefaultAsync(cmd);
        if (row == null)
            return default;
        
        return rowMapFunc(row);
    }

    public Task ExecuteAsync(string query, object? param = null, CancellationToken ct = default, int timeoutSeconds = 180)
    {
        var cmd = new CommandDefinition(
            query,
            parameters: param,
            transaction: transaction,
            commandTimeout: timeoutSeconds,
            commandType: CommandType.Text,
            cancellationToken: ct
        );

        return connection.ExecuteAsync(cmd);
    }
}