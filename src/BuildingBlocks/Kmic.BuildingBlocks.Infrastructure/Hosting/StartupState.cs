namespace Kmic.BuildingBlocks.Infrastructure.Hosting;

internal sealed class StartupState
{
    private readonly TaskCompletionSource _completed = new(TaskCreationOptions.RunContinuationsAsynchronously);

    public bool IsCompleted => _completed.Task.IsCompletedSuccessfully;

    public Task WaitAsync(CancellationToken ct) => _completed.Task.WaitAsync(ct);

    public void Complete() => _completed.TrySetResult();
}
