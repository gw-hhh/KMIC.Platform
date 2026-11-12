using Kmic.BuildingBlocks.Application.Access;
using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Tests.Fixtures;
using Kmic.BuildingBlocks.Domain.Errors;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Domain.Results;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shouldly;

namespace Kmic.BuildingBlocks.Application.Tests.Behaviors;

/// <summary>
/// 通过 Mediator 生成的真实管道发送消息
/// </summary>
public sealed class PipelineTests : IAsyncLifetime
{
    private static readonly SubmitThingCommand _validSubmit = new("样品", [new ThingItem(1)]);

    private readonly TestHost _host = new();

    public ValueTask InitializeAsync() => ValueTask.CompletedTask;

    public ValueTask DisposeAsync() => _host.DisposeAsync();

    [Fact]
    public async Task Command_WithPermission_ReachesHandler()
    {
        var result = await SendAsync(TestAccess.WithPermissions(SubmitThingCommand.Permission), _validSubmit);

        result.IsSuccess.ShouldBeTrue();
        _host.Probe.Calls.ShouldBe([(nameof(SubmitThingCommandHandler), CallOrigin.Http)]);
        _host.SecurityEvents.Events.ShouldBeEmpty();
    }

    [Fact]
    public async Task Command_WithoutPermission_DeniedAndSecurityEventWritten()
    {
        var result = await SendAsync(TestAccess.WithPermissions("Test.Other"), _validSubmit);

        result.Error.ShouldBeSameAs(CommonErrors.MissingPermission);
        _host.Probe.Calls.ShouldBeEmpty();
        var denied = _host.SecurityEvents.Events.ShouldHaveSingleItem();
        denied.Type.ShouldBe("AuthorizationDenied");
        denied.Reason.ShouldBe("MissingPermission");
        denied.Details!["useCase"].ShouldBe("BuildingBlocks.SubmitThingCommand");
    }

    [Fact]
    public async Task Validation_RunsBeforeAuthorization_WithJsonPathsAndCodes()
    {
        // 没有权限点时也先返回校验错误，因为校验在授权之前
        var result = await SendAsync(TestAccess.WithPermissions(), new SubmitThingCommand("", [new ThingItem(1), new ThingItem(0)]));

        result.Error.Code.ShouldBe(CommonErrors.ValidationFailed.Code);
        result.Error.FieldErrors.Select(e => (e.Path, e.Code)).ShouldBe([("name", "Required"), ("items[1].quantity", "Range")]);
        result.Error.FieldErrors.ShouldAllBe(e => !string.IsNullOrWhiteSpace(e.Message));
        _host.SecurityEvents.Events.ShouldBeEmpty();
        _host.Probe.Calls.ShouldBeEmpty();
    }

    [Fact]
    public async Task PasswordChangeRequired_OnlyExemptUseCasesPass()
    {
        var access = TestAccess.WithPermissions(SubmitThingCommand.Permission) with { PasswordChangeRequired = true };

        (await SendAsync(access, _validSubmit)).Error.ShouldBeSameAs(CommonErrors.PasswordChangeRequired);
        (await SendAsync(access, new ChangePasswordCommand())).IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task ReadOnlyImpersonation_DeniesCommandsAllowsQueries()
    {
        var access = TestAccess.WithPermissions(SubmitThingCommand.Permission) with
        {
            ImpersonatorUserId = UserId.New(),
            ImpersonationAllowsWrite = false,
        };

        (await SendAsync(access, _validSubmit)).Error.ShouldBeSameAs(CommonErrors.ImpersonationReadOnly);
        (await QueryAsync(access, new GetThingQuery())).IsSuccess.ShouldBeTrue();
        _host.SecurityEvents.Events.ShouldHaveSingleItem().Reason.ShouldBe("ImpersonationReadOnly");
    }

    [Fact]
    public async Task SystemIdentity_SystemOnlyPasses_UndeclaredUseCaseThrows()
    {
        await using var scope = _host.Scope(CallOrigin.ScheduledJob, CurrentUser.System());
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();

        (await sender.SendAsync(new SyncThingCommand(), CancellationToken.None)).IsSuccess.ShouldBeTrue();
        await Should.ThrowAsync<InvalidOperationException>(async () => await sender.SendAsync(_validSubmit, CancellationToken.None));
    }

    [Fact]
    public async Task SystemOnly_FromUser_Denied()
    {
        var result = await SendAsync(TestAccess.WithPermissions(), new SyncThingCommand());

        result.Error.ShouldBeSameAs(CommonErrors.MissingPermission);
        _host.SecurityEvents.Events.ShouldHaveSingleItem().Reason.ShouldBe("SystemOnly");
    }

    [Fact]
    public async Task ContractQuery_OnlyThroughContractSender()
    {
        var access = TestAccess.WithPermissions();
        await using var scope = _host.UserScope(access);
        var provider = scope.ServiceProvider;

        var direct = await provider.GetRequiredService<IQuerySender>().SendAsync(new LookupThingQuery(), CancellationToken.None);
        var viaContract = await provider.GetRequiredService<IContractQuerySender>().SendAsync(new LookupThingQuery(), CancellationToken.None);

        direct.Error.ShouldBeSameAs(CommonErrors.MissingPermission);
        viaContract.Value.ShouldBe("引用");
        _host.Probe.Calls.ShouldBe([(nameof(LookupThingQueryHandler), CallOrigin.ModuleCall)]);
        provider.GetRequiredService<IExecutionContext>().Origin.ShouldBe(CallOrigin.Http);
    }

    [Fact]
    public async Task CommandInsideCommand_Throws_QueryInsideCommandAllowed()
    {
        var access = TestAccess.WithPermissions();

        (await SendAsync(access, new SendInsideCommand(SendCommand: false))).IsSuccess.ShouldBeTrue();
        await Should.ThrowAsync<InvalidOperationException>(async () => await SendAsync(access, new SendInsideCommand(SendCommand: true)));
    }

    [Fact]
    public async Task Logging_RecordsOutcome_AndWarnsWhenSlow()
    {
        var access = TestAccess.WithPermissions();

        await SendAsync(access, _validSubmit);
        await QueryAsync(access, new SlowThingQuery());

        var records = _host.Logs.GetSnapshot();
        records.ShouldContain(r => r.Id.Id == 1001 && r.Message.Contains("BuildingBlocks.SubmitThingCommand handled with Common.MissingPermission", StringComparison.Ordinal));
        records.ShouldContain(r => r.Id.Id == 1002 && r.Level == LogLevel.Warning && r.Message.Contains("BuildingBlocks.SlowThingQuery", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Logging_ScopeCarriesOriginUserModuleAndMessage_ForEveryLogInsideTheHandler()
    {
        var access = TestAccess.WithPermissions(SubmitThingCommand.Permission);

        (await SendAsync(access, _validSubmit)).IsSuccess.ShouldBeTrue();

        var handled = _host.Logs.GetSnapshot().Single(r => r.Id.Id == 1001 && r.Message.Contains("SubmitThingCommand", StringComparison.Ordinal));
        var properties = handled.Scopes.OfType<IReadOnlyDictionary<string, object?>>().Single();
        properties["Origin"].ShouldBe("Http");
        properties["Module"].ShouldBe("BuildingBlocks");
        properties["Message"].ShouldBe("BuildingBlocks.SubmitThingCommand");
        properties["UserId"].ShouldBe(access.UserId.Value);
        properties["IsSystem"].ShouldBe(false);
    }

    [Fact]
    public async Task Logging_SystemIdentity_IsMarkedAndHasNoUserId()
    {
        await using var scope = _host.Scope(CallOrigin.ScheduledJob, CurrentUser.System());
        (await scope.ServiceProvider.GetRequiredService<ISender>().SendAsync(new SyncThingCommand(), CancellationToken.None)).IsSuccess.ShouldBeTrue();

        var properties = _host.Logs.GetSnapshot().Last(r => r.Id.Id == 1001).Scopes.OfType<IReadOnlyDictionary<string, object?>>().Single();
        properties["Origin"].ShouldBe("ScheduledJob");
        properties["IsSystem"].ShouldBe(true);
        properties["UserId"].ShouldBeNull();
    }

    [Fact]
    public async Task ExecutionScope_InitializeTwice_Throws()
    {
        await using var scope = _host.UserScope(TestAccess.WithPermissions());
        var executionScope = scope.ServiceProvider.GetRequiredService<ExecutionScope>();

        Should.Throw<InvalidOperationException>(() => executionScope.Initialize(CallOrigin.Http, CurrentUser.System()));
    }

    private async Task<Result> SendAsync(IAccessContext access, ICommand command)
    {
        await using var scope = _host.UserScope(access);
        return await scope.ServiceProvider.GetRequiredService<ISender>().SendAsync(command, CancellationToken.None);
    }

    private async Task<Result<TResponse>> SendAsync<TResponse>(IAccessContext access, ICommand<TResponse> command)
    {
        await using var scope = _host.UserScope(access);
        return await scope.ServiceProvider.GetRequiredService<ISender>().SendAsync(command, CancellationToken.None);
    }

    private async Task<Result<TResponse>> QueryAsync<TResponse>(IAccessContext access, IQuery<TResponse> query)
    {
        await using var scope = _host.UserScope(access);
        return await scope.ServiceProvider.GetRequiredService<ISender>().SendAsync(query, CancellationToken.None);
    }
}
