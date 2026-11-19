namespace Kmic.BuildingBlocks.Application.Integration;

/// <summary>
/// 一条死信的全部内容：多了载荷和信封，要有权限才能看
/// </summary>
/// <param name="Message">死信的概要</param>
/// <param name="State">状态</param>
/// <param name="Payload">载荷</param>
/// <param name="Headers">信封里的其余字段</param>
public sealed record DeadMessageDetail(DeadMessage Message, DeadMessageState State, string Payload, string Headers);
