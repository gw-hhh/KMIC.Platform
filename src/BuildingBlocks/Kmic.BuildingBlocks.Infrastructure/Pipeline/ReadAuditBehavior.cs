using System.Diagnostics.CodeAnalysis;
using Kmic.BuildingBlocks.Application.Observability;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.BuildingBlocks.Infrastructure.Auditing;
using Mediator;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Kmic.BuildingBlocks.Infrastructure.Pipeline;

/// <summary>
/// 标了 [AuditedRead] 的查询成功之后，写一条审计事件
/// </summary>
/// <remarks>
/// 查询失败不记。写审计出错时查询结果照常返回，只记错误日志：已经读出来的数据不能因为记录失败而收回
/// </remarks>
public sealed class ReadAuditBehavior<TMessage, TResponse>(
    UseCaseCatalog catalog,
    FrameworkMetrics metrics,
    // 从容器取框架服务：宿主要直接引用行为类型，所以行为只能是 public，构造函数里就放不了 internal 的服务
    IServiceProvider services,
    ILogger<ReadAuditBehavior<TMessage, TResponse>> logger) : IPipelineBehavior<TMessage, TResponse>
    where TMessage : IBaseQuery
    where TResponse : Result, IResultFactory<TResponse>
{
    /// <inheritdoc />
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "写审计失败不能影响已经读出来的查询结果，只记错误日志")]
    public async ValueTask<TResponse> Handle(TMessage message, MessageHandlerDelegate<TMessage, TResponse> next, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(next);

        var response = await next(message, cancellationToken);
        if (response.IsFailure || catalog.Get(typeof(TMessage)) is not { AuditedRead: true } useCase)
        {
            return response;
        }

        try
        {
            await services.GetRequiredService<ReadAuditWriter>().WriteAsync(useCase, cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            metrics.ReadAuditFailed();
            PipelineLog.ReadAuditFailed(logger, exception, typeof(TMessage).Name);
        }

        return response;
    }
}
