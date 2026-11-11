using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.BuildingBlocks.Presentation.Tests.Support;

/// <summary>
/// 按 <see cref="SenderScript"/> 的预设应答的发送器
/// </summary>
internal sealed class FakeSender(SenderScript script, RequestMetadata metadata) : ICommandSender, IQuerySender
{
    public ValueTask<Result> SendAsync(ICommand command, CancellationToken ct) =>
        ValueTask.FromResult(script.Run<Result>(command, metadata));

    public ValueTask<Result<TResponse>> SendAsync<TResponse>(ICommand<TResponse> command, CancellationToken ct) =>
        ValueTask.FromResult(script.Run<Result<TResponse>>(command, metadata));

    public ValueTask<Result<TResponse>> SendAsync<TResponse>(IQuery<TResponse> query, CancellationToken ct) =>
        ValueTask.FromResult(script.Run<Result<TResponse>>(query, metadata));
}
