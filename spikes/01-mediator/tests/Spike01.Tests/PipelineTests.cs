using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Spike01.BuildingBlocks;
using Spike01.Customers;
using Spike01.Quotations;
using Xunit;

namespace Spike01.Tests;

public sealed class PipelineTests : IDisposable
{
    private static readonly Guid _customerId = new("019a5c3e-0000-7000-8000-000000000001");
    private static readonly Guid _quotationId = new("019a5c3e-0000-7000-8000-000000000002");

    private readonly ServiceProvider _provider = SpikeHost.Build();

    public void Dispose() => _provider.Dispose();

    [Fact]
    public async Task Command_with_response_passes_all_behaviors_in_declared_order()
    {
        await using var scope = _provider.CreateAsyncScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();

        var result = await sender.SendAsync(new CreateQuotationCommand("示例", _customerId), TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Version.ShouldBe(7);
        scope.ServiceProvider.GetRequiredService<PipelineTrace>().Entries.ShouldBe(
        [
            "Logging:CreateQuotationCommand",
            "Validation:CreateQuotationCommand",
            "Authorization:CreateQuotationCommand:Http",
            "Transaction:begin:CreateQuotationCommand",
            "Handler:CreateQuotationCommand:before:Http",
            // 跨模块 Contracts 查询：完整经过查询管道（不含事务），来源临时切换为 ModuleCall
            "Logging:GetSelectableCustomerQuery",
            "Validation:GetSelectableCustomerQuery",
            "Authorization:GetSelectableCustomerQuery:ModuleCall",
            "Handler:GetSelectableCustomerQuery:ModuleCall",
            // 调用返回后恢复为原来的来源
            "Handler:CreateQuotationCommand:after:Http",
            "Transaction:commit:CreateQuotationCommand",
        ]);
    }

    [Fact]
    public async Task Command_without_response_passes_transaction()
    {
        await using var scope = _provider.CreateAsyncScope();
        var sender = scope.ServiceProvider.GetRequiredService<ICommandSender>();

        var result = await sender.SendAsync(new CancelQuotationCommand(_quotationId), TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        scope.ServiceProvider.GetRequiredService<PipelineTrace>().Entries.ShouldBe(
        [
            "Logging:CancelQuotationCommand",
            "Validation:CancelQuotationCommand",
            "Authorization:CancelQuotationCommand:Http",
            "Transaction:begin:CancelQuotationCommand",
            "Handler:CancelQuotationCommand",
            "Transaction:commit:CancelQuotationCommand",
        ]);
    }

    [Fact]
    public async Task Query_skips_command_only_behavior()
    {
        await using var scope = _provider.CreateAsyncScope();
        var sender = scope.ServiceProvider.GetRequiredService<IQuerySender>();

        var result = await sender.SendAsync(new GetQuotationTitleQuery(_quotationId), TestContext.Current.CancellationToken);

        result.Value.ShouldBe("示例报价");
        scope.ServiceProvider.GetRequiredService<PipelineTrace>().Entries.ShouldBe(
        [
            "Logging:GetQuotationTitleQuery",
            "Validation:GetQuotationTitleQuery",
            "Authorization:GetQuotationTitleQuery:Http",
            "Handler:GetQuotationTitleQuery",
        ]);
    }

    [Fact]
    public async Task Validation_failure_returns_typed_failure_without_reaching_handler()
    {
        await using var scope = _provider.CreateAsyncScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();

        var result = await sender.SendAsync(new CreateQuotationCommand(" ", _customerId), TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBe(new Error("Quotations.TitleRequired", "Title is required."));
        scope.ServiceProvider.GetRequiredService<PipelineTrace>().Entries.ShouldBe(
        [
            "Logging:CreateQuotationCommand",
            "Validation:CreateQuotationCommand",
        ]);
    }

    [Fact]
    public async Task Contract_query_sent_directly_from_http_is_rejected()
    {
        await using var scope = _provider.CreateAsyncScope();
        var sender = scope.ServiceProvider.GetRequiredService<IQuerySender>();

        var result = await sender.SendAsync(new GetSelectableCustomerQuery(_customerId), TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeFalse();
        result.Error!.Code.ShouldBe("Common.ContractQueryOnlyFromModuleCall");
        scope.ServiceProvider.GetRequiredService<PipelineTrace>().Entries.ShouldNotContain(e => e.StartsWith("Handler:", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Origin_override_does_not_leak_between_concurrent_scopes()
    {
        var tasks = Enumerable.Range(0, 50).Select(async i =>
        {
            await using var scope = _provider.CreateAsyncScope();
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            var trace = scope.ServiceProvider.GetRequiredService<PipelineTrace>();
            if (i % 2 == 0)
            {
                (await sender.SendAsync(new CreateQuotationCommand("示例", _customerId), TestContext.Current.CancellationToken)).IsSuccess.ShouldBeTrue();
                trace.Entries.ShouldContain(e => string.Equals(e, "Handler:CreateQuotationCommand:after:Http", StringComparison.Ordinal));
            }
            else
            {
                (await sender.SendAsync(new GetQuotationTitleQuery(_quotationId), TestContext.Current.CancellationToken)).IsSuccess.ShouldBeTrue();
                trace.Entries.ShouldContain(e => string.Equals(e, "Authorization:GetQuotationTitleQuery:Http", StringComparison.Ordinal));
            }
        });

        await Task.WhenAll(tasks);
    }
}
