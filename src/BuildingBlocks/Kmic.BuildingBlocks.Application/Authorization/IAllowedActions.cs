namespace Kmic.BuildingBlocks.Application.Authorization;

/// <summary>
/// 可用动作
/// </summary>
public interface IAllowedActions<in TResource>
{
    /// <summary>
    /// 当前用户对这条数据可用的动作编码
    /// </summary>
    IReadOnlyList<string> For(TResource resource);
}
