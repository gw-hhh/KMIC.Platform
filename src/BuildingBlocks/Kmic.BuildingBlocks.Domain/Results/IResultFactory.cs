namespace Kmic.BuildingBlocks.Domain.Results;

/// <summary>
/// 失败结果工厂
/// </summary>
/// <remarks>
/// 显式实现，业务代码看不到它，返回错误仍用隐式转换
/// </remarks>
public interface IResultFactory<out TSelf>
    where TSelf : IResultFactory<TSelf>
{
    /// <summary>
    /// 构造失败结果
    /// </summary>
    static abstract TSelf Fail(Error error);
}
