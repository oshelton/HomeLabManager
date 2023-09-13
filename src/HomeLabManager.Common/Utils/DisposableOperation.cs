namespace HomeLabManager.Common.Utils;

/// <summary>
/// Simple utility class for disposable operations.
/// </summary>
public sealed class DisposableOperation: IDisposable
{
    /// <summary>
    /// Construct the operation.
    /// </summary>
    /// <param name="onStart">Action to invoke when starting the operation (may be null).</param>
    /// <param name="onDispose">Action to invoke when disposing the object; should not be null.</param>
    public DisposableOperation(Action onStart, Action onDispose)
    {
        onStart?.Invoke();

        _onDispose = onDispose ?? throw new ArgumentNullException(nameof(onDispose));
    }

    public void Dispose() => _onDispose();

    private readonly Action _onDispose;
}
