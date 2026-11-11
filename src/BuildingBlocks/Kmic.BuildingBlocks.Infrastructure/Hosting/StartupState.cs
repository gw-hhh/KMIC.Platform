namespace Kmic.BuildingBlocks.Infrastructure.Hosting;

/// <summary>
/// 启动任务是否已全部完成
/// </summary>
/// <remarks>
/// 后台组件先等它，例如订阅路由登记完成之前不能开始投递
/// </remarks>
internal sealed class StartupState
{
    private readonly TaskCompletionSource _completed = new(TaskCreationOptions.RunContinuationsAsynchronously);

    public bool IsCompleted => _completed.Task.IsCompletedSuccessfully;

    public Task WaitAsync(CancellationToken ct) => _completed.Task.WaitAsync(ct);

    public void Complete() => _completed.TrySetResult();
}
