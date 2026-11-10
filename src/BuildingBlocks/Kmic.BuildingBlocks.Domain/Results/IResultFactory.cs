namespace Kmic.BuildingBlocks.Domain.Results;

/// <summary>
/// 让管道行为不知道具体响应类型也能构造失败结果
/// </summary>
/// <remarks>
/// Result 与 Result&lt;T&gt; 以显式接口实现，业务代码看不到这个入口，仍然用隐式转换返回错误
/// </remarks>
public interface IResultFactory<out TSelf>
    where TSelf : IResultFactory<TSelf>
{
    /// <summary>
    /// 构造一个失败结果
    /// </summary>
    static abstract TSelf Fail(Error error);
}
