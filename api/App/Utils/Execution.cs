namespace App.Utils;

public static class ExecutionUtils
{
    public static async Task GracefullyFailAsync(Func<Task> func, Action<Exception>? onFailure = null)
    {
        try
        {
            await func();
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            onFailure?.Invoke(ex);
        }
    }

    public static async void GracefullyFail(Action func, Action<Exception>? onFailure = null)
    {
        try
        {
            func();
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            onFailure?.Invoke(ex);
        }
    }
}