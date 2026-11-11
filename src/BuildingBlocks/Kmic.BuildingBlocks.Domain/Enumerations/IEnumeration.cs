namespace Kmic.BuildingBlocks.Domain.Enumerations;

/// <summary>
/// 智能枚举的非泛型接口
/// </summary>
public interface IEnumeration
{
    /// <summary>
    /// 编码，发布后不能修改
    /// </summary>
    string Code { get; }

    /// <summary>
    /// 显示名称
    /// </summary>
    string Name { get; }
}
