using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.BuildingBlocks.Application.Messaging;

/// <summary>命令发送器；命令处理器不得注入此接口，禁止在命令执行期间嵌套发送命令。</summary>
public interface ICommandSender
{
    ValueTask<Result> SendAsync(ICommand command, CancellationToken ct);

    ValueTask<Result<TResponse>> SendAsync<TResponse>(ICommand<TResponse> command, CancellationToken ct);
}
