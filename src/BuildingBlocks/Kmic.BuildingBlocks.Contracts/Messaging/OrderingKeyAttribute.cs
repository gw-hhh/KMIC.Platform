namespace Kmic.BuildingBlocks.Contracts.Messaging;

/// <summary>
/// 业务事实事件的顺序键
/// </summary>
/// <remarks>
/// 快照事件不用标，默认按 SourceId
/// </remarks>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class OrderingKeyAttribute(string propertyName) : Attribute
{
    /// <summary>
    /// 作为顺序键的属性名
    /// </summary>
    public string PropertyName { get; } = propertyName;
}
