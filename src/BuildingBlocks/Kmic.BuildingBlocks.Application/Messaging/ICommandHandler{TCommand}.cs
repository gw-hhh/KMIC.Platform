using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.BuildingBlocks.Application.Messaging;

/// <summary>
/// 不返回值的命令处理器
/// </summary>
/// <remarks>
/// 处理器要 public sealed，宿主里 Mediator 生成的代码直接引用它。处理器只编排，不保存、不开事务
/// </remarks>
public interface ICommandHandler<in TCommand> : Mediator.ICommandHandler<TCommand, Result>
    where TCommand : ICommand;
