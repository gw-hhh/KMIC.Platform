using Kmic.BuildingBlocks.Application.Access;
using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.Identity.Application.Authorization;

/// <summary>
/// 授权管理命令处理器的基类：先重查操作者，再执行
/// </summary>
/// <remarks>
/// Identity 里改授权的命令（角色、授予、组织单元、用户管理、模拟身份）都继承它。重查和修改在同一个事务里，
/// 不新增管道行为（模块不能增减行为），所以用基类表达
/// </remarks>
/// <typeparam name="TCommand">命令</typeparam>
/// <typeparam name="TResponse">响应</typeparam>
public abstract class AuthorizationAdminCommandHandler<TCommand, TResponse>(IOperatorRecheck recheck, IExecutionContext execution) : ICommandHandler<TCommand, TResponse>
    where TCommand : ICommand<TResponse>
{
    /// <inheritdoc />
    public async ValueTask<Result<TResponse>> Handle(TCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var access = execution.Access ?? throw new InvalidOperationException("授权管理命令需要已登录的访问上下文。");
        var rechecked = await recheck.CheckAsync(access, cancellationToken);
        return rechecked.IsFailure ? rechecked.Error : await ExecuteAsync(command, access, cancellationToken);
    }

    /// <summary>
    /// 操作者重查通过之后执行命令
    /// </summary>
    protected abstract ValueTask<Result<TResponse>> ExecuteAsync(TCommand command, IAccessContext access, CancellationToken cancellationToken);
}
