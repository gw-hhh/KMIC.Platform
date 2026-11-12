using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Errors;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.BuildingBlocks.Testing.Assertions;
using Kmic.BuildingBlocks.Testing.Database;
using Kmic.BuildingBlocks.Testing.Identity;
using Kmic.Sample.Application.Specimens.Close;
using Kmic.Sample.Application.Specimens.Create;
using Kmic.Sample.Application.Specimens.Delete;
using Kmic.Sample.Application.Specimens.Submit;
using Kmic.Sample.Domain.Errors;
using Kmic.Sample.Domain.Specimens;
using Shouldly;
using static Kmic.BuildingBlocks.Application.Access.ScopeLevel;

namespace Kmic.Api.Tests.Sample;

/// <summary>
/// 处理器的集成测试：以某个身份建立执行作用域，消息走完整管道（校验、授权、幂等、事务）落到真实数据库，不经过 HTTP
/// </summary>
public sealed class SpecimenCommandTests(ClassDatabase<ApiTemplate> database, ApiTemplate template)
    : ApiTestBase(database, template), IClassFixture<ClassDatabase<ApiTemplate>>
{
    private static readonly TestOrgTree _org = TestOrgTree.Create(t => t.Unit("总部"));

    private static readonly CreateSpecimenItemInput[] _oneItem = [new("样品", 2)];

    [Fact]
    public async Task Create_AssignsConsecutiveNumbersInTheOrderOfCreation()
    {
        var first = await CreateAsync(Alice);
        var second = await CreateAsync(Alice);

        first.Number.ShouldStartWith("SP-");
        int.Parse(second.Number[^4..], System.Globalization.CultureInfo.InvariantCulture)
            .ShouldBe(int.Parse(first.Number[^4..], System.Globalization.CultureInfo.InvariantCulture) + 1);
    }

    [Fact]
    public async Task Create_WithoutAnOrgUnit_FailsWithNoOrgUnitAndConsumesNoNumber()
    {
        var withoutUnit = TestIdentity.User("无组织").InTree(_org).WithRole(r => r.Permissions(Kmic.Sample.Application.SamplePermissions.Create).Scope(Self));
        Factory.Loader.Register(withoutUnit);
        var before = await CreateAsync(Alice);

        await using (var scope = As(withoutUnit))
        {
            scope.Requests.IdempotencyKey = Guid.CreateVersion7().ToString();
            var failed = await scope.Commands.SendAsync(new CreateSpecimenCommand("没有组织", _oneItem), Ct);
            failed.ShouldBeFailure(SampleErrors.NoOrgUnit);
        }

        // 失败的命令整个回滚，号码没有被消耗
        var after = await CreateAsync(Alice);
        Sequence(after.Number).ShouldBe(Sequence(before.Number) + 1);
    }

    [Fact]
    public async Task Submit_WithAStaleVersion_FailsWithVersionConflictCarryingTheCurrentVersion()
    {
        var created = await CreateAsync(Alice);
        await SubmitAsync(Alice, created);

        await using var scope = As(Alice);
        var result = await scope.Commands.SendAsync(new SubmitSpecimenCommand(created.Id, created.Version), Ct);

        var error = result.ShouldBeFailure(CommonErrors.VersionConflict);
        error.Metadata[ErrorMetadataKeys.CurrentVersion].ShouldBeOfType<long>().ShouldBeGreaterThan(created.Version);
    }

    [Fact]
    public async Task Close_BeforeItIsSubmitted_FailsWithInvalidState()
    {
        var created = await CreateAsync(Alice);

        await using var scope = As(Alice);
        var result = await scope.Commands.SendAsync(new CloseSpecimenCommand(created.Id, created.Version), Ct);

        result.ShouldBeFailure(SampleErrors.InvalidState);
    }

    [Fact]
    public async Task Submit_ByANonOwner_FailsWithNotOwner()
    {
        var created = await CreateAsync(Alice);

        await using var scope = As(Bob);
        var result = await scope.Commands.SendAsync(new SubmitSpecimenCommand(created.Id, created.Version), Ct);

        result.ShouldBeFailure(SampleErrors.NotOwner);
    }

    [Fact]
    public async Task Delete_AfterItIsSubmitted_FailsWithInvalidState()
    {
        var created = await CreateAsync(Alice);
        var submitted = await SubmitAsync(Alice, created);

        await using var scope = As(Alice);
        var result = await scope.Commands.SendAsync(new DeleteSpecimenCommand(created.Id, submitted.Version), Ct);

        result.ShouldBeFailure(SampleErrors.InvalidState);
    }

    private static long Sequence(string number) => long.Parse(number[^4..], System.Globalization.CultureInfo.InvariantCulture);

    private HandlerScope As(TestIdentity identity) => new(Factory.Services.GetRequiredService<IExecutionScopeFactory>().CreateScope(identity));

    private async Task<Created> CreateAsync(TestIdentity owner)
    {
        await using var scope = As(owner);
        scope.Requests.IdempotencyKey = Guid.CreateVersion7().ToString();
        var created = (await scope.Commands.SendAsync(new CreateSpecimenCommand("集成测试", _oneItem), Ct)).ShouldBeSuccess();
        var number = (await scope.Queries.SendAsync(new Kmic.Sample.Application.Specimens.GetDetail.GetSpecimenDetailQuery(created.Id), Ct)).ShouldBeSuccess().Number;
        return new Created(created.Id, created.ConcurrencyVersion, number);
    }

    private async Task<Created> SubmitAsync(TestIdentity owner, Created created)
    {
        await using var scope = As(owner);
        var submitted = (await scope.Commands.SendAsync(new SubmitSpecimenCommand(created.Id, created.Version), Ct)).ShouldBeSuccess();
        return created with { Version = submitted.ConcurrencyVersion };
    }

    private sealed record Created(SpecimenId Id, long Version, string Number);

    private sealed class HandlerScope(AsyncServiceScope scope) : IAsyncDisposable
    {
        public ICommandSender Commands => scope.ServiceProvider.GetRequiredService<ICommandSender>();

        public IQuerySender Queries => scope.ServiceProvider.GetRequiredService<IQuerySender>();

        public RequestMetadata Requests => scope.ServiceProvider.GetRequiredService<RequestMetadata>();

        public ValueTask DisposeAsync() => scope.DisposeAsync();
    }
}
