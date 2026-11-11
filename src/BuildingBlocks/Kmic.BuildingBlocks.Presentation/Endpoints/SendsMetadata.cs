namespace Kmic.BuildingBlocks.Presentation.Endpoints;

/// <summary>
/// 端点发送的消息，由 <c>Sends&lt;TMessage&gt;()</c> 写入端点元数据
/// </summary>
/// <param name="MessageType">命令或查询的类型</param>
public sealed record SendsMetadata(Type MessageType);
