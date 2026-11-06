namespace Spike01.BuildingBlocks;

/// <summary>
/// 当前执行上下文
/// </summary>
public interface IExecutionContext
{
    CallOrigin Origin { get; }
}
