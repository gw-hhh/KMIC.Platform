namespace Kmic.BuildingBlocks.Application.Authorization;

public interface IAllowedActions<in TResource>
{
    /// <summary>返回当前用户对该数据可执行的动作编码。</summary>
    IReadOnlyList<string> For(TResource resource);
}
