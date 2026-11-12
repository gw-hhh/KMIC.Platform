using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Contracts.References;
using Kmic.BuildingBlocks.Domain.Errors;
using Kmic.BuildingBlocks.Testing.Assertions;
using Kmic.BuildingBlocks.Testing.Database;
using Kmic.BuildingBlocks.Testing.Identity;
using Kmic.Sample.Application.ContractQueries;
using Kmic.Sample.Application.Specimens.Close;
using Kmic.Sample.Application.Specimens.Create;
using Kmic.Sample.Application.Specimens.Delete;
using Kmic.Sample.Application.Specimens.GetDetail;
using Kmic.Sample.Application.Specimens.Submit;
using Kmic.Sample.Contracts;
using Kmic.Sample.Domain.Specimens;
using Shouldly;
using static Kmic.BuildingBlocks.Application.Access.ScopeLevel;

namespace Kmic.Api.Tests.Sample;

/// <summary>
/// 其他模块解析标本引用：按 ID 批量、不受调用方权限和数据范围限制、自动分批
/// </summary>
public sealed class SpecimenReferencesTests(ClassDatabase<ApiTemplate> database, ApiTemplate template)
    : ApiTestBase(database, template), IClassFixture<ClassDatabase<ApiTemplate>>
{
    private static readonly TestOrgTree _org = TestOrgTree.Create(t => t.Unit("总部"));

    private static readonly CreateSpecimenItemInput[] _oneItem = [new("样品", 2)];

    private static readonly TestIdentity _stranger = TestIdentity.User("路人").InTree(_org).InOrgUnit(_org["总部"]).WithRole(r => r.Scope(Self));

    [Fact]
    public async Task Resolve_ReturnsTitleNumberAndStatus_EvenWithoutPermissionOrDataScope()
    {
        Factory.Loader.Register(_stranger);
        var created = await CreateAsync(Alice, "第一号标本");

        await using var scope = As(_stranger);
        var found = await References(scope).ResolveAsync([created.Id.Value], Ct);

        var reference = found.ShouldHaveSingleItem().Value;
        reference.Id.ShouldBe(created.Id.Value);
        reference.DisplayName.ShouldBe("第一号标本");
        reference.Code.ShouldBe(created.Number);
        reference.Status.ShouldBe(ReferenceStatus.Active);
    }

    [Fact]
    public async Task Resolve_UnknownAndDeletedIds_AreAbsent_DuplicatesAreHarmless()
    {
        var kept = await CreateAsync(Alice, "保留");
        var removed = await CreateAsync(Alice, "删除");
        await using (var scope = As(Alice))
        {
            (await scope.Commands.SendAsync(new DeleteSpecimenCommand(removed.Id, removed.Version), Ct)).ShouldBeSuccess();
        }

        await using var caller = As(Alice);
        var found = await References(caller).ResolveAsync([kept.Id.Value, removed.Id.Value, Guid.CreateVersion7(), kept.Id.Value], Ct);

        found.Keys.ShouldBe([kept.Id.Value]);
    }

    [Fact]
    public async Task Resolve_ClosedSpecimen_IsDisabled()
    {
        var draft = await CreateAsync(Alice, "草稿");
        var closed = await CreateAsync(Alice, "已关闭");
        // 每个作用域一个幂等键，两个命令要分开
        long version;
        await using (var submit = As(Alice))
        {
            version = (await submit.Commands.SendAsync(new SubmitSpecimenCommand(closed.Id, closed.Version), Ct)).ShouldBeSuccess().ConcurrencyVersion;
        }

        await using (var close = As(Alice))
        {
            (await close.Commands.SendAsync(new CloseSpecimenCommand(closed.Id, version), Ct)).ShouldBeSuccess();
        }

        await using var caller = As(Alice);
        var found = await References(caller).ResolveAsync([draft.Id.Value, closed.Id.Value], Ct);

        found[draft.Id.Value].Status.ShouldBe(ReferenceStatus.Active);
        found[closed.Id.Value].Status.ShouldBe(ReferenceStatus.Disabled);
    }

    [Fact]
    public async Task Resolve_NoIds_ReturnsEmpty()
    {
        await using var scope = As(Alice);

        (await References(scope).ResolveAsync([], Ct)).ShouldBeEmpty();
    }

    [Fact]
    public async Task Resolve_MoreIdsThanOneBatch_AreSplitAutomatically()
    {
        var first = await CreateAsync(Alice, "甲");
        var second = await CreateAsync(Alice, "乙");
        var ids = new List<Guid>([first.Id.Value]);
        ids.AddRange(Enumerable.Range(0, 1500).Select(_ => Guid.CreateVersion7()));
        ids.Add(second.Id.Value);

        await using var scope = As(Alice);
        var found = await References(scope).ResolveAsync(ids, Ct);

        found.Keys.ShouldBe([first.Id.Value, second.Id.Value], ignoreOrder: true);
    }

    [Fact]
    public async Task Query_OverTheLimit_RejectedByValidation()
    {
        await using var scope = As(Alice);
        var tooMany = Enumerable.Range(0, 1001).Select(_ => SpecimenId.New()).ToArray();

        var result = await scope.ContractQueries.SendAsync(new GetSpecimenReferencesQuery(tooMany), Ct);

        result.ShouldBeFailure(CommonErrors.ValidationFailed);
    }

    [Fact]
    public async Task Query_SentOutsideTheContractSender_IsDenied()
    {
        var created = await CreateAsync(Alice, "只能经 Contracts 调用");

        await using var scope = As(Alice);
        var result = await scope.Queries.SendAsync(new GetSpecimenReferencesQuery([created.Id]), Ct);

        result.ShouldBeFailure(CommonErrors.MissingPermission);
    }

    private static ISpecimenReferences References(CallerScope scope) => scope.Services.GetRequiredService<ISpecimenReferences>();

    private CallerScope As(TestIdentity identity)
    {
        var scope = new CallerScope(Factory.Services.GetRequiredService<IExecutionScopeFactory>().CreateScope(identity));
        scope.Requests.IdempotencyKey = Guid.CreateVersion7().ToString();
        return scope;
    }

    private async Task<Created> CreateAsync(TestIdentity owner, string title)
    {
        await using var scope = As(owner);
        var created = (await scope.Commands.SendAsync(new CreateSpecimenCommand(title, _oneItem), Ct)).ShouldBeSuccess();
        var number = (await scope.Queries.SendAsync(new GetSpecimenDetailQuery(created.Id), Ct)).ShouldBeSuccess().Number;
        return new Created(created.Id, created.ConcurrencyVersion, number);
    }

    private sealed record Created(SpecimenId Id, long Version, string Number);

    private sealed class CallerScope(AsyncServiceScope scope) : IAsyncDisposable
    {
        public IServiceProvider Services => scope.ServiceProvider;

        public ICommandSender Commands => scope.ServiceProvider.GetRequiredService<ICommandSender>();

        public IQuerySender Queries => scope.ServiceProvider.GetRequiredService<IQuerySender>();

        public IContractQuerySender ContractQueries => scope.ServiceProvider.GetRequiredService<IContractQuerySender>();

        public RequestMetadata Requests => scope.ServiceProvider.GetRequiredService<RequestMetadata>();

        public ValueTask DisposeAsync() => scope.DisposeAsync();
    }
}
