using System.Diagnostics.CodeAnalysis;
using Kmic.BuildingBlocks.Application.Observability;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.BuildingBlocks.Infrastructure.Auditing;
using Mediator;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Kmic.BuildingBlocks.Infrastructure.Pipeline;

/// <summary>对标注 [AuditedRead] 的查询在成功后记录读取审计（写入失败仅记日志，不影响结果返回）</summary>
public sealed class ReadAuditBehavior<TMessage, TResponse>(
    UseCaseCatalog catalog,
    FrameworkMetrics metrics,
    // 从服务容器解析框架内部服务：由于宿主需直接引用行为类型（必须为 public），构造函数无法直接注入 internal 服务
    IServiceProvider services,
    ILogger<ReadAuditBehavior<TMessage, TResponse>> logger) : IPipelineBehavior<TMessage, TResponse>
    where TMessage : IBaseQuery
    where TResponse : Result, IResultFactory<TResponse>
{
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "写入读取审计失败时仅记录错误日志，不能影响已获取的查询结果")]
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
