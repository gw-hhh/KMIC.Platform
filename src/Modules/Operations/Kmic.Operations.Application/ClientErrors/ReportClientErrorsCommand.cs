using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Operations.Application.ClientErrors;

/// <summary>
/// 前端批量上报错误：只写日志，不落库
/// </summary>
/// <remarks>
/// 登录就能报，包括必须先改密码的账号和只读的模拟登录会话：出错的页面恰好是它们最需要上报的。
/// 日志分类是 Kmic.ClientErrors，和后端日志进入同一个观测平台。按用户限流，超限 429，前端丢弃不重试。
/// 重复上报只是多一条日志，所以不要求幂等键
/// </remarks>
/// <param name="Errors">一批错误，条数上限见配置</param>
[AllowAuthenticated]
[AllowWhenPasswordChangeRequired]
[AllowWhenImpersonationReadOnly]
[NotAudited("前端错误日志")]
[NotIdempotent("只写日志，重复上报只是多一条日志")]
public sealed record ReportClientErrorsCommand(IReadOnlyList<ClientErrorReport> Errors) : ICommand;
