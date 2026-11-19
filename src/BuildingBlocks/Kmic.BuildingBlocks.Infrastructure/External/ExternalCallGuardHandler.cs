using Kmic.BuildingBlocks.Application.Execution;

namespace Kmic.BuildingBlocks.Infrastructure.External;

/// <summary>
/// 命令里不能调用外部系统
/// </summary>
internal sealed class ExternalCallGuardHandler : DelegatingHandler
{
    protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        EnsureOutsideCommand(request);
        return base.Send(request, cancellationToken);
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        EnsureOutsideCommand(request);
        return base.SendAsync(request, cancellationToken);
    }

    private static void EnsureOutsideCommand(HttpRequestMessage request)
    {
        if (ExecutionScope.CurrentlyInCommand)
        {
            throw new InvalidOperationException(
                $"命令里不能调用外部系统（{request.Method} {request.RequestUri?.Host}）。在命令里写待办，由后台作业或投递程序在事务之外调用。");
        }
    }
}
