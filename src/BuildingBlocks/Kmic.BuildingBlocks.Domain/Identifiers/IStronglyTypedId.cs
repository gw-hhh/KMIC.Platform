namespace Kmic.BuildingBlocks.Domain.Identifiers;

/// <summary>
/// 强类型 ID 的标记，供 EF 约定、OpenAPI 架构转换器与架构测试识别
/// </summary>
/// <remarks>
/// 由 Vogen 生成的 readonly partial struct 实现，底层为 UUID v7；命名以 Id 结尾，跨模块引用以 Ref 结尾
/// </remarks>
public interface IStronglyTypedId
{
    /// <summary>
    /// 底层的 Guid 值
    /// </summary>
    Guid Value { get; }
}
