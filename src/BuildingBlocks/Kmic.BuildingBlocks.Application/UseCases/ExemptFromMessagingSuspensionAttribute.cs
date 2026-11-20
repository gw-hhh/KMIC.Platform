namespace Kmic.BuildingBlocks.Application.UseCases;

/// <summary>
/// 消息写入被软线熔断时仍可执行
/// </summary>
/// <remarks>
/// 只给熔断期间也必须可用的命令：认证与会话、运维命令。硬线下仍然拒绝
/// </remarks>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class ExemptFromMessagingSuspensionAttribute : Attribute;
