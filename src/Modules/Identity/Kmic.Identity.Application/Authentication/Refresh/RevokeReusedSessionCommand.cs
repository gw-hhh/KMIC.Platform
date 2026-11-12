using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Identity.Application.Authentication.Refresh;

/// <summary>
/// 刷新令牌被重用之后，作废它所属的整个会话
/// </summary>
/// <remarks>
/// 刷新命令发现重用时返回失败，事务回滚，作废做不了，所以由接口层随后发这个命令单独完成。
/// 命令凭刷新令牌本身重新核实重用（旧令牌且已过宽限期），核实不通过什么也不改，所以不能被用来作废别人的会话
/// </remarks>
/// <param name="RefreshToken">客户端出示的刷新令牌</param>
[AllowAnonymous]
[NotAudited("结果记入安全事件")]
[NotIdempotent("重复执行没有额外效果：会话已经作废")]
public sealed record RevokeReusedSessionCommand(string RefreshToken) : ICommand;
