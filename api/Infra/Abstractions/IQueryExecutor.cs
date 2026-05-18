namespace Infra.Abstractions;

public interface IQueryExecutor
{
    Task<IEnumerable<T>> QueryAsync<T>(string query, object? param, Func<dynamic, T> rowMapFunc, CancellationToken ct = default, int timeoutSeconds = 180);
    Task<T?> QuerySingleAsync<T>(string query, object? param, Func<dynamic, T> rowMapFunc, CancellationToken ct = default, int timeoutSeconds = 180);
    Task ExecuteAsync(string query, object? param = null, CancellationToken ct = default, int timeoutSeconds = 180);
}