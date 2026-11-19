namespace Kmic.BuildingBlocks.Contracts.Messaging;

/// <summary>
/// 请求型消息涉及的对象
/// </summary>
/// <remarks>
/// 只能标在请求型消息上。接收方删除对象之前，按它查还有没有涉及该对象的未完成请求
/// </remarks>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class MessageSubjectsAttribute(string propertyName) : Attribute
{
    /// <summary>
    /// 存对象 ID 的属性名
    /// </summary>
    public string PropertyName { get; } = propertyName;
}
