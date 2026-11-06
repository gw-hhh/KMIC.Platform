namespace Spike01.BuildingBlocks;

// 包装 Mediator，业务代码不直接引用 Mediator 的类型（03 第 4.1 节）
internal sealed class Sender(Mediator.ISender mediator) : ISender
{
    public ValueTask<Result> SendAsync(ICommand command, CancellationToken cancellationToken) =>
        mediator.Send(command, cancellationToken);

    public ValueTask<Result<TResponse>> SendAsync<TResponse>(ICommand<TResponse> command, CancellationToken cancellationToken) =>
        mediator.Send(command, cancellationToken);

    public ValueTask<Result<TResponse>> SendAsync<TResponse>(IQuery<TResponse> query, CancellationToken cancellationToken) =>
        mediator.Send(query, cancellationToken);
}
