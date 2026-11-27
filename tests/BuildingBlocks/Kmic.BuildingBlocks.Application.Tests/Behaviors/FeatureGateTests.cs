using Kmic.BuildingBlocks.Application.Access;
using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Tests.Fixtures;
using Kmic.BuildingBlocks.Domain.Errors;
using Kmic.BuildingBlocks.Domain.Results;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shouldly;

namespace Kmic.BuildingBlocks.Application.Tests.Behaviors;

public sealed class FeatureGateTests : IAsyncLifetime
{
    private readonly TestHost _host = new();

    public ValueTask InitializeAsync() => ValueTask.CompletedTask;

    public ValueTask DisposeAsync() => _host.DisposeAsync();

    private async Task<Result<string>> SendAsync(CallOrigin origin, ICurrentUser user, IAccessContext? access)
    {
        await using var scope = _host.Scope(origin, user, access);
        return await scope.ServiceProvider.GetRequiredService<IQuerySender>().SendAsync(new GatedThingQuery(), TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task WhenTheFeatureIsOff_TheHandlerDoesNotRun_AndTheCallerSeesNotFound()
    {
        var access = TestAccess.WithPermissions();

        var result = await SendAsync(CallOrigin.Http, CurrentUser.From(access), access);

        result.Error.ShouldBeSameAs(CommonErrors.FeatureDisabled);
        result.Error.Type.ShouldBe(ErrorType.NotFound);
        _host.Probe.Calls.ShouldBeEmpty();
    }

    [Fact]
    public async Task WhenTheFeatureIsOn_TheHandlerRuns()
    {
        _host.Features.On.Add(GatedThingQuery.Feature);
        var access = TestAccess.WithPermissions();

        var result = await SendAsync(CallOrigin.Http, CurrentUser.From(access), access);

        result.Value.ShouldBe("开放");
        _host.Probe.Calls.ShouldBe([(nameof(GatedThingQueryHandler), CallOrigin.Http)]);
    }

    [Fact]
    public async Task AFeatureThatIsOff_HidesTheUseCaseEvenFromSomeoneWhoWouldBeRefusedAnyway()
    {
        // 匿名访问同样返回“不存在”而非“无权限”：已关闭的功能不暴露其存在性
        var result = await SendAsync(CallOrigin.Http, CurrentUser.Anonymous, access: null);

        result.Error.ShouldBeSameAs(CommonErrors.FeatureDisabled);
    }

    [Theory]
    [InlineData(CallOrigin.Inbox)]
    [InlineData(CallOrigin.ScheduledJob)]
    [InlineData(CallOrigin.BackgroundJob)]
    public async Task ABackgroundEntry_DoesNotRunItEither_AndLogsInformation(CallOrigin origin)
    {
        var result = await SendAsync(origin, CurrentUser.System(), access: null);

        result.Error.ShouldBeSameAs(CommonErrors.FeatureDisabled);
        _host.Probe.Calls.ShouldBeEmpty();
        _host.Logs.GetSnapshot().ShouldContain(r => r.Level == LogLevel.Information && r.Message.Contains(GatedThingQuery.Feature, StringComparison.Ordinal));
    }

    [Fact]
    public async Task TheSwitchOnlyOpensTheFeature_PermissionsAreStillChecked()
    {
        _host.Features.On.Add(GatedThingQuery.Feature);

        // 功能开关已开启，但匿名用户仍无权访问：开关本身不授予权限
        var result = await SendAsync(CallOrigin.Http, CurrentUser.Anonymous, access: null);

        result.Error.ShouldBeSameAs(CommonErrors.MissingPermission);
    }
}
