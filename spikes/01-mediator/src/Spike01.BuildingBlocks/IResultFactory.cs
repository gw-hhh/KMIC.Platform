namespace Spike01.BuildingBlocks;

/// <summary>
/// 让管道行为可以泛型地构造失败结果（05-02 第 2.1 节）
/// </summary>
public interface IResultFactory<out TSelf>
    where TSelf : IResultFactory<TSelf>
{
    static abstract TSelf Fail(Error error);
}
