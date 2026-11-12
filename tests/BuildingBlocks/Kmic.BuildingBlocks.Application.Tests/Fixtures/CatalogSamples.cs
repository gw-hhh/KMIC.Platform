using Kmic.BuildingBlocks.Application.Authorization;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.BuildingBlocks.Contracts.References;
using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.BuildingBlocks.Application.Tests.Fixtures;

/// <summary>
/// 故意写错声明的消息，只交给 UseCaseCatalog.Build 校验。Mediator 要求每个消息都有处理器，所以也一并提供
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

    [RequirePermission("Test.Thing.View")]
    public sealed record CheckNumberQuery : IQuery<bool>;

    [RequirePermission("Test.Thing.View")]
    public sealed record BlankBypassReasonQuery : IQuery<bool>;

    [BypassesDataScope("测试：不是处理器")]
    public sealed class NotAHandler;

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

    [BypassesDataScope("编号唯一性要在全部数据中判断")]
    public sealed class CheckNumberQueryHandler : IQueryHandler<CheckNumberQuery, bool>
    {
        public ValueTask<Result<bool>> Handle(CheckNumberQuery query, CancellationToken cancellationToken) => ValueTask.FromResult<Result<bool>>(false);
    }

    [BypassesDataScope(" ")]
    public sealed class BlankBypassReasonQueryHandler : IQueryHandler<BlankBypassReasonQuery, bool>
    {
        public ValueTask<Result<bool>> Handle(BlankBypassReasonQuery query, CancellationToken cancellationToken) => ValueTask.FromResult<Result<bool>>(false);
    }

    [ContractQuery(ContractAccess.Reference)]
    public sealed record ReferenceOfDictionaryQuery : IQuery<IReadOnlyDictionary<Guid, ReferenceInfo>>;

    [ContractQuery(ContractAccess.Reference)]
    public sealed record ReferenceOfArrayQuery : IQuery<ReferenceInfo[]>;

    [ContractQuery(ContractAccess.Reference)]
    public sealed record ReferenceOfListQuery : IQuery<IReadOnlyList<ReferenceInfo>>;

    [ContractQuery(ContractAccess.Reference, ResultExemption = "候选人员：调用方需要看到范围外的人，只返回编号和姓名")]
    public sealed record ReferenceExemptQuery : IQuery<string>;

    [ContractQuery(ContractAccess.CallerScoped)]
    public sealed record CallerScopedStringQuery : IQuery<string>;

    [ContractQuery(ContractAccess.Reference)]
    public sealed record ReferenceOfStringQuery : IQuery<string>;

    [ContractQuery(ContractAccess.Reference)]
    public sealed record ReferenceOfNamesQuery : IQuery<IReadOnlyDictionary<Guid, string>>;

    [ContractQuery(ContractAccess.Reference)]
    public sealed record ReferenceOfWrongKeyQuery : IQuery<IReadOnlyDictionary<ReferenceInfo, string>>;

    [ContractQuery(ContractAccess.Reference, ResultExemption = "多余的例外")]
    public sealed record ReferenceUselessExemptionQuery : IQuery<ReferenceInfo>;

    [ContractQuery(ContractAccess.Reference, ResultExemption = " ")]
    public sealed record ReferenceBlankExemptionQuery : IQuery<string>;

    [ContractQuery(ContractAccess.CallerScoped, ResultExemption = "不是引用解析")]
    public sealed record CallerScopedExemptionQuery : IQuery<string>;

    public sealed class ReferenceOfDictionaryQueryHandler : IQueryHandler<ReferenceOfDictionaryQuery, IReadOnlyDictionary<Guid, ReferenceInfo>>
    {
        public ValueTask<Result<IReadOnlyDictionary<Guid, ReferenceInfo>>> Handle(ReferenceOfDictionaryQuery query, CancellationToken cancellationToken) => throw new NotSupportedException();
    }

    public sealed class ReferenceOfArrayQueryHandler : IQueryHandler<ReferenceOfArrayQuery, ReferenceInfo[]>
    {
        public ValueTask<Result<ReferenceInfo[]>> Handle(ReferenceOfArrayQuery query, CancellationToken cancellationToken) => throw new NotSupportedException();
    }

    public sealed class ReferenceOfListQueryHandler : IQueryHandler<ReferenceOfListQuery, IReadOnlyList<ReferenceInfo>>
    {
        public ValueTask<Result<IReadOnlyList<ReferenceInfo>>> Handle(ReferenceOfListQuery query, CancellationToken cancellationToken) => throw new NotSupportedException();
    }

    public sealed class ReferenceExemptQueryHandler : IQueryHandler<ReferenceExemptQuery, string>
    {
        public ValueTask<Result<string>> Handle(ReferenceExemptQuery query, CancellationToken cancellationToken) => throw new NotSupportedException();
    }

    public sealed class CallerScopedStringQueryHandler : IQueryHandler<CallerScopedStringQuery, string>
    {
        public ValueTask<Result<string>> Handle(CallerScopedStringQuery query, CancellationToken cancellationToken) => throw new NotSupportedException();
    }

    public sealed class ReferenceOfStringQueryHandler : IQueryHandler<ReferenceOfStringQuery, string>
    {
        public ValueTask<Result<string>> Handle(ReferenceOfStringQuery query, CancellationToken cancellationToken) => throw new NotSupportedException();
    }

    public sealed class ReferenceOfNamesQueryHandler : IQueryHandler<ReferenceOfNamesQuery, IReadOnlyDictionary<Guid, string>>
    {
        public ValueTask<Result<IReadOnlyDictionary<Guid, string>>> Handle(ReferenceOfNamesQuery query, CancellationToken cancellationToken) => throw new NotSupportedException();
    }

    public sealed class ReferenceOfWrongKeyQueryHandler : IQueryHandler<ReferenceOfWrongKeyQuery, IReadOnlyDictionary<ReferenceInfo, string>>
    {
        public ValueTask<Result<IReadOnlyDictionary<ReferenceInfo, string>>> Handle(ReferenceOfWrongKeyQuery query, CancellationToken cancellationToken) => throw new NotSupportedException();
    }

    public sealed class ReferenceUselessExemptionQueryHandler : IQueryHandler<ReferenceUselessExemptionQuery, ReferenceInfo>
    {
        public ValueTask<Result<ReferenceInfo>> Handle(ReferenceUselessExemptionQuery query, CancellationToken cancellationToken) => throw new NotSupportedException();
    }

    public sealed class ReferenceBlankExemptionQueryHandler : IQueryHandler<ReferenceBlankExemptionQuery, string>
    {
        public ValueTask<Result<string>> Handle(ReferenceBlankExemptionQuery query, CancellationToken cancellationToken) => throw new NotSupportedException();
    }

    public sealed class CallerScopedExemptionQueryHandler : IQueryHandler<CallerScopedExemptionQuery, string>
    {
        public ValueTask<Result<string>> Handle(CallerScopedExemptionQuery query, CancellationToken cancellationToken) => throw new NotSupportedException();
    }
}
