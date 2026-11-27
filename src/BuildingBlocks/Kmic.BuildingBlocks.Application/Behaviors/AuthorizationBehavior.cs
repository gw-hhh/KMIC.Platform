using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Observability;
using Kmic.BuildingBlocks.Application.Security;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.BuildingBlocks.Domain.Errors;
using Kmic.BuildingBlocks.Domain.Results;
using Mediator;

namespace Kmic.BuildingBlocks.Application.Behaviors;

/// <summary>用例访问与权限点校验管道；数据权限与资源策略由处理器内部自行校验。</summary>
public sealed class AuthorizationBehavior<TMessage, TResponse>(
    UseCaseCatalog catalog,
    IExecutionContext context,
    ISecurityEventWriter securityEvents,
    FrameworkMetrics metrics) : IPipelineBehavior<TMessage, TResponse>
    where TMessage : IMessage
    where TResponse : Result, IResultFactory<TResponse>
{
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
            // 系统身份调用了仅限用户的用例，属于编码错误
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

        // 认证中间件已拦截匿名请求，此处作为兜底按无权限处理
        var access = context.Access;
        if (access is null)
        {
            return Deny(useCase, CommonErrors.MissingPermission, "Unauthenticated");
        }

        if (access.PasswordChangeRequired && !useCase.AllowWhenPasswordChangeRequired)
        {
            return CommonErrors.PasswordChangeRequired;
        }

        if (useCase.Kind == MessageKind.Command && access.ImpersonatorUserId is not null && !access.ImpersonationAllowsWrite && !useCase.AllowWhenImpersonationReadOnly)
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
            // Contracts 查询不允许经 HTTP 暴露，HTTP 请求按无权限处理，其余来源视为编码错误
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
        metrics.AuthzDenied(reason);
        securityEvents.Write(new SecurityEvent("AuthorizationDenied", SecuritySeverity.Warning, reason)
        {
            Details = new Dictionary<string, string>(StringComparer.Ordinal) { ["useCase"] = useCase.Name },
        });
        return error;
    }
}
