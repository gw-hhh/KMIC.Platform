namespace Kmic.BuildingBlocks.Domain.Results;

/// <summary>失败结果构造接口。</summary>
public interface IResultFactory<out TSelf>
    where TSelf : IResultFactory<TSelf>
{
    static abstract TSelf Fail(Error error);
}
