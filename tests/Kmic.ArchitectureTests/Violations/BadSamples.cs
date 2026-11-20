using System.Diagnostics.CodeAnalysis;
using FluentValidation;
using Kmic.BuildingBlocks.Application.Access;
using Kmic.BuildingBlocks.Application.Authorization;
using Kmic.BuildingBlocks.Application.Caching;
using Kmic.BuildingBlocks.Application.Events;
using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Jobs;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Querying;
using Kmic.BuildingBlocks.Application.ReadModels;
using Kmic.BuildingBlocks.Application.Settings;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.BuildingBlocks.Contracts.References;
using Kmic.BuildingBlocks.Domain.Entities;
using Kmic.BuildingBlocks.Domain.Events;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Domain.Repositories;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.BuildingBlocks.Infrastructure.Files;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.Identity.Application.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Kmic.ArchitectureTests.Violations;

/// <summary>
/// 故意违规的样例：每条架构规则至少有一个，规则对它们必须报出违规，证明规则不会写错之后永远通过
/// </summary>
internal static class BadSamples
{
    // 带密码的 record 用了编译器生成的 ToString
    internal sealed record LeakyLogin(string Account, string Password);

    // 命令：名字不以 Command 结尾，也不是 record
    internal sealed class SubmitThing : ICommand;

    // 查询：名字不以 Query 结尾
    internal sealed record GetThing : IQuery<string>;

    // 处理器：internal，名字不以 Handler 结尾
    internal sealed class SubmitThingRunner : ICommandHandler<SubmitThing>
    {
        public ValueTask<Result> Handle(SubmitThing command, CancellationToken cancellationToken) => ValueTask.FromResult(Result.Success());
    }

    // 命令处理器注入了命令发送器
    public sealed class ChainingHandler(ICommandSender sender) : ICommandHandler<SubmitThing>
    {
        public ValueTask<Result> Handle(SubmitThing command, CancellationToken cancellationToken) => sender.SendAsync(command, cancellationToken);
    }

    // 校验器：public，注入了 DbContext
    public sealed class DatabaseValidator(FakeDb db) : AbstractValidator<FakeMessage>
    {
        public FakeDb Db => db;
    }

    internal sealed record FakeMessage(string Name);

    internal sealed class FakeDb(DbContextOptions<FakeDb> options) : DbContext(options);

    // 端点：public，名字不以 Endpoint 结尾，处理方法依赖了仓储
    public sealed class PublicThing : IEndpoint
    {
        public void Map(IEndpointRouteBuilder group) => group.MapGet("/x", Handle);

        private static string Handle(IRepository<FakeAggregate, FakeAggregateId> repository) => repository.ToString() ?? "";
    }

    // 领域事件：不是 record
    internal sealed class ThingHappened : IDomainEvent;

    // 强类型 ID：名字不以 Id 或 Ref 结尾
    internal readonly struct Widget : IStronglyTypedId
    {
        public Guid Value => Guid.Empty;
    }

    internal readonly struct FakeAggregateId : IStronglyTypedId
    {
        public Guid Value => Guid.Empty;
    }

    // 错误：静态类不叫 *Errors，错误码前缀不是模块名（KM0101 只许 *Errors 类调用 Error 的工厂方法，这里是故意违规）
    [SuppressMessage("Usage", "KM0101", Justification = "故意违规的样例，验证架构测试会失败")]
    internal static class ErrorHolder
    {
        public static readonly Error Wrong = Error.NotFound("Wrong.Thing", "没有这个东西");
    }

    // 可变静态字段
    internal static class SharedState
    {
        public static int Counter = 1;
    }

    // 异常类型
    internal sealed class ThingException : Exception;

    // 后台组件
    internal sealed class RogueWorker : BackgroundService
    {
        protected override Task ExecuteAsync(CancellationToken stoppingToken) => Task.CompletedTask;
    }

    // 并发原语
    internal sealed class Guarded : IDisposable
    {
        public readonly SemaphoreSlim Gate = new(1);

        public void Dispose() => Gate.Dispose();
    }

    // 聚合根持有另一个聚合根
    internal sealed class FakeAggregate : AggregateRoot<FakeAggregateId>
    {
        public FakeAggregate()
            : base(default)
        {
        }

        public FakeAggregate Parent { get; set; } = null!;
    }

    // 不是聚合根却可以软删除
    internal sealed class SoftLine : Entity<FakeAggregateId>, ISoftDeletable
    {
        public SoftLine()
            : base(default)
        {
        }
    }

    // 命令里出现了领域实体
    internal sealed record LeakyCommand(FakeAggregate Aggregate) : ICommand;

    // 不是 Files 模块的类型使用了文件存储
    internal sealed class FileTouchingService(IFileStorage storage)
    {
        public IFileStorage Storage { get; } = storage;
    }

    // 命令一侧的类型读缓存
    internal sealed class CachingService(HybridCache cache)
    {
        public HybridCache Cache { get; } = cache;
    }

    // 缓存键的第一段不是本模块名
    internal static class OrdersCacheKeys
    {
        public const string Prices = "Orders:Prices";

        public static string Customer(Guid id) => CacheKey.Of("Customer", "Profile", id);
    }

    // 在 CacheKeys 类之外拼缓存键
    internal sealed class KeyBuildingService
    {
        public static string Key(Guid id) => CacheKey.Of("Orders", "Thing", id);
    }

    // Identity 中需要权限点的命令，处理器没有继承授权管理命令基类
    [RequirePermission("Identity.Bad.Grant")]
    internal sealed record GrantEverythingCommand : ICommand;

    public sealed class GrantEverythingCommandHandler : ICommandHandler<GrantEverythingCommand>
    {
        public ValueTask<Result> Handle(GrantEverythingCommand command, CancellationToken cancellationToken) => ValueTask.FromResult(Result.Success());
    }

    // 合格：继承授权管理命令基类
    [RequirePermission("Identity.Bad.Rename")]
    internal sealed record RenameRoleSampleCommand(string Name) : ICommand<Guid>;

    public sealed class RenameRoleSampleCommandHandler(IOperatorRecheck recheck, IExecutionContext execution)
        : AuthorizationAdminCommandHandler<RenameRoleSampleCommand, Guid>(recheck, execution)
    {
        protected override ValueTask<Result<Guid>> ExecuteAsync(RenameRoleSampleCommand command, IAccessContext access, CancellationToken cancellationToken) =>
            ValueTask.FromResult<Result<Guid>>(Guid.Empty);
    }

    // 运维的“管理”命令没有声明熔断时仍可执行
    [RequirePermission("Operations.Bad.Manage")]
    internal sealed record RestartEverythingSampleCommand : ICommand;

    public sealed class RestartEverythingSampleCommandHandler : ICommandHandler<RestartEverythingSampleCommand>
    {
        public ValueTask<Result> Handle(RestartEverythingSampleCommand command, CancellationToken cancellationToken) => ValueTask.FromResult(Result.Success());
    }

    // 合格：声明了豁免
    [RequirePermission("Operations.Bad.Manage")]
    [ExemptFromMessagingSuspension]
    internal sealed record RetrySampleCommand : ICommand;

    public sealed class RetrySampleCommandHandler : ICommandHandler<RetrySampleCommand>
    {
        public ValueTask<Result> Handle(RetrySampleCommand command, CancellationToken cancellationToken) => ValueTask.FromResult(Result.Success());
    }

    // 合格：不是“管理”权限点
    [RequirePermission("Operations.Bad.View")]
    internal sealed record ViewSampleCommand : ICommand;

    public sealed class ViewSampleCommandHandler : ICommandHandler<ViewSampleCommand>
    {
        public ValueTask<Result> Handle(ViewSampleCommand command, CancellationToken cancellationToken) => ValueTask.FromResult(Result.Success());
    }

    // 应用层的处理器注入了不经缓存的设置读取
    public sealed class FreshSettingsHandler(IFreshSettings settings) : ICommandHandler<ViewSampleCommand>
    {
        public IFreshSettings Settings => settings;

        public ValueTask<Result> Handle(ViewSampleCommand command, CancellationToken cancellationToken) => ValueTask.FromResult(Result.Success());
    }

    // 匿名用例出现在 Identity 之外
    [Kmic.BuildingBlocks.Application.UseCases.AllowAnonymous]
    internal sealed record OpenDoorCommand : ICommand;

    // 命令响应：带嵌套对象（业务内容应当经查询获取）
    internal sealed record BadReceiptCommand(string Name) : ICommand<BadReceipt>;

    internal sealed record BadReceipt(Guid Id, ThingDetail Detail);

    internal sealed record ThingDetail(string Name, decimal Amount);

    // 命令响应：集合的元素不是收据
    internal sealed record BadReceiptItemsCommand(string Name) : ICommand<BadReceiptItems>;

    internal sealed record BadReceiptItems(IReadOnlyList<ThingDetail> Items);

    // 合格的收据：标识、版本号、逐条收据
    internal sealed record GoodReceiptCommand(string Name) : ICommand<GoodReceipt>;

    internal sealed record GoodReceipt(Guid Id, long Version, IReadOnlyList<GoodReceiptLine> Lines);

    internal sealed record GoodReceiptLine(Guid Id, string Code, bool Succeeded);

    // 系统专用命令不受收据规则限制
    [SystemOnly]
    internal sealed record SystemOutcomeCommand(string Name) : ICommand<BadReceipt>;

    // 引用了另一个命令处理器
    internal sealed class HandlerUser(ChainingHandler other)
    {
        public ChainingHandler Other => other;
    }

    // 领域事件处理器注入了别的模块的服务
    internal interface IOtherModuleThing;

    internal sealed class BadEventHandler(IOtherModuleThing other) : IDomainEventHandler<ThingHappened>
    {
        public IOtherModuleThing Other => other;

        public ValueTask<Result> HandleAsync(ThingHappened domainEvent, CancellationToken ct) => ValueTask.FromResult(Result.Success());
    }

    // 服务注册写在了模块注册类之外
    internal static class RogueRegistration
    {
        public static IServiceCollection AddThing(IServiceCollection services) => services;
    }
    // 状态机声明有误：类型初始化时抛异常
    internal sealed class BrokenAggregate : AggregateRoot<FakeAggregateId>
    {
        public static readonly int Broken = Fail();

        public BrokenAggregate()
            : base(default)
        {
        }

        private static int Fail() => throw new InvalidOperationException("状态机没有初始状态");
    }

    // 命令处理器注入了查询入口
    internal interface IThingQueryDb;

    public sealed class QueryingHandler(IThingQueryDb db) : ICommandHandler<SubmitThing>
    {
        public IThingQueryDb Db => db;

        public ValueTask<Result> Handle(SubmitThing command, CancellationToken cancellationToken) => ValueTask.FromResult(Result.Success());
    }

    // 作业处理器注入了 DbContext
    internal sealed record ThingJob : IBackgroundJob;

    internal sealed class DatabaseJobHandler(FakeDb db) : IJobHandler<ThingJob>
    {
        public FakeDb Db => db;

        public Task<Result<JobOutcome>> ExecuteAsync(ThingJob job, IJobContext context, CancellationToken ct) => throw new NotSupportedException();
    }

    // 资源策略依赖了用户偏好
    internal sealed class PreferenceAwarePolicy(IPreferences preferences) : IResourcePolicy<FakeAggregate, string>
    {
        public IPreferences Preferences => preferences;

        public PolicyDecision Evaluate(IAccessContext access, FakeAggregate resource, string action) => PolicyDecision.Allow;
    }

    // 敏感权限点没有标 Sensitive：物理删除
    [PermissionGroup("Bad", "示例")]
    internal static class BadPermissions
    {
        [Permission("物理删除")]
        public const string Purge = "Bad.Thing.Purge";

        [Permission("查看")]
        public const string View = "Bad.Thing.View";

        [Permission("导出全部", Sensitive = true)]
        public const string ExportAll = "Bad.Thing.ExportAll";
    }

    // 读模型同时是聚合根，可以被仓储和 SaveChanges 写入
    internal sealed class BadReadModel : AggregateRoot<FakeAggregateId>, IReadModel
    {
        public BadReadModel()
            : base(default)
        {
        }

        Guid IReadModel.Id => Id.Value;

        public long SourceVersion => 0;

        public ReferenceStatus Status => default;

        public bool IsTerminal => false;

        public ReadModelTerminalReason? TerminalReason => null;

        public DateTimeOffset? TerminalAt => null;

        public Guid? RebuildRunId => null;

        public DateTimeOffset SyncedAt => default;
    }

    // 创建类命令既没有 [Idempotent] 也没有 [NotIdempotent]
    internal sealed record CreateThingCommand(string Name) : ICommand;

    // 豁免要写明原因
    [NotIdempotent("天然幂等")]
    internal sealed record SubmitThingAgainCommand(string Name) : ICommand;

    // 校验器注入了 Contracts 里的类型
    internal interface IThingReferences;

    public sealed class ReferencingValidator(IThingReferences references) : AbstractValidator<FakeMessage>
    {
        public IThingReferences References => references;
    }

    // 绕过数据范围：处理器没有标 [BypassesDataScope]，调用藏在 async 方法的状态机里
    internal sealed record CountAllQuery(IQueryable<FakeAggregate> Source) : IQuery<int>;

    public sealed class UnmarkedBypassHandler : IQueryHandler<CountAllQuery, int>
    {
        public async ValueTask<Result<int>> Handle(CountAllQuery query, CancellationToken cancellationToken)
        {
            await Task.Yield();
            return query.Source.WithoutDataScope().Count();
        }
    }

    // 标了 [BypassesDataScope] 并写明原因：合格
    [BypassesDataScope("编号要在全部数据中判断是否重复")]
    public sealed class MarkedBypassHandler : IQueryHandler<CountAllQuery, int>
    {
        public ValueTask<Result<int>> Handle(CountAllQuery query, CancellationToken cancellationToken) =>
            ValueTask.FromResult<Result<int>>(query.Source.WithoutDataScope().Count());
    }

    // 不是处理器的类型调用了 WithoutDataScope
    internal sealed class ScopeHelper
    {
        public static int CountAll(IQueryable<FakeAggregate> source) => source.WithoutDataScope().Count();
    }

    // 缓存值：可变的类和不可变的记录
    internal sealed class MutableValue
    {
        public string Name { get; set; } = "";

        public List<string> Tags { get; } = [];
    }

    internal sealed record FrozenValue(string Name, IReadOnlyList<string> Tags);

    internal sealed class CachingQueries(HybridCache cache)
    {
        public ValueTask<MutableValue> MutableAsync(CancellationToken ct) =>
            cache.GetOrCreateAsync("Bad:mutable", _ => ValueTask.FromResult(new MutableValue()), cancellationToken: ct);

        public ValueTask<FrozenValue> FrozenAsync(CancellationToken ct) =>
            cache.GetOrCreateAsync("Bad:frozen", _ => ValueTask.FromResult(new FrozenValue("a", [])), cancellationToken: ct);
    }
}
