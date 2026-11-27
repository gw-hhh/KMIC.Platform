namespace Kmic.BuildingBlocks.Domain.Identifiers;

/// <summary>强类型 ID 接口（本模块实体标识以 Id 结尾，跨模块引用标识以 Ref 结尾）。</summary>
public interface IStronglyTypedId
{
    /// <summary>底层 Guid 值。</summary>
    Guid Value { get; }
}
