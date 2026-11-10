namespace Kmic.BuildingBlocks.Domain.Enumerations;

/// <summary>
/// 智能枚举的非泛型视图，供状态机、EF 与 JSON 约定统一读取编码
/// </summary>
public interface IEnumeration
{
    /// <summary>
    /// 稳定编码，用于存储、JSON 与业务历史；一经发布不得修改
    /// </summary>
    string Code { get; }

    /// <summary>
    /// 默认的中文显示名
    /// </summary>
    string Name { get; }
}
