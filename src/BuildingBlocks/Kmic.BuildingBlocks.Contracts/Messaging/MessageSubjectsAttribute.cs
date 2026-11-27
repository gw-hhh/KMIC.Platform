namespace Kmic.BuildingBlocks.Contracts.Messaging;

/// <summary>声明请求型消息关联的主体对象，接收方在删除对象前据此校验引用。</summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class MessageSubjectsAttribute(string propertyName) : Attribute
{
    /// <summary>主体对象 ID 属性名。</summary>
    public string PropertyName { get; } = propertyName;
}
