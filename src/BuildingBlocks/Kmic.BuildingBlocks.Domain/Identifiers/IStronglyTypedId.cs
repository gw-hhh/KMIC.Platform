namespace Kmic.BuildingBlocks.Domain.Identifiers;

/// <summary>
/// 强类型 ID
/// </summary>
/// <remarks>
/// 类型名以 Id 结尾，引用其他模块的以 Ref 结尾
/// </remarks>
public interface IStronglyTypedId
{
    /// <summary>
    /// Guid 值
    /// </summary>
    Guid Value { get; }
}
