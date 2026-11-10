using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.BuildingBlocks.Application.Tests.Fixtures;

/// <summary>
/// 故意写错声明的消息，只交给 UseCaseCatalog.Build 校验；Mediator 要求每个消息都有处理器，所以一并提供
/// </summary>
public static class CatalogSamples
{
    [NotAudited("测试")]
    public sealed record NoDeclarationCommand : ICommand;

    [AllowAuthenticated]
    [RequirePermission("Test.Thing.View")]
    public sealed record TwoDeclarationsQuery : IQuery<int>;

    [SystemOnly]
    [AllowSystem]
    [NotAudited("测试")]
    public sealed record SystemOnlyWithAllowSystemCommand : ICommand;

    [RequirePermission("Test.Thing.Edit")]
    [AllowWhenPasswordChangeRequired]
    [NotAudited("测试")]
    public sealed record PasswordExemptWithPermissionCommand : ICommand;

    [ContractQuery(ContractAccess.Reference)]
    [NotAudited("测试")]
    public sealed record ContractCommand : ICommand;

    [AllowAuthenticated]
    public sealed record UnauditedCommand : ICommand;

    [AllowAuthenticated]
    [Idempotent]
    [NotAudited("测试")]
    public sealed record IdempotentNotAuditedCommand : ICommand;

    [AllowAuthenticated]
    [Idempotent]
    public sealed record IdempotentQuery : IQuery<int>;

    public sealed class NoDeclarationCommandHandler : ICommandHandler<NoDeclarationCommand>
    {
        public ValueTask<Result> Handle(NoDeclarationCommand command, CancellationToken cancellationToken) => ValueTask.FromResult(Result.Success());
    }

    public sealed class TwoDeclarationsQueryHandler : IQueryHandler<TwoDeclarationsQuery, int>
    {
        public ValueTask<Result<int>> Handle(TwoDeclarationsQuery query, CancellationToken cancellationToken) => ValueTask.FromResult<Result<int>>(0);
    }

    public sealed class SystemOnlyWithAllowSystemCommandHandler : ICommandHandler<SystemOnlyWithAllowSystemCommand>
    {
        public ValueTask<Result> Handle(SystemOnlyWithAllowSystemCommand command, CancellationToken cancellationToken) => ValueTask.FromResult(Result.Success());
    }

    public sealed class PasswordExemptWithPermissionCommandHandler : ICommandHandler<PasswordExemptWithPermissionCommand>
    {
        public ValueTask<Result> Handle(PasswordExemptWithPermissionCommand command, CancellationToken cancellationToken) => ValueTask.FromResult(Result.Success());
    }

    public sealed class ContractCommandHandler : ICommandHandler<ContractCommand>
    {
        public ValueTask<Result> Handle(ContractCommand command, CancellationToken cancellationToken) => ValueTask.FromResult(Result.Success());
    }

    public sealed class UnauditedCommandHandler : ICommandHandler<UnauditedCommand>
    {
        public ValueTask<Result> Handle(UnauditedCommand command, CancellationToken cancellationToken) => ValueTask.FromResult(Result.Success());
    }

    public sealed class IdempotentNotAuditedCommandHandler : ICommandHandler<IdempotentNotAuditedCommand>
    {
        public ValueTask<Result> Handle(IdempotentNotAuditedCommand command, CancellationToken cancellationToken) => ValueTask.FromResult(Result.Success());
    }

    public sealed class IdempotentQueryHandler : IQueryHandler<IdempotentQuery, int>
    {
        public ValueTask<Result<int>> Handle(IdempotentQuery query, CancellationToken cancellationToken) => ValueTask.FromResult<Result<int>>(0);
    }
}
