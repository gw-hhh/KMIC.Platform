using Kmic.BuildingBlocks.Application.Access;
using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.Identity.Application.Authorization;

/// <summary>
/// 授权管理命令的基类，执行前重新校验操作者
/// </summary>
/// <typeparam name="TCommand">命令</typeparam>
/// <typeparam name="TResponse">响应</typeparam>
public abstract class AuthorizationAdminCommandHandler<TCommand, TResponse>(IOperatorRecheck recheck, IExecutionContext execution) : ICommandHandler<TCommand, TResponse>
    where TCommand : ICommand<TResponse>
{
    /// <inheritdoc />
    public async ValueTask<Result<TResponse>> Handle(TCommand command, CancellationToken cancellationToken)
    {
        var access = execution.Access ?? throw new InvalidOperationException("授权管理命令需要已登录的访问上下文。");
        var rechecked = await recheck.CheckAsync(access, cancellationToken);
        return rechecked.IsFailure ? rechecked.Error : await ExecuteAsync(command, access, cancellationToken);
    }

    /// <summary>
    /// 操作者重查通过之后执行命令
    /// </summary>
    protected abstract ValueTask<Result<TResponse>> ExecuteAsync(TCommand command, IAccessContext access, CancellationToken cancellationToken);
}
