using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Security;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.BuildingBlocks.Domain.Errors;
using Kmic.BuildingBlocks.Domain.Results;
using Mediator;

namespace Kmic.BuildingBlocks.Application.Behaviors;

/// <summary>
/// 权限点检查
/// </summary>
/// <remarks>
/// 数据权限、资源策略和状态检查在处理器里做
/// </remarks>
public sealed class AuthorizationBehavior<TMessage, TResponse>(
    UseCaseCatalog catalog,
    IExecutionContext context,
    ISecurityEventWriter securityEvents) : IPipelineBehavior<TMessage, TResponse>
    where TMessage : IMessage
    where TResponse : Result, IResultFactory<TResponse>
{
    /// <inheritdoc />
    public ValueTask<TResponse> Handle(TMessage message, MessageHandlerDelegate<TMessage, TResponse> next, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(next);

        var useCase = catalog.Get(typeof(TMessage));
        var denied = Check(useCase);
        return denied is null ? next(message, cancellationToken) : ValueTask.FromResult(TResponse.Fail(denied));
    }

    private Error? Check(UseCase useCase)
    {
        if (useCase.Requirement == AccessRequirement.ContractQuery)
        {
            return CheckContractQuery(useCase);
        }

        var user = context.User;
        if (user.IsSystem)
        {
            // 系统身份执行了只给用户用的用例，是代码写错了，比如定时任务发错了命令
            return useCase.AllowSystem || useCase.Requirement == AccessRequirement.SystemOnly
                ? null
                : throw new InvalidOperationException($"{useCase.Name} 没有声明 [AllowSystem]，不能以系统身份执行。");
        }

        if (useCase.Requirement == AccessRequirement.SystemOnly)
        {
            return Deny(useCase, CommonErrors.MissingPermission, "SystemOnly");
        }

        if (useCase.Requirement == AccessRequirement.Anonymous)
        {
            return null;
        }

        // 认证中间件已经拒绝了匿名请求，这里是兜底。ErrorType 没有 401，按无权限处理
        var access = context.Access;
        if (access is null)
        {
            return Deny(useCase, CommonErrors.MissingPermission, "Unauthenticated");
        }

        if (access.PasswordChangeRequired && !useCase.AllowWhenPasswordChangeRequired)
        {
            return CommonErrors.PasswordChangeRequired;
        }

        if (useCase.Kind == MessageKind.Command && access.ImpersonatorUserId is not null && !access.ImpersonationAllowsWrite)
        {
            return Deny(useCase, CommonErrors.ImpersonationReadOnly, "ImpersonationReadOnly");
        }

        if (useCase.Requirement == AccessRequirement.Authenticated || useCase.Permission is { } permission && access.Permissions.Contains(permission))
        {
            return null;
        }

        return Deny(useCase, CommonErrors.MissingPermission, "MissingPermission");
    }

    private Error? CheckContractQuery(UseCase useCase)
    {
        if (context.Origin != CallOrigin.ModuleCall)
        {
            // 端点不能绑定 Contracts 查询，HTTP 请求走到这里按无权限处理，其他来源是代码写错了
            return context.Origin == CallOrigin.Http
                ? Deny(useCase, CommonErrors.MissingPermission, "ContractQueryOutsideModuleCall")
                : throw new InvalidOperationException($"{useCase.Name} 是 Contracts 查询，只能经 IContractQuerySender 调用。");
        }

        if (useCase.ContractAccess == ContractAccess.SystemOnly && !context.User.IsSystem)
        {
            return Deny(useCase, CommonErrors.MissingPermission, "SystemOnly");
        }

        return null;
    }

    private Error Deny(UseCase useCase, Error error, string reason)
    {
        securityEvents.Write(new SecurityEvent("AuthorizationDenied", SecuritySeverity.Warning, reason)
        {
            Details = new Dictionary<string, string>(StringComparer.Ordinal) { ["useCase"] = useCase.Name },
        });
        return error;
    }
}
