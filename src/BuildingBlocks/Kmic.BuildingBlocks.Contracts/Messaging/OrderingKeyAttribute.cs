namespace Kmic.BuildingBlocks.Contracts.Messaging;

/// <summary>业务事实事件的顺序键（快照事件无需标注，默认以 SourceId 作为顺序键）。</summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class OrderingKeyAttribute(string propertyName) : Attribute
{
    /// <summary>顺序键属性名。</summary>
    public string PropertyName { get; } = propertyName;
}
