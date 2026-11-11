namespace Kmic.BuildingBlocks.Contracts.Messaging;

/// <summary>
/// 业务事实事件的顺序键
/// </summary>
/// <remarks>
/// 声明了有序处理的订阅，同一个键的消息按发布顺序处理。快照事件不用标，默认按 SourceId
/// </remarks>
/// <param name="propertyName">作为顺序键的属性，类型是 Guid 或 string</param>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class OrderingKeyAttribute(string propertyName) : Attribute
{
    /// <summary>
    /// 作为顺序键的属性名
    /// </summary>
    public string PropertyName { get; } = propertyName;
}
