using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.BuildingBlocks.Application.Messaging;

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
        // 单次请求仅允许执行一个命令，禁止在命令内嵌套发送命令
        using var marker = scope.EnterCommand();
        return await send();
    }
}
