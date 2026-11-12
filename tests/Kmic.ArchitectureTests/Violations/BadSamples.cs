using System.Diagnostics.CodeAnalysis;
using FluentValidation;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Entities;
using Kmic.BuildingBlocks.Domain.Events;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Domain.Repositories;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.BuildingBlocks.Infrastructure.Files;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Hosting;

namespace Kmic.ArchitectureTests.Violations;

/// <summary>
/// 故意违规的样例：每条架构规则至少有一个，规则对它们必须报出违规，证明规则不会写错之后永远通过
/// </summary>
internal static class BadSamples
{
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

    // 匿名用例出现在 Identity 之外
    [Kmic.BuildingBlocks.Application.UseCases.AllowAnonymous]
    internal sealed record OpenDoorCommand : ICommand;
}
