using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.BuildingBlocks.Application.Messaging;

/// <summary>
/// 包装 Mediator，并在发送命令期间标记“当前在命令内”
/// </summary>
internal sealed class Sender(Mediator.ISender mediator, ExecutionScope scope) : ISender
{
    public ValueTask<Result> SendAsync(ICommand command, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);
        return SendCommandAsync(() => mediator.Send(command, ct));
    }

    public ValueTask<Result<TResponse>> SendAsync<TResponse>(ICommand<TResponse> command, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);
        return SendCommandAsync(() => mediator.Send(command, ct));
    }

    public ValueTask<Result<TResponse>> SendAsync<TResponse>(IQuery<TResponse> query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);
        return mediator.Send(query, ct);
    }

    private async ValueTask<TResult> SendCommandAsync<TResult>(Func<ValueTask<TResult>> send)
    {
        // 一个请求 = 一个命令 = 一个事务；命令内再发命令说明代码写错了（05-02 D5）
        using var marker = scope.EnterCommand();
        return await send();
    }
}
