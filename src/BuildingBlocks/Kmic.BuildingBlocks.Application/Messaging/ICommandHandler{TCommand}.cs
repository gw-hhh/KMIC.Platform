using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.BuildingBlocks.Application.Messaging;

/// <summary>
/// 不返回值的命令处理器：只做编排，不保存、不开事务、不写 Outbox
/// </summary>
/// <remarks>
/// 处理器是 public sealed：Mediator 在宿主中生成的分发代码直接引用它（原型 #1）；除生成代码外任何类型不得引用处理器（架构测试）
/// </remarks>
public interface ICommandHandler<in TCommand> : Mediator.ICommandHandler<TCommand, Result>
    where TCommand : ICommand;
