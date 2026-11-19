namespace Kmic.BuildingBlocks.Contracts.Messaging;

/// <summary>
/// 请求型消息涉及的对象，接收方删除对象前按它探查
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class MessageSubjectsAttribute(string propertyName) : Attribute
{
    /// <summary>
    /// 存对象 ID 的属性名
    /// </summary>
    public string PropertyName { get; } = propertyName;
}
