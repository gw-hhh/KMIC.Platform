namespace Kmic.BuildingBlocks.Application.Authorization;

/// <summary>
/// 详情查询返回给前端的 allowedActions；前端只按它显示或禁用按钮
/// </summary>
public interface IAllowedActions<in TResource>
{
    /// <summary>
    /// 当前用户对这条数据可用的动作编码
    /// </summary>
    IReadOnlyList<string> For(TResource resource);
}
