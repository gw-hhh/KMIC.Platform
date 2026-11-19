namespace Kmic.BuildingBlocks.Domain.Results;

/// <summary>
/// 失败结果工厂
/// </summary>
public interface IResultFactory<out TSelf>
    where TSelf : IResultFactory<TSelf>
{
    /// <summary>
    /// 构造失败结果
    /// </summary>
    static abstract TSelf Fail(Error error);
}
